namespace APIAggregationAssignment.Models
{
    public class APIAggregateDTO
    {
        //the DTO will be a Dictionary with key-value-pairs in the form of APIURL: APICall.Result
        public Dictionary<string, object> APIResponses { get; set; } = new();
    }
}
