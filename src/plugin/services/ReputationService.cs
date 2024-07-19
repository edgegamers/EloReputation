using EloReputation.api;
using MySqlConnector;

namespace EloReputation.plugin.services;

public class ReputationService : IReputationService {
  private readonly IEloPlugin plugin;

  public ReputationService(IEloPlugin plugin) {
    this.plugin = plugin;
    createReputationDatabase();
  }

  private async void createReputationDatabase() {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = """
    CREATE FUNCTION IF NOT EXISTS harmonic_sum(n INT) RETURNS FLOAT
    BEGIN
        DECLARE sum FLOAT DEFAULT 0;
        DECLARE i INT DEFAULT 1;
        WHILE i <= n DO
            SET sum = sum + 1.0/i;
            SET i = i + 1;
        END WHILE;
        RETURN sum;
    END;
    """;

    await cmd.ExecuteNonQueryAsync();

    cmd = conn.CreateCommand();
    cmd.CommandText = """
    CREATE OR REPLACE VIEW @total_name AS
    SELECT
        target,
        (COALESCE(
            (SELECT SUM(harmonic_sum(count))
             FROM @positive_name t1
             WHERE t1.target = v.target), 0) -
         COALESCE(
            (SELECT SUM(harmonic_sum(count))
             FROM @negative_name t2
             WHERE t2.target = v.target), 0)) AS total_value
    FROM
        (SELECT DISTINCT target FROM @negative_name
         UNION
         SELECT DISTINCT target FROM @positive_name) v;
    """;

    cmd.Parameters.AddWithValue("@total_name",
      plugin.Config.DatabaseTablePrefix + "total");
    cmd.Parameters.AddWithValue("@positive_name",
      plugin.Config.DatabaseTablePrefix + "positive");
    cmd.Parameters.AddWithValue("@negative_name",
      plugin.Config.DatabaseTablePrefix + "negative");

    await cmd.ExecuteNonQueryAsync();

    await createRepTable(conn, plugin.Config.DatabaseTablePrefix + "negative");
    await createRepTable(conn, plugin.Config.DatabaseTablePrefix + "positive");
  }

  private async Task createRepTable(MySqlConnection connection, string name) {
    var cmd = connection.CreateCommand();
    cmd.CommandText = """
      CREATE TABLE IF NOT EXISTS `@name` (
        `source` VARCHAR(17) NOT NULL,
        `target` VARCHAR(17) NOT NULL,
        `count` INT NOT NULL DEFAULT 0,
        PRIMARY KEY (`source`, `target`)
      );
    """;

    cmd.Parameters.AddWithValue("@name", name);
    await cmd.ExecuteNonQueryAsync();

    cmd = connection.CreateCommand();
    cmd.CommandText = """
        CREATE VIEW IF NOT EXISTS `@name` AS (
          SELECT `source`, `target`, `count` FROM `@name`
          WHERE `count` > 0
        );
    """;
  }

  public async Task<float> GetReputation(ulong steamId) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();
    var cmd = conn.CreateCommand();
    cmd.CommandText = """
      SELECT `total_value` FROM `@table`
      WHERE `target` = @steamId;
    """;

    cmd.Parameters.AddWithValue("@table",
      plugin.Config.DatabaseTablePrefix + "total");
    cmd.Parameters.AddWithValue("@steamId", steamId);

    return (float)(await cmd.ExecuteScalarAsync() ?? 0);
  }

  public async Task<IEnumerable<(ulong, float)>> GetReputation(
    params ulong[] steamIds) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = """
      SELECT `target`, `total_value` FROM `@table`
      WHERE `target` IN (@steamIds)
      ORDER BY `total_value` DESC
      LIMIT @limit OFFSET @offset;
    """;

    cmd.Parameters.AddWithValue("@table",
      plugin.Config.DatabaseTablePrefix + "total");
    cmd.Parameters.AddWithValue("@steamIds", steamIds);

    await using var reader  = await cmd.ExecuteReaderAsync();
    var             results = new List<(ulong, float)>();
    while (await reader.ReadAsync()) {
      results.Add(((ulong)reader["target"], (float)reader["total_value"]));
    }

    return results;
  }

  public async Task<IEnumerable<(ulong, float)>> GetTopReputation(int limit = 10,
    int offset = 0) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = """
      SELECT `target`, `total_value` FROM `@table`
      ORDER BY `total_value` DESC
      LIMIT @limit OFFSET @offset;
    """;

    cmd.Parameters.AddWithValue("@table",
      plugin.Config.DatabaseTablePrefix + "total");
    cmd.Parameters.AddWithValue("@limit", limit);
    cmd.Parameters.AddWithValue("@offset", offset);

    await using var reader  = await cmd.ExecuteReaderAsync();
    var             results = new List<(ulong, float)>();
    while (await reader.ReadAsync()) {
      results.Add(((ulong)reader["target"], (float)reader["total_value"]));
    }

    return results;
  }

  public async Task AddReputation(ulong giver, ulong receiver,
    bool positive = true) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = """
          INSERT INTO `@table` (`source`, `target`, `count`)
          VALUES (@giver, @receiver, 1)
          ON DUPLICATE KEY UPDATE `count` = `count` + 1;
    """;

    cmd.Parameters.AddWithValue("@table",
      plugin.Config.DatabaseTablePrefix + (positive ? "positive" : "negative"));
    cmd.Parameters.AddWithValue("@giver", giver);
    cmd.Parameters.AddWithValue("@receiver", receiver);

    await cmd.ExecuteNonQueryAsync();
  }
}