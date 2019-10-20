using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RxStoreDemos.Blazor.FSharp.State;
using static RxStoreDemos.Blazor.FSharp.State.Weather;

namespace RxStoreDemos.Blazor.FSharp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly WeatherSummary[] Summaries = new[]
        {
            WeatherSummary.Freezing,
            WeatherSummary.Bracing,
            WeatherSummary.Chilly,
            WeatherSummary.Cool,
            WeatherSummary.Mild,
            WeatherSummary.Warm,
            WeatherSummary.Balmy,
            WeatherSummary.Hot,
            WeatherSummary.Sweltering,
            WeatherSummary.Scorching
        };

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast(
                DateTime.Now.AddDays(index),
                rng.Next(-20, 55),
                Summaries[rng.Next(Summaries.Length)]
            ))
            .ToArray();
        }
    }
}
