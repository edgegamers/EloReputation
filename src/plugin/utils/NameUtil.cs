using CounterStrikeSharp.API;

namespace EloReputation.plugin.utils;

public static class NameUtil {
  private static readonly Dictionary<ulong, string> NAME_CACHE = new();

  public static string GetPlayerName(ulong steamID) {
    if (NAME_CACHE.TryGetValue(steamID, out var value)) return value;

    var player = Utilities.GetPlayerFromSteamId(steamID);
    if (player == null || !player.IsValid)
      return GetPlayerNameFromSteam(steamID).GetAwaiter().GetResult();
    NAME_CACHE[steamID] = player.PlayerName;
    return player.PlayerName;
  }

  public static async Task<string> GetPlayerNameFromSteam(ulong steamID) {
    if (NAME_CACHE.TryGetValue(steamID, out var value)) return value;

    try {
      using var client = new HttpClient();
      var response =
        await client.GetAsync(
          $"https://steamcommunity.com/profiles/{steamID}/?xml=1");
      response.EnsureSuccessStatusCode();
      var xmlContent = await response.Content.ReadAsStringAsync();

      var xmlDoc = new System.Xml.XmlDocument();
      xmlDoc.LoadXml(xmlContent);

      var nameNode = xmlDoc.SelectSingleNode("//steamID");

      var name = nameNode!.InnerText.Trim();

      NAME_CACHE[steamID] = name;
      return name;
    } catch (Exception ex) {
      Console.WriteLine(
        "GetPlayerNameFromSteamID Error occurred: " + ex.Message);
      return "Unknown";
    }
  }
}