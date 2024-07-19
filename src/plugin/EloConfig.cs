using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace EloReputation.plugin;

public class EloConfig : BasePluginConfig {
  [JsonPropertyName("DatabaseConnectionString")]
  public string? DatabaseConnectionString { get; set; }

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

  [JsonPropertyName("MaxPeriodicElo")]
  public Dictionary<string, int> MaxPeriodicElo { get; set; } = new() {
    { "", 1 },
    { "@ego/e", 1 },
    { "@ego/dssilver", 2 },
    { "@ego/dsgold", 2 },
    { "@ego/dsplatinum", 2 },
    { "@ego/dsroyal", 3 }
  };
}