using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Movies.Api;

// One time run
const bool runOnce = false;

if (runOnce)
{
    await new DataSeeder().ImportDataAsync<MoviePkTitle>("movies_title_year");
    await new DataSeeder().ImportDataAsync<MoviePkReleaseYear>("movies_year_title");
    return;
}

var newMovieByTitle = new MoviePkTitle
{
    Id = Guid.NewGuid(),
    Title = "New Movie Title",
    AgeRestriction = 18,
    ReleaseYear = 2012,
    RottenTomatoesPercentage = 85
};

var newMovieByReleaseDate = new MoviePkReleaseYear()
{
    Id = Guid.NewGuid(),
    Title = "New Movie Title",
    AgeRestriction = 18,
    ReleaseYear = 2012,
    RottenTomatoesPercentage = 85
};

var movieAsJsonByTitle = JsonSerializer.Serialize(newMovieByTitle);
var movieAsJsonByReleaseDate = JsonSerializer.Serialize(newMovieByReleaseDate);

var movieByTitleAsAttributeMap = Document.FromJson(movieAsJsonByTitle).ToAttributeMap();
var movieByReleaseAsAttributeMap = Document.FromJson(movieAsJsonByReleaseDate).ToAttributeMap();

// Actions available are PUT, UPDATE, DELETE, CONDITIONCHECK
var transactionRequest = new TransactWriteItemsRequest
{
    TransactItems = new List<TransactWriteItem>()
    {
        new()
        {
            Put = new Put
            {
                TableName = "movies_title_year",
                Item = movieByTitleAsAttributeMap
            }
        },
        new()
        {
            Put = new Put
            {
                TableName = "movies_year_title",
                Item = movieByReleaseAsAttributeMap
            }
        }
    }
};

var dynamoDbClient = new AmazonDynamoDBClient();

var response = await dynamoDbClient.TransactWriteItemsAsync(transactionRequest);