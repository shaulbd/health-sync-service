using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HealthSync.Api.Auth
{
	public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
	{
		private const string ApiKeyHeaderName = "X-API-Key";

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
			var apiKey = configuration["API_KEY"];

			if (string.IsNullOrEmpty(apiKey))
			{
				return;
			}

			if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			if (!apiKey.Equals(potentialApiKey))
			{
				context.Result = new UnauthorizedResult();
			}
		}
	}
}
