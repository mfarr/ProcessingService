using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ProcessingService.Common.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProcessingService.Api;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;

    public Function()
    {
        var dynamoDbClient = new AmazonDynamoDBClient();

        _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
    }
    
    /// <summary>
    /// A simple function that returns all scheduled jobs in a JSON format.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var results = await GetAllOrganizationSchedulesAsync();

        var resultsAsJson = JsonSerializer.Serialize(results);
        
        return new APIGatewayProxyResponse
        {
            Body = resultsAsJson,
            Headers = new Dictionary<string, string> { {"Content-Type", "application/json"} },
            StatusCode = 200
        };
    }

    private async Task<IEnumerable<OrganizationSchedule>> GetAllOrganizationSchedulesAsync()
    {
        var table = _dynamoDbContext.GetTargetTable<OrganizationSchedule>();

        var search = table.Scan(new ScanOperationConfig());

        var results = await search.GetNextSetAsync();

        return _dynamoDbContext.FromDocuments<OrganizationSchedule>(results);
    }
}
