using System.Text.Json;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Splitters.Text;
using Microsoft.Data.Sqlite;

namespace ParentDocumentRetriever;

public static class ChildDocuments
{
    public static async Task<List<Document>> ExtractFromParentCollection(
        string dbName,
        string parentTableName,
        ITextSplitter childSplitter
    )
    {
        await using var connection = new SqliteConnection($"Data Source={dbName}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {parentTableName}";

        var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

        var retrievedParentVectors = new List<(string, string)>();
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var id = reader.GetString(0);
            var doc = await reader.GetFieldValueAsync<string>(2).ConfigureAwait(false);
            var document = JsonSerializer.Deserialize<Document>(doc) ?? Document.Empty;
            var text = document.PageContent;

            retrievedParentVectors.Add((id, text));
        }

        connection.Close();

        return retrievedParentVectors
            .Select(tuple => childSplitter
                .CreateDocuments([tuple.Item2], [new Dictionary<string, object> { { "parentId", tuple.Item1 } }]))
            .Aggregate(new List<Document>(), (list, onlyList) => list.Concat(onlyList).ToList());
    }
}