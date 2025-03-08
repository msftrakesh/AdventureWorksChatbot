#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using System;
using System.IO;
using System.Net.NetworkInformation;
using AdventureWorksChatbot;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.DependencyInjection;


// ✅ Load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())  // Ensure correct path
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string? connectionString = config.GetConnectionString("AdventureWorks");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'AdventureWorks' not found.");
}

Console.WriteLine($"Connection String: {connectionString}");

// ✅ Initialize In-Memory Vector Store
var memoryStore = new InMemoryVectorStore();
var embeddingGenerator = new FakeEmbeddingGenerator();


var dbPlugin = new DatabasePlugin(connectionString);


var builder = Kernel.CreateBuilder();
builder.Plugins.AddFromObject(dbPlugin);
builder.AddInMemoryVectorStore();

builder.Services.AddSingleton<ITextEmbeddingGenerationService, FakeEmbeddingGenerator>();

builder.Services.AddSingleton<ISemanticTextMemory>(sp =>
    new SemanticTextMemory(
        new VolatileMemoryStore(),
        sp.GetRequiredService<ITextEmbeddingGenerationService>()
    )
);



builder.AddAzureOpenAIChatCompletion(
    "openai-service-deployment-gpt-4o-rakeshangi-ai",  // Your Deployment Name
    "https://openai-service-rakeshangi-ai.openai.azure.com/",  // Your Endpoint
    "G2enkBrSMOaQJfhhp9yLCV7WyFvYtMUqhAvhEfwUi1FyO0opZ72QJQQJ99BCACfhMk5XJ3w3AAABACOG7lah"  // Paste your key here
);



var kernel = builder.Build();

var memory = kernel.Services.GetRequiredService<ISemanticTextMemory>();

await memory.SaveInformationAsync("schema_memory", "product_table",
    "The Production.Product table contains ProductID, Name, ListPrice, ProductSubcategoryID.");

await memory.SaveInformationAsync("schema_memory", "product_subcategory_table",
    "The Production.ProductSubcategory table contains ProductSubcategoryID, ProductCategoryID, Name.");

await memory.SaveInformationAsync("schema_memory", "product_category_table",
    "The Production.ProductCategory table contains ProductCategoryID, Name.");

await memory.SaveInformationAsync("schema_memory", "product_description_view",
    "The Production.vProductAndDescription view contains ProductID, Name, ProductModel, CultureID, and Description.");



async Task<string> RetrieveSchemaAsync(string userQuery)
{
    var searchResults = new List<MemoryQueryResult>();
    await foreach (var result in memory.SearchAsync("schema_memory", userQuery, 3))
    {
        searchResults.Add(result);
    }

    if (searchResults.Count == 0)
    {
        return "No relevant schema found.";
    }

    return string.Join("\n", searchResults.Select(r => r.Metadata.Id));
}


// ✅ Print available plugins for debugging
Console.WriteLine("Registered Plugins & Functions:");
foreach (var plugin in kernel.Plugins)
{
    foreach (var function in plugin)
    {
        Console.WriteLine($"- {plugin.Name}.{function.PluginName}.{function.Description}");
    }
}

// ✅ Get the function reference for `QueryDatabaseAsync`
//var dbQueryFunction = kernel.Plugins.GetFunction("DatabasePlugin", "QueryDatabaseAsync");


Console.WriteLine("Chatbot Ready! Enter a question:");
while (true)
{
    Console.Write("User: ");
    string? userInput = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(userInput))
    {
        string relevantSchema = await RetrieveSchemaAsync(userInput);
        Console.WriteLine($"Schema: {relevantSchema}");
        // ✅ Convert user question to SQL
        var result = await kernel.InvokePromptAsync($@"
            Convert the following natural language request into a valid T-SQL query for SQL Server.

            ### Schema Context:
            - Use only VIEWS, not tables.
            - Available views:
            {relevantSchema}
            

            ### User Request:
            {userInput}

            ### Rules:
            - Use only SQL Server syntax.           
            - Return only the SQL query, no explanations or formatting.
            - Use valid SQL Server syntax.

            ### Output:
            ");
        string sqlQuery = result.GetValue<string>() ?? "SELECT 1";


        //clean unwanted formatting from the openai response
        sqlQuery = sqlQuery
        .Replace("sql\n", "", StringComparison.OrdinalIgnoreCase) // Remove "sql\n"
        .Replace("```sql", "", StringComparison.OrdinalIgnoreCase) // Remove Markdown SQL block
        .Replace("```", "") // Remove any stray Markdown code blocks
        .Replace("\n", " ") // Replace newlines with spaces
        .Replace("\r", " ") // Remove carriage returns
        .Trim(); // Trim leading/trailing spaces


        Console.WriteLine($"Generated SQL Query: {sqlQuery}");  // ✅ Debugging SQL Query

       // continue;

        // ✅ Correctly invoke the function inside Semantic Kernel
        var dbResponseResult = await kernel.InvokeAsync("DatabasePlugin", "query_database", new KernelArguments { ["query"] = sqlQuery });
        string dbResponse = dbResponseResult.GetValue<string>() ?? "No data found.";

        // ✅ Use GPT-4o to summarize
        var finalResult = await kernel.InvokePromptAsync($"Summarize this data: {dbResponse}");
        string finalResponse = finalResult.GetValue<string>() ?? "No summary available.";

        Console.WriteLine($"AI: {finalResponse}");
    }
}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
