using System.Runtime.CompilerServices;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using ProcessingService.Common.Models;

namespace ProcessingService.Deployment;

public class ProcessingServiceDataStack : Stack
{
    internal ProcessingServiceDataStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
        var organizationScheduleTable = new Table(this, OrganizationSchedule.TableName, new TableProps
        {
            PartitionKey = new Attribute { Name = OrganizationSchedule.PartitionKey, Type = AttributeType.STRING },
            SortKey = new Attribute { Name = OrganizationSchedule.SortKey, Type = AttributeType.STRING }
        });
    }
}