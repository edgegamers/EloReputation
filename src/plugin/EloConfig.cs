using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace EloReputation.api;

public class EloConfig : BasePluginConfig {
  [JsonPropertyName("DatabaseConnectionString")]
  public string? DatabaseConnectionString { get; set; } =
    "Server=sm-db.edge-gamers.com;Database=reputation_dev;user=reputation_user;password=mVPDwA24y8768yvLHx6Jr6SppxjF;port=3306";

  [JsonPropertyName("DatabaseTablePrefix")]
  public string DatabaseTablePrefix { get; set; } = "reputation_";

  [JsonPropertyName("EloGrantedPerMap")]
  public Dictionary<string, int> EloGrantedPerMap { get; set; } = new() {
    { "", 2 },
    { "@ego/e", 3 },
    { "@ego/dssilver", 4 },
    { "@ego/dsgold", 5 },
    { "@ego/dsplatinum", 6 },
    { "@ego/dsroyal", 8 }
  };
}