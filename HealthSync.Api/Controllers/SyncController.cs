using HealthSync.Api.Auth;
using HealthSync.Api.Models;
using HealthSync.BackgroundServices.Services;
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
		/// <param name="request">The sync request.</param>
		/// <param name="cancellationToken">A token to cancel the operation if needed.</param>
		/// <returns>An IActionResult representing the result of the synchronization operation.</returns>
		[HttpPost("{taskId}/manual")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
		public async Task<IActionResult> ManualSync(ManualSyncRequest request, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _syncService.ExecuteSyncTaskAsync(request.TaskId, request.Start, request.End, cancellationToken);
			return result.IsSuccess ? Ok() : throw new SyncResultException(result);
		}


		/// <summary>
		/// Querying last sync timestamp from linked repository.
		/// </summary>
		/// <param name="request">The sync request.</param>
		/// <param name="cancellationToken">A token to cancel the operation if needed.</param>
		/// <returns>An IActionResult representing the result of the synchronization operation.</returns>
		[HttpGet("{taskId}/lastSync")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DateTimeOffset?))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
		public async Task<IActionResult> LastSync(LastSyncRequest request, CancellationToken cancellationToken = default)
		{
			var result = await _syncService.GetLastSync(request.TaskId, cancellationToken);
			return Ok(result);
		}
	}
}