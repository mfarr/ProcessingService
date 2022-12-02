using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
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
            var apiLamdbaFunction = new Function(this, "ClientApi", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Handler = "ClientApi::ClientApi.LambdaEntryPoint::FunctionHandlerAsync",
                Code = Code.FromAsset("./dist/ClientApi"),
                Timeout = Duration.Seconds(30)
            });

            var queueReportsFromScheduleLambda = new Function(this, "QueueReportsFromSchedule", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Handler = "QueueReportsFromSchedule::QueueReportsFromSchedule.Function::FunctionHandler",
                Code = Code.FromAsset("./dist/QueueReportsFromSchedule"),
                Timeout = Duration.Seconds(30)
            });

            var dataStackPolicy = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] {"dynamodb:Scan", "dynamodb:Query", "dynamodb:DescribeTable", "dynamodb:UpdateItem"},
                Resources = new[] {dataStack.OrganizationScheduleTable.TableArn, dataStack.JobRunLogTable.TableArn}
            });
            
            apiLamdbaFunction.AddToRolePolicy(dataStackPolicy);
            
            queueReportsFromScheduleLambda.AddToRolePolicy(dataStackPolicy);

            var scheduleEvent = new Rule(this, "ScheduleEvent", new RuleProps
            {
                Schedule = Schedule.Cron(new CronOptions
                {
                    Minute = "*"
                })
            });
            
            scheduleEvent.AddTarget(new LambdaFunction(queueReportsFromScheduleLambda));

            var apiGateway = new RestApi(this, "ProcessingServiceApi", new RestApiProps
            {
                RestApiName = "ClientApi",
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

            apiGateway.Root.AddProxy(new ProxyResourceProps
            {
                AnyMethod = true,
                DefaultIntegration = new LambdaIntegration(apiLamdbaFunction)
            });
            
            new CfnOutput(this, "ApiGatewayEndpoint", new CfnOutputProps
            {
                Value = apiGateway.Url
            });
        }
    }
}
