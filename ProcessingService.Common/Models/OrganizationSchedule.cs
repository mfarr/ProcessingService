using Amazon.DynamoDBv2.DataModel;

namespace ProcessingService.Common.Models;

[DynamoDBTable("OrganizationSchedule")]
public class OrganizationSchedule
{
    [DynamoDBIgnore]
    public const string TableName = "OrganizationSchedule";

    [DynamoDBIgnore] public const string PartitionKey = "OrganizationId";

    [DynamoDBIgnore] public const string SortKey = "ScheduleType";
    
    [DynamoDBHashKey]
    public string OrganizationId { get; set; }
    
    [DynamoDBRangeKey]
    public string ScheduleType { get; set; }
    
    public int IntervalMinutes { get; set; }
    
    public DateTime LastRun { get; set; }
    
    public DateTime NextRun { get; set; }
}