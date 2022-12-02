using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.Common.Models;

namespace ClientApi.Controllers;

[Route("[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly DynamoDBContext _dynamoDbContext;
    
    public SchedulesController()
    {
        var dynamoDbClient = new AmazonDynamoDBClient();

        _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
    }
    
    // GET api/schedules
    [HttpGet]
    public async Task<IEnumerable<OrganizationSchedule>> Get()
    {
        return await GetAllOrganizationSchedulesAsync();
    }

    private async Task<IEnumerable<OrganizationSchedule>> GetAllOrganizationSchedulesAsync()
    {
        var table = _dynamoDbContext.GetTargetTable<OrganizationSchedule>();

        var search = table.Scan(new ScanOperationConfig());

        var results = await search.GetNextSetAsync();

        return _dynamoDbContext.FromDocuments<OrganizationSchedule>(results);
    }
}