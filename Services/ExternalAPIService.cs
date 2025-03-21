using System.Text.Json;
using APIAggregationAssignment.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Serilog;

namespace APIAggregationAssignment.Services
{
    //implementation of IExternalAPIService interface, fetches the data from external APIs
    public class ExternalAPIService : IExternalAPIService
    {
        //injecting HttpClient to be able to perform GET operations, as a private field (will provide access to it in the constructor)
        private readonly HttpClient _httpClient;

        //injecting ILogger to log errors. I'm using Serilog and Serilog.Sinks.File
        private readonly ILogger<ExternalAPIService> _logger;

        //injecting IMemoryCache for caching
        private readonly IMemoryCache _cache;

        public ExternalAPIService(HttpClient httpClient, ILogger<ExternalAPIService> logger, IMemoryCache cache)
        {
            //providing access to the private _httpClient field via a public property
            _httpClient = httpClient;
            //providing access to the private _logger field via a public property
            _logger = logger;
            //providing access to the private _cache field via a public property
            _cache = cache;

            //timeout the call if there's been no response after 5 seconds
            _httpClient.Timeout = TimeSpan.FromSeconds(5);

        }

        //method that accesses an API via the given url and returns either the result or an error message
        private async Task<object> FetchDataAsync(string url)
        {
            //try to get a response three times, return error details if can't succeed
            int maxTryCount = 3;
            int tryCount = 0;

            while (tryCount < maxTryCount)
            {
                tryCount++;
                try
                {
                    //save the API's response to the response variable
                    var response = await _httpClient.GetStringAsync(url);
                    //structure the JSON string that's returned in a JSON form for easier processing. Also "using" ensures that when the code's done with the object, it will be removed from memory
                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    if (url.Contains("openweathermap"))
                    {
                        return new WeatherForCityDTO
                        {
                            City = root.GetProperty("name").GetString(),
                            Coordinates = new CoordinatesDTO
                            {
                                Latitude = root.GetProperty("coord").GetProperty("lat").GetDouble(),
                                Longitude = root.GetProperty("coord").GetProperty("lon").GetDouble()
                            },
                            Description = root.GetProperty("weather")[0].GetProperty("main").GetString(),
                            Temperature = root.GetProperty("main").GetProperty("temp").GetDouble() - 273.15,
                            TempMin = root.GetProperty("main").GetProperty("temp_min").GetDouble() - 273.15,
                            TempMax = root.GetProperty("main").GetProperty("temp_max").GetDouble() - 273.15,
                            Pressure = root.GetProperty("main").GetProperty("pressure").GetInt32(),
                            Humidity = root.GetProperty("main").GetProperty("humidity").GetInt32(),
                            WindSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble()
                        };
                    }
                    else if (url.Contains("jsonplaceholder.typicode.com/posts"))
                    {
                        // Deserialize the response into a list of posts
                        var posts = JsonSerializer.Deserialize<List<JsonPlaceholderDTO>>(response);
                        return new JsonPlaceholderResponseDTO { Posts = posts };
                    }
                    else if (url.Contains("reqres.in/api/users"))
                    {
                        // Deserialize the response into the UsersResponseDTO (supports paginated data)
                        var usersResponse = JsonSerializer.Deserialize<UsersResponseDTO>(response);
                        return usersResponse;
                    }
                    else
                    {
                        //if it's not one of the known and handled APIs just return the raw JSON response as is
                        return response;
                    }
                }
                catch (HttpRequestException ex)
                {
                    //returning error details
                    string errorMessage = $"Error with {url}.\nError: {ex.Message}";
                    //Console.WriteLine(errorMessage); //better to log this to a file probably instead
                    _logger.LogError(ex, errorMessage);
                }
            }
            string maxTriesError = $"Error fetching from {url}, tried {tryCount} times.";
            _logger.LogError(maxTriesError);
            return maxTriesError;
        }

        //method that accesses each API that corresponds to a given URL (from the provided list argument), gets the response and returns a dictionary with a key-value-pair of URL: response
        public async Task<Dictionary<string, object>> GetDataFromAPIsAsync(List<string> APIURLs)
        {
            // Ensure URLs are sorted before generating the cache key. Otherwise, the same URLs in a different order would constitute a different cacheKey
            var sortedURLs = APIURLs.OrderBy(url => url).ToList();
            string cacheKey = $"APIResponse_{string.Join("_", sortedURLs)}";

            // Check if the data exists in cache
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, object> cachedData))
            {
                return cachedData; // Return cached data if available
            }
            //if cacheKey doesn't find anything, the requests go through
            //starts the API calls asyncronously since the FetchDataAsync method is Async. I'm using sortedURLs instead of APIURLs because the cacheKey will be the URL list, sorted. So the responses should also be sorted based on their URLs so that matching is correct
            var tasks = sortedURLs.ToDictionary(url => url, url => FetchDataAsync(url));

            //makes the method wait for all the API responses to come through before continuing with the rest of the code
            var results = await Task.WhenAll(tasks.Values);

            //this changes the tasks from key: String, value: Task, to key: String, value: Task.Result, so that what's returned as the value of each key-value-pair is the result of the API call
            var responseDictionary = tasks.Keys.Zip(results, (key, result) => new { key, result }).ToDictionary(kvp => kvp.key, kvp => kvp.result);

            //caches the aggregated response with a sliding expiration of 45 seconds and an absolute expiration of 5 minutes. Sliding is refreshed every time it's accessed, as long as it hasn't already expired. Absolute expires after set duration regardless of being accessed
            _cache.Set(cacheKey, responseDictionary, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(45),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return responseDictionary;
        }
    }
}
