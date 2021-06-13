using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace newki_inventory_jobs
{
    public interface IDynamoDbContext
    {
        AmazonDynamoDBClient GetClient();
    }
    public class DynamoDbContext :IDynamoDbContext
    {
        public AmazonDynamoDBClient GetClient()
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials("AKIAIEHJNB5Q2L2Z74DQ", "ijalg1mTQyXYkF+bPD4lk5uvh3uRFxCLJW5QgNqe");
            return new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        }
    }
}