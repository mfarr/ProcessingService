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
    private readonly DynamoDBContext _dynamoDbContext;
    
    public const string ScheduleType = "QueueReportsFromSchedule";
    
    public Function()
    {
        var dynamoDbClient = new AmazonDynamoDBClient();

        _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
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
        var table = _dynamoDbContext.GetTargetTable<OrganizationSchedule>();

        var filter = new QueryFilter();
        
        filter.AddCondition(OrganizationSchedule.PartitionKey, QueryOperator.Equal, ScheduleType);
        
        filter.AddCondition("NextRun", ScanOperator.LessThan, fromDate);

        var result = table.Query(filter);

        var documents = await result.GetRemainingAsync();

        return _dynamoDbContext.FromDocuments<OrganizationSchedule>(documents);
    }

    private async Task ProcessSchedules(IEnumerable<OrganizationSchedule> schedules)
    {
        foreach (var schedule in schedules)
        {
            var runDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // DO TASK!

            var completionDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            var runLog = new JobRunLog
            {
                Status = "complete",
                CompletionDate = completionDate,
                RunDate = runDate,
                OrganizationSchedule = $"{schedule.OrganizationId}#{ScheduleType}"
            };

            var timeNow = DateTime.Now;
            
            schedule.LastRun = timeNow;
            
            schedule.NextRun = timeNow.AddMinutes(schedule.IntervalMinutes);

            await _dynamoDbContext.SaveAsync(schedule);

            await _dynamoDbContext.SaveAsync(runLog);
        }
    }
}
