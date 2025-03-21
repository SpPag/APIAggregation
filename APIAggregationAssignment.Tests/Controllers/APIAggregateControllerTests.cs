using APIAggregationAssignment.Controllers;
using APIAggregationAssignment.Models;
using APIAggregationAssignment.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace APIAggregationAssignment.Tests.Controllers
{
    public class APIAggregateControllerTests
    {
        [Fact]
        public async Task APIAggregateController_GetAggregatedData_ReturnsBadRequest_WhenNoAPIURLsProvided()
        {
            // Arrange
            var fakeService = A.Fake<IExternalAPIService>();
            var controller = new APIAggregateController(fakeService);
            var request = new APIURLsRequest { APIURLs = new List<string>() };

            // Act
            var result = await controller.GetAggregatedData(request, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task APIAggregateController_GetAggregatedData_ReturnsAggregatedData_WhenServiceReturnsData()
        {
            // Arrange: Create fake aggregated data for the test
            var fakeData = new Dictionary<string, object>
            {
                { "https://jsonplaceholder.typicode.com/posts", new JsonPlaceholderResponseDTO
                    { Posts = new List<JsonPlaceholderDTO>
                        { new JsonPlaceholderDTO { Id = 1, Title = "Test Post", Body = "Test Body", UserId = 1 } }
                    }
                }
            };

            var fakeService = A.Fake<IExternalAPIService>();
            A.CallTo(() => fakeService.GetDataFromAPIsAsync(A<List<string>>.Ignored))
                .Returns(Task.FromResult(fakeData));
            var controller = new APIAggregateController(fakeService);
            var request = new APIURLsRequest
            {
                APIURLs = new List<string> { "https://jsonplaceholder.typicode.com/posts" }
            };

            // Act
            var result = await controller.GetAggregatedData(request, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var aggregatedDTO = Assert.IsType<APIAggregateDTO>(okResult.Value);
            Assert.Equal(fakeData, aggregatedDTO.APIResponses);
        }

        [Fact]
        public async Task APIAggregateController_GetAggregatedData_ReturnsNotFound_WhenFilterResultsEmpty()
        {
            // Arrange: Fake service returns a response that doesn't match the filter "Holt"
            var fakeData = new Dictionary<string, object>
            {
                { "https://reqres.in/api/users", new UsersResponseDTO
                    { Data = new List<UserDTO>
                        { new UserDTO { FirstName = "John", LastName = "Doe", Id = 1, Email = "john@example.com" } }
                    }
                }
            };

            var fakeService = A.Fake<IExternalAPIService>();
            A.CallTo(() => fakeService.GetDataFromAPIsAsync(A<List<string>>.Ignored))
                .Returns(Task.FromResult(fakeData));
            var controller = new APIAggregateController(fakeService);
            var request = new APIURLsRequest { APIURLs = new List<string> { "https://reqres.in/api/users" } };

            // Act: Apply a filter that does not match the existing data
            var result = await controller.GetAggregatedData(request, null, "Holt");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
