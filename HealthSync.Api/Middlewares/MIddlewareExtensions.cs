namespace HealthSync.Api.Middlewares
{
	public static class MiddlewareExtensions
	{
		public static void AddErrorHandler(this IServiceCollection services)
		{
			services.AddProblemDetails(options =>
				options.CustomizeProblemDetails = ctx =>
				{
					ctx.ProblemDetails.Extensions.Add("trace-id", ctx.HttpContext.TraceIdentifier);
					ctx.ProblemDetails.Extensions.Add("instance", $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}");
					ctx.ProblemDetails.Extensions.Add("nodeId", Environment.MachineName);
				});
			services.AddExceptionHandler<AppExceptionHandler>();
		}
	}
}
