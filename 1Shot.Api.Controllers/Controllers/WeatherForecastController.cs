using Gizmo.DAL;
using Microsoft.AspNetCore.Mvc;

namespace _1Shot.Api.Controllers.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IGizmoDbContextProvider gizmoDbContextProvider , ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _gizmoDbContextProvider = gizmoDbContextProvider;
        }

        private readonly IGizmoDbContextProvider _gizmoDbContextProvider;

        [HttpGet]
        public IEnumerable<object> Get()
        {
            using (var dbContext = _gizmoDbContextProvider.GetDbNonProxyContext())
            {
               var apps = dbContext.QueryableSet<Gizmo.DAL.Entities.App>();
                return apps.Select(a => new { Title = a.Title}).ToArray();

            }


            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
