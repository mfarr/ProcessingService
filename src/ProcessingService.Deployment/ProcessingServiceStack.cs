using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace ProcessingService.Deployment
{
    public class ProcessingServiceStack : Stack
    {
        internal ProcessingServiceStack(Construct scope, string id, ProcessingServiceDataStack dataStack, IStackProps props = null) 
            : base(scope, id, props)
        {
            var apiLamdbaFunction = new Function(this, "ApiLambdaFunction", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Handler = "ProcessingService.Api::ProcessingService.Api.Function::FunctionHandler",
                Code = Code.FromAsset("./dist/ProcessingService.Api")
            });
            
            apiLamdbaFunction.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new []{ "dynamodb:Scan", "dynamodb:DescribeTable" },
                Resources = new [] { dataStack.OrganizationScheduleTable.TableArn, dataStack.JobRunLogTable.TableArn }
            }));

            var apiGateway = new RestApi(this, "ProcessingServiceApi", new RestApiProps
            {
                RestApiName = "ProcessingServiceApi",
                Description = "API for Tessitura Processing Service",
                DeployOptions = new StageOptions
                {
                    StageName = "Dev",
                    ThrottlingBurstLimit = 10,
                    ThrottlingRateLimit = 10,
                    LoggingLevel = MethodLoggingLevel.INFO,
                    MetricsEnabled = true
                },
                CloudWatchRole = true
            });

            apiGateway.Root.AddMethod("GET", new LambdaIntegration(apiLamdbaFunction,
                new LambdaIntegrationOptions
                {
                    Proxy = true
                }));
            
            new CfnOutput(this, "ApiGatewayEndpoint", new CfnOutputProps
            {
                Value = apiGateway.Url
            });
        }
    }
}
