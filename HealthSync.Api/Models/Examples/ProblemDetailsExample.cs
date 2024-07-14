using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace HealthSync.Api.Models.Examples
{
    public class ProblemDetailsExample : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                Title = "Sync Error",
                Status = 400,
                Detail = "A detailed description of the error",
                Instance = "/path/to/resource",
                Extensions = new Dictionary<string, object?>() {
                    { "trace-id" ,"Trace id"} ,
                    { "nodeId" ,"Machine name"}
                },
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2"
			};
        }
    }

}
