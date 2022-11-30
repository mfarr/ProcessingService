using System.Runtime.CompilerServices;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using ProcessingService.Common.Models;

namespace ProcessingService.Deployment;

public class ProcessingServiceDataStack : Stack
{
    public readonly Table OrganizationScheduleTable;

    public readonly Table JobRunLogTable;
    
    internal ProcessingServiceDataStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
        OrganizationScheduleTable = new Table(this, "OrganizationScheduleTable", new TableProps
        {
            TableName = OrganizationSchedule.TableName,
            PartitionKey = new Attribute { Name = OrganizationSchedule.PartitionKey, Type = AttributeType.STRING },
            SortKey = new Attribute { Name = OrganizationSchedule.SortKey, Type = AttributeType.STRING }
        });

        JobRunLogTable = new Table(this, "JobRunLogTable", new TableProps
        {
            TableName = JobRunLog.TableName,
            PartitionKey = new Attribute { Name = JobRunLog.PartitionKey, Type = AttributeType.STRING },
            SortKey = new Attribute { Name = JobRunLog.SortKey, Type = AttributeType.NUMBER }
        });
    }
}