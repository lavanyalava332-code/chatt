
using ModelContextProtocol.Server;
using System.ComponentModel;
using Npgsql;
using System.Collections.Generic;

[McpServerToolType]
public static class LightbulbTools
{
    private static string? _connectionString;

    // Set connection string from environment variable or config
    public static void SetConnectionString(string connStr) => _connectionString = connStr;

    [McpServerTool]
    [Description("Gets a list of all bulbs and their current state (on/off and location) from the database.")]
    public static List<LightModel> GetLights()
    {
        var lights = new List<LightModel>();
        if (string.IsNullOrWhiteSpace(_connectionString)) return lights;
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT id, name, is_on, location FROM lightbulbs", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lights.Add(new LightModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsOn = reader.GetBoolean(2),
                Location = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }
        return lights;
    }

    [McpServerTool]
    [Description("Change the state (on/off) of a bulb in the database.")]
    public static LightModel? ChangeState(int id, bool isOn)
    {
        if (string.IsNullOrWhiteSpace(_connectionString)) return null;
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE lightbulbs SET is_on = @isOn WHERE id = @id RETURNING id, name, is_on, location", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("isOn", isOn);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new LightModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsOn = reader.GetBoolean(2),
                Location = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }
        return null;
    }
}

public class LightModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool? IsOn { get; set; }
    public string? Location { get; set; }
}