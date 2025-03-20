# APIAggregation
This is a simple API Aggregation service. It's currently setup to deserialize and nicely present the responses from three APIs:
  - https://jsonplaceholder.typicode.com/posts
  - http://api.openweathermap.org/data/2.5/weather?q=Athens,gr&appid=fdbd122fc2168fc5916ab13827182b58
  - https://reqres.in/api/users

Example of POST request & request body:
url: https://localhost:7103/api/aggregate (the 4 digit number after localhost: is the port on which the service is locally hosted on the user's pc
request body (where YourAppId is your token for the openweathermap API: {

    "APIURLs": [
    "https://jsonplaceholder.typicode.com/posts",
    "http://api.openweathermap.org/data/2.5/weather?q=Athens,gr&appid={YourAppId}",
    "https://reqres.in/api/users"
    ]
}

Any other API will return the response in raw JSON as it's configured to.

Should more API responses need to be deserialized and more nicely presented, following my approach, the respective DTOs would need to be created, the ExternalAPIService class' FetchDataAsync() method as well as APIAggregateController's GetAggregatedData() method would need to have an additional else if statement for each additional API.

There is some error handling capability. Each provided API URL will be called three times and after the last one fails, an appropriate error will be returned.

There is some filtering capability. The query can accept the filterBy parameter, based on which it will return the appropriate data.

Required dependencies
- .NET SDK 8 or later
- Serilog, Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File for logging
