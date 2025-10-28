using Microsoft.AspNetCore.Mvc;

namespace Utils.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            MockTest();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private void MockTest()
        {

            System.Diagnostics.Debug.WriteLine("test--------------------------");

            solution("10:00", "13:21");
            solution("09:42", "11:42");

        }

        //entrance = 2
        //full or partial = 3
        //successive = 4

        //10 - 13:21
        //[2 + 3] + 4 + 4 + 4
        //[2 + 3] + 4


        //1
        //10:00 - 13: 21

        //2
        //09:42 - 11:42

        public int solution(string E, string L)
        {
            // Implement your solution here

            Console.WriteLine(E);
            Console.WriteLine(L);

            TimeSpan start = TimeSpan.Parse(E);
            TimeSpan end = TimeSpan.Parse(L);

            TimeSpan totalHr = end - start;
            double dblTotalHrs = totalHr.TotalHours;

            Console.WriteLine(dblTotalHrs);

            if (dblTotalHrs < 1)
                return 5;
            else
            {
                int pCount = (int)Math.Ceiling(dblTotalHrs);
                return 5 + (pCount * 4);

            }

        }


        public string solution(int X)
        {
            if (X <= 0)
                return "0s";

            long remaining = X;
            string result = "";
            int unitsUsed = 0;

            // loop date unit
            foreach (var unit in Units)
            {
                long count = remaining / unit.Seconds;
                if (count > 0)
                {
                    remaining %= unit.Seconds;

                    // round the second unit if leftover seconds exist
                    if (unitsUsed == 1 && remaining > 0)
                        count++;

                    result += $"{count}{unit.Abbr}";
                    unitsUsed++;

                    // Only two units
                    if (unitsUsed == 2)
                        break;
                }
            }

            // if no unit was used (X < 1)
            if (result == "")
                result = $"{X}s";

            return result;
        }

        private struct TimeUnit
        {
            public int Seconds { get; }
            public string Abbr { get; }

            public TimeUnit(int seconds, string abbr)
            {
                Seconds = seconds;
                Abbr = abbr;
            }
        }

        private readonly TimeUnit[] Units = new TimeUnit[]
        {
            new TimeUnit(604800, "w"),
            new TimeUnit(86400, "d"),
            new TimeUnit(3600, "h"),
            new TimeUnit(60, "m"),
            new TimeUnit(1, "s")
        };

        public int suliton1(int N)
        {
            return 1;
        }

    }

}