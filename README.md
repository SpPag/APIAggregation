# APIAggregation
This is a simple API Aggregation service. It's currently set up to deserialize and format the responses from three APIs:

  • `https://jsonplaceholder.typicode.com/posts`
  
  • `http://api.openweathermap.org/data/2.5/weather?q=Athens,gr&appid=fdbd122fc2168fc5916ab13827182b58`
  
  • `https://reqres.in/api/users`


Endpoint:

• URL: `https://localhost:{localhostPort}/api/aggregate` (replace {localhostPort} with the actual port number on which your service is running locally (for example, 7103))

• Method: POST

• Description: Aggregates data from multiple external APIs, formats, and returns the data.

• Request Body: The body should be a JSON object containing a list of API URLs to fetch.
  
  {
  
      "APIURLs": [
      "https://jsonplaceholder.typicode.com/posts",
      "http://api.openweathermap.org/data/2.5/weather?q=Athens,gr&appid={YourAppId}",
      "https://reqres.in/api/users"
      ]
  }

• Query Parameters: You can use the filterBy query parameter to filter results. Example: /api/aggregate?filterBy=Holt

• Response Body: Returns a JSON object with the results from the requested APIs, formatted and processed for the registered APIs. Any other API will return the response in raw JSON depending on its configuration.

Should more API responses need to be deserialized and more nicely presented, following my approach, these are the required adjustments.
• The respective DTOs would need to be created
• The ExternalAPIService class' FetchDataAsync() method would need to have an additional else if statement for each additional API
• APIAggregateController's GetAggregatedData() method would need to have an additional else if statement for each additional API

There is some error handling capability. Each provided API URL is retried up to three times. If all attempts fail, an appropriate error will be returned.

There is some filtering capability. The query supports a filterBy parameter to filter results.
For example: `https://localhost:7103/api/aggregate?filterBy=Holt` (will return a single entry from https://reqres.in/api/users that has "Holt" as last name)

Required dependencies

• .NET SDK 8 or later

• Serilog libraries for logging

  - Serilog
    
  - Serilog.AspNetCore
  
  - Serilog.Sinks.Console
  
  - Serilog.Sinks.File
