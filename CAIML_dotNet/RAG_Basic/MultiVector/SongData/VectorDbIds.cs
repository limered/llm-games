using Microsoft.Data.Sqlite;

namespace MultiVector.SongData;

public static class VectorDbIds
{
    public static async Task<List<string>> ExtractFromCollection(
        string dbName,
        string parentTableName
    )
    {
        await using var connection = new SqliteConnection($"Data Source={dbName}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {parentTableName}";

        var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

        var retrievedParentVectors = new List<string>();
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var id = reader.GetString(0);
            retrievedParentVectors.Add(id);
        }

        connection.Close();

        return retrievedParentVectors;
    }
}