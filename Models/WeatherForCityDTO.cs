namespace APIAggregationAssignment.Models
{
    public class WeatherForCityDTO
    {
        public string City { get; set; }
        public CoordinatesDTO Coordinates { get; set; }
        public string Description { get; set; }
        public double Temperature { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
}
