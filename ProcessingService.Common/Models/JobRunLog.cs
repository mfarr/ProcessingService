using Amazon.DynamoDBv2.DataModel;

namespace ProcessingService.Common.Models;

[DynamoDBTable("JobRunLog")]
public class JobRunLog
{
    [DynamoDBIgnore]
    public const string TableName = "JobRunLog";

    [DynamoDBIgnore] public const string PartitionKey = "OrganizationSchedule";

    [DynamoDBIgnore] public const string SortKey = "RunDate";
    
    [DynamoDBHashKey]
    public string OrganizationSchedule { get; set; }
    
    [DynamoDBRangeKey]
    public long RunDate { get; set; }
    
    public long CompletionDate { get; set; }
    
    public string Status { get; set; }

    public void SetOrganizationSchedule(string organization, string schedule)
    {
        OrganizationSchedule = $"{organization}#{schedule}";
    }

    public void SetRunDate(DateTime date)
    {
        RunDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void SetCompletionDate(DateTime date)
    {
        CompletionDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}