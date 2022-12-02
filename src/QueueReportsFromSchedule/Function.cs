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
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        var currentTime = DateTime.Now;

        var upcomingSchedules = await GetUpcomingSchedulesAsync(currentTime);
        
        await ProcessSchedules(upcomingSchedules);
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

    private async Task ProcessSchedules(IEnumerable<OrganizationSchedule> schedules)
    {
        foreach (var schedule in schedules)
        {
            var runLog = new JobRunLog
            {
                Status = "complete",
                CompletionDate = DateTimeOffset.Now.ToUnixTimeSeconds(),
                RunDate = DateTimeOffset.Now.ToUnixTimeSeconds(),
                OrganizationSchedule = $"{schedule.OrganizationId}#{ScheduleType}"
            };

            var timeNow = DateTime.Now;
            
            schedule.LastRun = timeNow;
            
            schedule.NextRun = timeNow.AddMinutes(schedule.IntervalMinutes);

            await DynamoDbContext.SaveAsync(schedule);

            await DynamoDbContext.SaveAsync(runLog);
        }
    }
}
