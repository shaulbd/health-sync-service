using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HealthSync.Api.Auth
{
    public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var settings = context.HttpContext.RequestServices.GetRequiredService<ApiSettings>();
            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                return;

            }
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!settings.ApiKey.Equals(potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

}
