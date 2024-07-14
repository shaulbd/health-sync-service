using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HealthSync.Api.Auth
{
	public static class AuthExtensions
	{
		public static void AddSwaggerAuth(this SwaggerGenOptions swaggerOptions, string key)
		{
			// Enable API Key auth if set
			if (!string.IsNullOrEmpty(key))
			{
				swaggerOptions.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
				{
					Description = "API Key Authentication",
					Type = SecuritySchemeType.ApiKey,
					Name = "X-API-Key",
					In = ParameterLocation.Header,
					Scheme = "ApiKeyScheme"
				});

				var scheme = new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "ApiKey"
					},
					In = ParameterLocation.Header
				};

				var requirement = new OpenApiSecurityRequirement
				{
					{ scheme, new List<string>() }
				};

				swaggerOptions.AddSecurityRequirement(requirement);
			}
		}
	}
}
