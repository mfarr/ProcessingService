using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using ProcessingService.Common.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace QueueReportsFromSchedule;

public class Function
{
    public readonly DynamoDBContext DynamoDbContext;
    
    public const string ScheduleType = "QueueReportsFromSchedule";
    
    public Function()
    {
        var dynamoDbClient = new AmazonDynamoDBClient();

        DynamoDbContext = new DynamoDBContext(dynamoDbClient);
    }
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(string input, ILambdaContext context)
    {
        var currentTime = DateTime.Now;

        var upcomingSchedules = await GetUpcomingSchedulesAsync(currentTime);
    }

    private async Task<IEnumerable<OrganizationSchedule>> GetUpcomingSchedulesAsync(DateTime fromDate)
    {
        var table = DynamoDbContext.GetTargetTable<OrganizationSchedule>();

        var filter = new QueryFilter();
        
        filter.AddCondition(OrganizationSchedule.PartitionKey, QueryOperator.Equal, ScheduleType);
        
        filter.AddCondition("NextRun", ScanOperator.LessThan, fromDate);

        var result = table.Query(filter);

        var documents = await result.GetRemainingAsync();

        return DynamoDbContext.FromDocuments<OrganizationSchedule>(documents);
    }
}
