using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.Common.Models;

namespace ClientApi.Controllers;

[Route("[controller]")]
public class JobLogsController : Controller
{

    private readonly DynamoDBContext _dynamoDbContext;
    
    public JobLogsController()
    {
        var dynamoDbClient = new AmazonDynamoDBClient();

        _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
    }
    
    // GET
    public async Task<IEnumerable<JobRunLog>> Index(string schedule, string organization)
    {
        return await GetAllJobsAsync(schedule, organization);
    }

    private async Task<IEnumerable<JobRunLog>> GetAllJobsAsync(string schedule, string organization)
    {
        var table = _dynamoDbContext.GetTargetTable<JobRunLog>();

        var filter = new QueryFilter(JobRunLog.PartitionKey, QueryOperator.Equal, $"{organization}#{schedule}");

        var result = table.Query(filter);

        var documents = await result.GetRemainingAsync();

        return _dynamoDbContext.FromDocuments<JobRunLog>(documents);
    }
}