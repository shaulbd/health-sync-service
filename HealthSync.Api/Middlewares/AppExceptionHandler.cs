using HealthSync.BackgroundServices;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Middlewares
{

	/// <summary>
	/// Application Exception handler
	/// </summary>
	/// <param name="logger">Logger</param>
	/// <param name="problemDetailsService">Problem details service</param>
	public class AppExceptionHandler(ILogger<AppExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
	{
		private readonly ILogger<AppExceptionHandler> _logger = logger;
		private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;


		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			// Handling sync result errors
			if (exception is SyncResultException syncException)
			{
				var syncResult = syncException.SyncResult;
				var statusCode = syncResult.Result switch
				{
					SyncResultStatus.Success => StatusCodes.Status200OK,
					SyncResultStatus.PartialSuccess => StatusCodes.Status207MultiStatus,
					SyncResultStatus.AlreadyRunning => StatusCodes.Status409Conflict,
					SyncResultStatus.TaskNotFound => StatusCodes.Status404NotFound,
					_ => StatusCodes.Status500InternalServerError,
				};

				var problem = new ProblemDetails
				{
					Status = statusCode,
					Title = "Sync Error",
					Detail = syncResult.Message
				};

				problem.Extensions["errors"] = syncResult.Errors;

				_logger.LogError(exception, message: exception.Message);

				var problemContext = new ProblemDetailsContext
				{
					HttpContext = httpContext,
					ProblemDetails = problem
				};

				httpContext.Response.StatusCode = statusCode;
				httpContext.Response.ContentType = "application/problem+json";
				await _problemDetailsService.WriteAsync(problemContext);

				return true;
			}
			return false;
		}
	}

}
