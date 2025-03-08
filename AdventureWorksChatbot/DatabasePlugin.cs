using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AdventureWorksChatbot
{
  

    public class DatabasePlugin
    {
        private readonly string _connectionString;

        public DatabasePlugin(string connectionString)
        {
            _connectionString = connectionString;
        }

        [KernelFunction("query_database")]
        public async Task<string> QueryDatabaseAsync(string query)
        {
            Console.WriteLine($"Executing SQL Query: {query}");  // for Debugging
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            StringBuilder result = new();
            while (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    result.Append($"{reader.GetName(i)}: {reader[i]} | ");
                }
                result.AppendLine();
            }

            return result.Length > 0 ? result.ToString() : "No data found.";
        }
    }


}
