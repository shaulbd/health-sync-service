

namespace HealthSync.Api
{
	public class CorsSettings
	{
		public List<string> AllowedOrigins { get; set; } = [];
	}

	public class ApiSettings
	{
		public string ApiKey { get; set; }
		public bool EnableSwagger { get; set; } = false;
		public CorsSettings CorsSettings { get; set; } = new();
	}
}
