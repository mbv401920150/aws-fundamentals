using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Customers.Api.Contracts.Data;

namespace Customers.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName = "customers";

    public CustomerRepository(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
    }

    public async Task<bool> CreateAsync(CustomerDto customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;

        var customerAsJson = JsonSerializer.Serialize(customer);
        var customerAsAttributes = Document.FromJson(customerAsJson).ToAttributeMap();

        var createItemRequest = new PutItemRequest()
        {
            TableName = _tableName,
            Item = customerAsAttributes,
            ConditionExpression = "attribute_not_exists(pk) and attribute_not_exists(sk)",
        };

        var createItemResponse = await _dynamoDb.PutItemAsync(createItemRequest);

        return createItemResponse.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<CustomerDto?> GetAsync(Guid id)
    {
        var getItemRequest = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = id.ToString() } },
                { "sk", new AttributeValue { S = id.ToString() } }
            }
        };

        var getItemResponse = await _dynamoDb.GetItemAsync(getItemRequest);

        if (getItemResponse.Item.Count == 0)
        {
            return null;
        }

        var itemAsDocument = Document.FromAttributeMap(getItemResponse.Item);

        return JsonSerializer.Deserialize<CustomerDto>(itemAsDocument.ToJson());
    }
    
public async Task<CustomerDto?> GetByEmailAsync(string email)
{
    var queryRequest = new QueryRequest
    {
        TableName = _tableName,
        IndexName = "email-id-index",
        KeyConditionExpression = "Email = :email",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            {
                ":email", new AttributeValue { S = email }    
            }
        }
    };

    var queryResponse = await _dynamoDb.QueryAsync(queryRequest);

    if (queryResponse.Items.Count == 0)
    {
        return null;
    }

    var itemAsDocument = Document.FromAttributeMap(queryResponse.Items.First());

    return JsonSerializer.Deserialize<CustomerDto>(itemAsDocument.ToJson());
}

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var scanRequest = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDb.ScanAsync(scanRequest);
        return response.Items.Select(item =>
        {
            var jsonItem = Document.FromAttributeMap(item).ToJson();
            return JsonSerializer.Deserialize<CustomerDto>(jsonItem);
        })!;
    }

    public async Task<bool> UpdateAsync(CustomerDto customer, DateTime requestStarted)
    {
        customer.UpdatedAt = DateTime.UtcNow;

        var customerAsJson = JsonSerializer.Serialize(customer);
        var customerAsAttributes = Document.FromJson(customerAsJson).ToAttributeMap();

        var updateItemRequest = new PutItemRequest()
        {
            TableName = _tableName,
            Item = customerAsAttributes,
            ConditionExpression = "UpdatedAt < :requestStarted",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":requestStarted", new AttributeValue { S = requestStarted.ToString("O") } }
            }
        };

        var updateItemResponse = await _dynamoDb.PutItemAsync(updateItemRequest);

        return updateItemResponse.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var deleteItemRequest = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = id.ToString() } },
                { "sk", new AttributeValue { S = id.ToString() } }
            }
        };

        var deleteItemResponse = await _dynamoDb.DeleteItemAsync(deleteItemRequest);

        return deleteItemResponse.HttpStatusCode == HttpStatusCode.OK;
    }
}