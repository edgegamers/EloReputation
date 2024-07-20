using CounterStrikeSharp.API;
using EloReputation.api;
using EloReputation.plugin.extensions;
using MySqlConnector;

namespace EloReputation.plugin.services;

public class ReputationService : IReputationService {
  private readonly IEloPlugin plugin;

  public ReputationService(IEloPlugin plugin) {
    this.plugin = plugin;
    createReputationDatabase();
  }

  public async Task<double> GetReputation(ulong steamId) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();
    var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
        SELECT `total_value` FROM `{plugin.Config.DatabaseTablePrefix}total`
        WHERE `target` = @steamId;
      """;

    cmd.Parameters.AddWithValue("@steamId", steamId);

    var result = await cmd.ExecuteScalarAsync();

    if (result is not double) return 0.0;
    return Math.Round((double)result, 2);
  }

  public async Task<IEnumerable<(ulong, double)>> GetReputation(
    params ulong[] steamIds) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();
    var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
        SELECT `target`, `total_value` FROM `{plugin.Config.DatabaseTablePrefix}total`
        WHERE `target` IN ({string.Join(",", steamIds)})
        ORDER BY `total_value` DESC;
      """;

    Server.NextFrame(() => { Server.PrintToConsole(cmd.CommandText); });

    await using var reader  = await cmd.ExecuteReaderAsync();
    var             results = new List<(ulong, double)>();
    while (await reader.ReadAsync())
      results.Add((ulong.Parse(reader["target"].ToString() ?? string.Empty),
        (double)reader["total_value"]));

    return results;
  }

  public async Task<IEnumerable<(ulong, double)>> GetTopReputation(
    int limit = 10, int offset = 0) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
        SELECT `target`, `total_value` FROM `{plugin.Config.DatabaseTablePrefix}total`
        ORDER BY `total_value` DESC
        LIMIT @limit OFFSET @offset;
      """;

    cmd.Parameters.AddWithValue("@limit", limit);
    cmd.Parameters.AddWithValue("@offset", offset);

    await using var reader  = await cmd.ExecuteReaderAsync();
    var             results = new List<(ulong, double)>();
    while (await reader.ReadAsync()) {
      var target = ulong.Parse(reader["target"].ToString() ?? string.Empty);
      var value  = (double)reader["total_value"];
      results.Add((target, value));
    }

    return results;
  }

  public async Task<(int, int)> GetReputationPosition(ulong steamId) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
          SELECT COUNT(*) AS `position`
          FROM `{plugin.Config.DatabaseTablePrefix}total`
          WHERE `total_value` > (
              SELECT `total_value`
              FROM `{plugin.Config.DatabaseTablePrefix}total`
              WHERE `target` = @steamId
          );
      """;

    cmd.Parameters.AddWithValue("@steamId", steamId);

    await using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return (0, 0);

    var position = int.Parse(reader["position"].ToString() ?? "0") + 1;

    reader.Close();

    cmd = conn.CreateCommand();
    cmd.CommandText = $"""
          SELECT COUNT(*) AS `total_value`
          FROM `{plugin.Config.DatabaseTablePrefix}total`;
      """;

    return (position,
      Convert.ToInt32((long)(await cmd.ExecuteScalarAsync() ?? 0)));
  }

  public async Task AddReputation(ulong giver, ulong receiver,
    bool positive = true) {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var table = plugin.Config.DatabaseTablePrefix
      + (positive ? "positive" : "negative");

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
            INSERT INTO `{table}` (`source`, `target`, `count`)
            VALUES (@giver, @receiver, 1)
            ON DUPLICATE KEY UPDATE `count` = `count` + 1;
      """;

    cmd.Parameters.AddWithValue("@giver", giver);
    cmd.Parameters.AddWithValue("@receiver", receiver);

    await cmd.ExecuteNonQueryAsync();

    var rep = await GetReputation(receiver);

    Server.NextFrame(() => {
      var player = Utilities.GetPlayerFromSteamId(receiver);
      if (player == null || !player.IsValid) return;
      foreach (var p in Utilities.GetPlayers())
        p.PrintLocalizedChat(plugin.GetBase().Localizer, "rep_status",
          player.PlayerName, rep);
    });
  }

  private async void createReputationDatabase() {
    await using var conn =
      new MySqlConnection(plugin.Config.DatabaseConnectionString);
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = """
    CREATE FUNCTION IF NOT EXISTS harmonic_sum(n INT) RETURNS double
    BEGIN
        DECLARE sum double DEFAULT 0;
        DECLARE i INT DEFAULT 1;
        WHILE i <= n DO
            SET sum = sum + 1.0/i;
            SET i = i + 1;
        END WHILE;
        RETURN sum;
    END;
    """;

    await cmd.ExecuteNonQueryAsync();

    await createRepTable(conn, plugin.Config.DatabaseTablePrefix + "negative");
    await createRepTable(conn, plugin.Config.DatabaseTablePrefix + "positive");

    cmd = conn.CreateCommand();
    var prefix = plugin.Config.DatabaseTablePrefix;

    cmd.CommandText = $"""
          CREATE OR REPLACE VIEW {prefix}total AS
          SELECT
              target,
              (COALESCE(
                  (SELECT SUM(harmonic_sum(count))
                   FROM {prefix}positive t1
                   WHERE t1.target = v.target), 0) -
               COALESCE(
                  (SELECT SUM(harmonic_sum(count))
                   FROM {prefix}negative t2
                   WHERE t2.target = v.target), 0)) AS total_value
          FROM
              (SELECT DISTINCT target FROM {prefix}negative
               UNION
               SELECT DISTINCT target FROM {prefix}positive) v;
      """;

    await cmd.ExecuteNonQueryAsync();
  }

  private async Task createRepTable(MySqlConnection connection, string name) {
    var cmd = connection.CreateCommand();
    cmd.CommandText = $"""
            CREATE TABLE IF NOT EXISTS `{name}` (
              `source` VARCHAR(17) NOT NULL,
              `target` VARCHAR(17) NOT NULL,
              `count` INT NOT NULL DEFAULT 0,
              PRIMARY KEY (`source`, `target`)
            );
      """;

    await cmd.ExecuteNonQueryAsync();
  }
}