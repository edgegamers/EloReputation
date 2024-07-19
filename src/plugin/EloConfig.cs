using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace EloReputation.api;

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
}