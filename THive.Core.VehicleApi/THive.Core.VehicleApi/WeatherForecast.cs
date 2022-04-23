using THive.Core.DeviceApi.Models;

namespace THive.Core.DeviceApi
{
    public class WeatherForecast : IApiModel<DateTime>
    {
        public DateTime Id { get => Date; set => Date = value; }
        public int Version { get; set; }



        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}