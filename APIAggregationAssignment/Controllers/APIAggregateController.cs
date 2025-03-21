using APIAggregationAssignment.Models;
using APIAggregationAssignment.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIAggregationAssignment.Controllers
{
    [ApiController]
    [Route("api/aggregate")]
    public class APIAggregateController : ControllerBase
    {
        //injecting my IExternalAPIService as a private field (will provide access to it in the constructor)
        private readonly IExternalAPIService _externalAPIService;

        //providing access to the private _externalAPIService field via a public property
        public APIAggregateController(IExternalAPIService externalAPIService)
        {
            _externalAPIService = externalAPIService;
        }

        [HttpPost]
        public async Task<IActionResult> GetAggregatedData([FromBody] APIURLsRequest request, [FromQuery] string sortBy = null, string filterBy = null)
        {
            //checks if no API URLs have been provided. If so returns a bad request status code with the below message
            if (request.APIURLs == null || request.APIURLs.Count == 0)
            {
                string errorMessage = "Please provide at least one API's URL.";
                return BadRequest(new { error = errorMessage });
            }

            try
            {
                //save the API call response in the "result" variable
                var result = await _externalAPIService.GetDataFromAPIsAsync(request.APIURLs);

                //filter logic
                if (!String.IsNullOrEmpty(filterBy))
                {
                    var filteredResult = new Dictionary<string, object>();

                    foreach (var kvp in result)
                    {
                        // For string responses
                        if (kvp.Value is string strValue)
                        {
                            if (strValue.Contains(filterBy, StringComparison.OrdinalIgnoreCase))
                                filteredResult.Add(kvp.Key, kvp.Value);
                        }
                        // For Weather API responses (WeatherForCityDTO)
                        else if (kvp.Value is WeatherForCityDTO weather)
                        {
                            if (weather.City.Contains(filterBy, StringComparison.OrdinalIgnoreCase) ||
                                weather.Description.Contains(filterBy, StringComparison.OrdinalIgnoreCase))
                            {
                                filteredResult.Add(kvp.Key, kvp.Value);
                            }
                        }
                        // For JSONPlaceholder posts (List<JsonPlaceholderDTO>)
                        else if (kvp.Value is JsonPlaceholderResponseDTO jsonPlaceholderResponse)
                        {
                            var filteredPosts = jsonPlaceholderResponse.Posts
                                .Where(post => post.Title.Contains(filterBy, StringComparison.OrdinalIgnoreCase) ||
                                               post.Body.Contains(filterBy, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (filteredPosts.Any())
                            {
                                //create a new response DTo with only the filtered posts
                                var filteredJsonPlaceholderResponse = new JsonPlaceholderResponseDTO
                                {
                                    Posts = filteredPosts
                                };

                                filteredResult.Add(kvp.Key, filteredJsonPlaceholderResponse);
                            }
                                
                        }
                        // For Reqres users (UsersResponseDTO)
                        else if (kvp.Value is UsersResponseDTO usersResponse)
                        {
                            var filteredUsers = usersResponse.Data
                                .Where(user => user.FirstName.Contains(filterBy, StringComparison.OrdinalIgnoreCase) ||
                                               user.LastName.Contains(filterBy, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (filteredUsers.Any())
                            {
                                // Create a new UsersResponseDTO with only the matching users.
                                var filteredResponse = new UsersResponseDTO
                                {
                                    Page = usersResponse.Page,
                                    PerPage = usersResponse.PerPage,
                                    Total = usersResponse.Total,
                                    TotalPages = usersResponse.TotalPages,
                                    Data = filteredUsers
                                };

                                filteredResult.Add(kvp.Key, filteredResponse);
                            }
                        }
                    }
                    result = filteredResult;
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    var sortedResult = new Dictionary<string, object>();

                    foreach (var kvp in result)
                    {
                        // For JSONPlaceholder posts
                        if (kvp.Value is JsonPlaceholderResponseDTO placeholderResponse)
                        {
                            // Attempt to get a property from JsonPlaceholderDTO matching sortBy (case-insensitive)
                            var propertyInfo = typeof(JsonPlaceholderDTO).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (propertyInfo != null)
                            {
                                placeholderResponse.Posts = placeholderResponse.Posts
                                    .OrderBy(post => propertyInfo.GetValue(post))
                                    .ToList();
                            }
                            sortedResult.Add(kvp.Key, placeholderResponse);
                        }
                        // For Reqres users
                        else if (kvp.Value is UsersResponseDTO usersResponse)
                        {
                            // Attempt to get a property from UserDTO matching sortBy (case-insensitive)
                            var propertyInfo = typeof(UserDTO).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (propertyInfo != null)
                            {
                                usersResponse.Data = usersResponse.Data
                                    .OrderBy(user => propertyInfo.GetValue(user))
                                    .ToList();
                            }
                            sortedResult.Add(kvp.Key, usersResponse);
                        }
                        // For other types (like WeatherForCityDTO) or if property not found, leave unchanged.
                        else
                        {
                            sortedResult.Add(kvp.Key, kvp.Value);
                        }
                    }
                    result = sortedResult;
                }


                if (result == null || result.Count == 0)
                {
                    string errorMessage = "No data was found.";
                    return NotFound(new { error = errorMessage });
                }
                return Ok(new APIAggregateDTO { APIResponses = result });
            }
            catch (HttpRequestException ex)
            {
                //503: Service Unavailable
                string errorMessage = $"External API request failed due to {ex.Message}";
                return StatusCode(503, new
                {
                    error = errorMessage
                });
            }
            catch (Exception ex)
            {
                //Internal Server Error
                string errorMessage = $"Unexpected error occurred: {ex.Message}";
                return StatusCode(500, new { error = errorMessage });
            }
        }
    }
}
