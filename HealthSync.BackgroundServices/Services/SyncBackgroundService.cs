using Cronos;
using HealthSync.Core;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using HealthSync.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthSync.BackgroundServices.Services
{
	/// <summary>
	/// Background service responsible for executing synchronization tasks based on configured schedules.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the SyncBackgroundService class.
	/// </remarks>
	/// <param name="logger"></param>
	/// <param name="configuration"></param>
	/// <param name="semaphoreManager"></param>
	/// <param name="pluginFactory"></param>
	public class SyncBackgroundService(
		ILogger<SyncBackgroundService> logger,
		SyncConfiguration configuration,
		SemaphoreManager semaphoreManager,
		PluginFactory pluginFactory) : BackgroundService
	{
		private readonly ILogger<SyncBackgroundService> _logger = logger;
		private readonly SyncConfiguration _configuration = configuration;
		private readonly PluginFactory _pluginFactory = pluginFactory;
		private readonly SemaphoreManager _semaphoreManager = semaphoreManager;
		private readonly TimeSpan _waitTime = TimeSpan.FromMinutes(1);

		/// <summary>
		/// Executes the background service, scheduling and running sync tasks.
		/// </summary>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				// Create a list to hold all the task executions
				var taskExecutions = new List<Task>();

				// Schedule all tasks concurrently
				foreach (var task in _configuration.Sync)
				{
					// Start the task execution and add it to the list
					taskExecutions.Add(ScheduleAndExecuteTask(task, stoppingToken));
				}

				// Await all scheduled tasks to complete concurrently
				await Task.WhenAll(taskExecutions);

				// Delay before the next iteration, respecting cancellation token
				await Task.Delay(_waitTime, stoppingToken);
			}
		}

		/// <summary>
		/// Schedules and executes a single sync task.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		private async Task ScheduleAndExecuteTask(SyncTask task, CancellationToken stoppingToken)
		{
			var cronExpression = CronExpression.Parse(task.Cron);
			var nextOccurrence = cronExpression.GetNextOccurrence(DateTime.UtcNow);

			if (nextOccurrence.HasValue)
			{
				var delay = nextOccurrence.Value - DateTime.UtcNow;
				if (delay > TimeSpan.Zero)
				{
					await Task.Delay(delay, stoppingToken);
				}

				await ExecuteSyncTaskAsync(task, stoppingToken);
			}
		}

		/// <summary>
		/// Executes a sync task and returns the result.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		public async Task<SyncResult> ExecuteSyncTaskAsync(SyncTask task, CancellationToken stoppingToken)
		{
			var syncContext = SyncContext.Create(task);
			var semaphore = _semaphoreManager.GetSemaphore(syncContext.SyncId);
			using var scope = _logger.BeginScope(new LoggerScope(syncContext.SyncId));

			if (!await semaphore.WaitAsync(0, stoppingToken))
			{
				_logger.LogWarning("Sync already in progress ");
				return new SyncResult(SyncResultStatus.AlreadyRunning, "Sync already in progress.");
			}

			try
			{
				return await ExecuteSyncLogic(task, syncContext, stoppingToken);
			}
			catch (SyncException ex)
			{
				LogSyncException(ex);
				return new SyncResult(ex.SuccessfulChunks == 0 ? SyncResultStatus.Error : SyncResultStatus.PartialSuccess, 
					ex.Message, ex.Exceptions?.Select(f => f.Message).ToList());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error during sync");
				return new SyncResult(SyncResultStatus.Error, $"Sync failed with error: {ex.Message}");
			}
			finally
			{
				semaphore.Release();
			}
		}

		/// <summary>
		/// Executes the core sync logic for a task.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="syncContext"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		private async Task<SyncResult> ExecuteSyncLogic(SyncTask task, SyncContext syncContext, CancellationToken stoppingToken)
		{
			_logger.LogDebug("Creating provider and repository plugins");
			var providerPlugin = _pluginFactory.CreatePlugin<IProviderPlugin>(task.Input.Plugin, syncContext);
			var repositoryPlugin = _pluginFactory.CreatePlugin<IRepositoryPlugin>(task.Output.Plugin, syncContext);

			await ValidateRepositoryStatus(repositoryPlugin, stoppingToken);
			var (start, end) = await DetermineSyncRange(repositoryPlugin, task, stoppingToken);
			var ranges = start.ChunkRange(end, _configuration.MaxSyncSize).ToList();

			if (ranges.Count == 0)
			{
				throw new InvalidOperationException("No date ranges to process.");
			}

			return await ProcessSyncRanges(ranges, task, providerPlugin, repositoryPlugin, stoppingToken);
		}

		/// <summary>
		/// Validates the repository status.
		/// </summary>
		/// <param name="repositoryPlugin"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		private async Task ValidateRepositoryStatus(IRepositoryPlugin repositoryPlugin, CancellationToken stoppingToken)
		{
			var status = await repositoryPlugin.GetStatus(stoppingToken);
			if (!status)
			{
				throw new InvalidOperationException("Repository is OFFLINE!");
			}
		}

		/// <summary>
		/// Determines the sync range based on the last sync data.
		/// </summary>
		/// <param name="repositoryPlugin"></param>
		/// <param name="task"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		private async Task<(DateTimeOffset start, DateTimeOffset end)> DetermineSyncRange(IRepositoryPlugin repositoryPlugin, SyncTask task, CancellationToken stoppingToken)
		{
			DateTimeOffset start;

			if (task.Start.HasValue)
			{
				start = task.Start.Value;
			}
			else
			{
				var lastSync = await repositoryPlugin.GetLastSyncData(task.Index, stoppingToken);
				if (lastSync == null)
				{
					_logger.LogInformation("Last sync not available for task. Will request last 24 hours");
				}
				start = lastSync ?? DateTimeOffset.Now.AddDays(-1);
			}

			var end = task.End ?? DateTimeOffset.UtcNow;
			return (start, end);
		}

		/// <summary>
		/// Processes sync ranges and returns the overall result.
		/// </summary>
		/// <param name="ranges"></param>
		/// <param name="task"></param>
		/// <param name="providerPlugin"></param>
		/// <param name="repositoryPlugin"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		/// <exception cref="SyncException"></exception>
		private async Task<SyncResult> ProcessSyncRanges(List<(DateTimeOffset Start, DateTimeOffset End)> ranges, SyncTask task, IProviderPlugin providerPlugin, IRepositoryPlugin repositoryPlugin, CancellationToken stoppingToken)
		{
			var exceptions = new List<Exception>();
			var successfulChunks = 0;

			for (var i = 0; i < ranges.Count; i++)
			{
				var (start, end) = ranges[i];
				var part = $"{i + 1}/{ranges.Count}";
				try
				{
					await ProcessSyncChunk(task, providerPlugin, repositoryPlugin, start, end, part, stoppingToken);
					successfulChunks++;
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
					_logger.LogError(ex, "Error during sync in chunk {Part}", part);
				}
			}

			if (exceptions.Count > 0)
			{
				throw new SyncException(
					exceptions.Count == ranges.Count
						? $"All {ranges.Count} chunks failed during sync"
						: $"Partial success: {successfulChunks} out of {ranges.Count} chunks completed successfully",
					exceptions, successfulChunks, ranges.Count);
			}

			return new SyncResult(SyncResultStatus.Success);
		}

		/// <summary>
		/// Processes a single sync chunk.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="providerPlugin"></param>
		/// <param name="repositoryPlugin"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="part"></param>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		private async Task ProcessSyncChunk(SyncTask task, IProviderPlugin providerPlugin, IRepositoryPlugin repositoryPlugin, DateTimeOffset start, DateTimeOffset end, string part, CancellationToken stoppingToken)
		{
			_logger.LogInformation("Processing chunk {Chunk} for range {Start} - {End}", part, start, end);

			_logger.LogDebug("({Part}) Fetching health data from provider...", part);
			var healthData = await providerPlugin.GetAsync(start, end, task.Index, stoppingToken);

			_logger.LogDebug("({Part}) Pushing health data to repository...", part);
			await repositoryPlugin.PushAsync(task.Index, healthData, stoppingToken);

			_logger.LogInformation("({Part}) Sync completed", part);
		}

		/// <summary>
		/// Logs details of a SyncException.
		/// </summary>
		/// <param name="ex"></param>
		private void LogSyncException(SyncException ex)
		{
			_logger.LogError(ex, "Error during sync: {Message}. Successful chunks: {SuccessfulChunks}/{TotalChunks}",
				ex.Message, ex.SuccessfulChunks, ex.TotalChunks);
			//foreach (var innerEx in ex.InnerExceptions)
			//{
			//	_logger.LogError(innerEx, "Inner exception during sync");
			//}
		}
	}
}