# AdventureWorks Chatbot with Semantic Kernel

This project is a C# console application that integrates with Microsoft Semantic Kernel to create an intelligent chatbot capable of retrieving and querying database schema information from an AdventureWorks database. The chatbot uses Azure OpenAI GPT-4o for natural language processing and generating SQL queries based on user input.

### Prerequisites
.NET 8.0+
Azure OpenAI GPT-4o deployment
AdventureWorks SQL Server database

## How to Run
1. Execute the application:
```bash
dotnet build
dotnet run
```

2. Enter a natural language query:
User:
What are the product categories available?
3. Example output:
Generated SQL Query:
```sql
SELECT ProductCategoryID, Name FROM Production.ProductCategory;
```
4. AI: There are 4 product categories available.



## How It Works
### ✅ Step 1: Load Configuration
Loads database connection strings and OpenAI settings from appsettings.json
### ✅ Step 2: Initialize Semantic Kernel
Creates an in-memory vector store
Registers the DatabasePlugin for database access
Adds Azure OpenAI GPT-4o for natural language processing
### ✅ Step 3: Handle User Input
Searches schema using vector embeddings
Converts natural language to SQL using GPT-4o
Executes SQL query
Summarizes result using GPT-4o
### ✅ Step 4: Display Results
Displays the generated SQL query
Displays the summarized response

## Security
Ensure that the API key and connection string are securely stored.
Use Azure Key Vault to manage secrets.
## 🚨 Known Issues
Currently using a fake embedding generator — replace with a real generator for production.
SQL injection prevention should be added for enhanced security.

## 🏅 License
This project is licensed under the MIT License.

