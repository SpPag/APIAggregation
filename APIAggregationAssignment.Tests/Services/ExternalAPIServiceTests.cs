using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using APIAggregationAssignment.Models;
using APIAggregationAssignment.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using FakeItEasy;
using APIAggregationAssignment.Tests.Helpers; // still used for faking ILogger, etc.
using Microsoft.Extensions.Logging;

public class ExternalAPIServiceTests
{
    [Fact]
    public async Task ExternalAPIService_GetDataFromAPIsAsync_ReturnsWeatherData_ForOpenWeatherUrl()
    {
        // Arrange: Create a sample JSON response for OpenWeather API
        var weatherJson = @"{
            ""name"": ""Athens"",
            ""coord"": { ""lat"": 37.9795, ""lon"": 23.7162 },
            ""weather"": [{ ""main"": ""Clouds"" }],
            ""main"": { ""temp"": 285.65, ""temp_min"": 284.15, ""temp_max"": 287.15, ""pressure"": 1031, ""humidity"": 50 },
            ""wind"": { ""speed"": 4.92 }
        }";

        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(weatherJson)
        };

        //uses our FakeHttpMessageHandler instead of trying to fake a protected method
        var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
        var fakeHttpClient = new HttpClient(fakeHandler);

        //fakes the logger using FakeItEasy
        var fakeLogger = A.Fake<ILogger<ExternalAPIService>>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new ExternalAPIService(fakeHttpClient, fakeLogger, memoryCache);

        var apiUrl = "http://api.openweathermap.org/data/2.5/weather?q=Athens,gr&appid=DummyKey";

        // Act
        var result = await service.GetDataFromAPIsAsync(new List<string> { apiUrl });

        // Assert
        Assert.True(result.ContainsKey(apiUrl));
        var weatherResult = result[apiUrl] as WeatherForCityDTO;
        Assert.NotNull(weatherResult);
        Assert.Equal("Athens", weatherResult.City);
        Assert.Equal("Clouds", weatherResult.Description);
    }
}
