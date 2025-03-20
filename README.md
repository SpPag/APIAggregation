# APIAggregation
This is a simple API Aggregation service. It's currently setup to deserialize and nicely present the responses from three APIs:
  - https://jsonplaceholder.typicode.com/posts
  - http://api.openweathermap.org/data/2.5/weather?q=Athens,gr&appid=fdbd122fc2168fc5916ab13827182b58
  - https://reqres.in/api/users

Any other API will return the response in raw JSON as it's configured to.
Should more API responses need to be deserialized and more nicely presented, following my approach, the respective DTOs would need to be created, the ExternalAPIService class' FetchDataAsync() method as well as APIAggregateController's GetAggregatedData() method would need to have an additional else if statement for each additional API.

There is some filtering capability. The query can accept the filterBy parameter, based on which it will return the appropriate data.
