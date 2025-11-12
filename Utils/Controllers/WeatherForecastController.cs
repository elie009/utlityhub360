using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Utils.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            string result = MockTest();

            return null;
        }

        private string MockTest()
        {

            var json = System.IO.File.ReadAllText(@"D:/calories2.json");

            string DishName = "";
            double maxTotal = 0;
            double curTotal = 0;

            var objects = JArray.Parse(json); // parse as array  
            foreach (JObject root in objects)
            {

                var xappName = (Object)root;
                var x = (Object)root;

                foreach (KeyValuePair<String, JToken> app in root)
                {
                    var appName = app.Key;
                    var var = (String)app.Value;


                    //var strProtein = (String)app.Value["protein"];
                    //var strFat = (String)app.Value["fat"];
                    //var strCarbs = (String)app.Value["carbs"];

                    //double protein = double.Parse(strProtein);
                    //double fat = double.Parse(strFat);
                    //double carbs = double.Parse(strCarbs);

                    //curTotal = protein + fat + carbs;

                    //if(curTotal > maxTotal)
                    //{
                    //    DishName = (String)app.Value["name"]; 
                    //    maxTotal = curTotal;
                    //}
                }
            }
            return DishName + " - " + curTotal;
        }
    }

}