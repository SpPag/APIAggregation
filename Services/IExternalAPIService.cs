namespace APIAggregationAssignment.Services
{
    //interface for fetching data from external APIs
    public interface IExternalAPIService
    {
        Task<Dictionary<string, object>> GetDataFromAPIsAsync(List<string> APIURLs);
    }
}
