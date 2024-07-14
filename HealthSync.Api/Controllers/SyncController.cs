using HealthSync.Api.Auth;
using HealthSync.Api.Models;
using HealthSync.BackgroundServices.Services;
using HealthSync.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace HealthSync.Api.Controllers
{
	/// <summary>
	/// Controller for handling synchronization operations in the HealthSync API.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="SyncController"/> class.
	/// </remarks>
	/// <param name="syncService">The sync service for executing sync tasks.</param>
	[ApiController]
	[ApiKeyAuth]
	[Route("api/[controller]")]
	[Produces(MediaTypeNames.Application.Json)]
	[Consumes(MediaTypeNames.Application.Json)]
	public class SyncController(SyncBackgroundService syncService) : ControllerBase
	{
		private readonly SyncBackgroundService _syncService = syncService;

		/// <summary>
		/// Performs a manual synchronization operation.
		/// </summary>
		/// <param name="request">The manual synchronization request details.</param>
		/// <param name="cancellationToken">A token to cancel the operation if needed.</param>
		/// <returns>An IActionResult representing the result of the synchronization operation.</returns>
		[HttpPost("manual")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
		public async Task<IActionResult> ManualSync([FromBody] ManualSyncRequest request, CancellationToken cancellationToken = default)
		{
			var task = CreateSyncTask(request);
			var result = await _syncService.ExecuteSyncTaskAsync(task, cancellationToken);
			return result.IsSuccess ? Ok() : throw new SyncResultException(result);
		}

		/// <summary>
		/// Create async task from manual synchronization request.
		/// </summary>
		/// <param name="request">The manual synchronization request details.</param>
		/// <returns>The generated synchronization task.</returns>
		private static SyncTask CreateSyncTask(ManualSyncRequest request)
		{
			return new SyncTask
			{
				Start = request.Start,
				End = request.End,
				Index = request.Index,
				Input = new PluginConfig
				{
					Meta = request.Provider.Meta ?? [],
					Plugin = request.Provider.Plugin,
				},
				Output = new PluginConfig
				{
					Meta = request.Repository.Meta ?? [],
					Plugin = request.Repository.Plugin,
				}
			};
		}
	}
}