using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApolloBus.InterfacesAbstraction;
using MicroserviceA.Event;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroserviceA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IApolloBus _ApolloBus;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IApolloBus ApolloBus)
        {
            _logger = logger;
            _ApolloBus = ApolloBus;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };



        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();

            var eventq = new EventFromMicroserviceA()
            {
                Name = "teste01",
                Message= Summaries[rng.Next(Summaries.Length)],

            };
            _ApolloBus.Publish(eventq);

            var eventA = new EventFromMicroserviceA()
            {
                Name = "teste02",
                Message = Summaries[rng.Next(Summaries.Length)],

            };
            _ApolloBus.PublishDelay(eventA, 2);


            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
