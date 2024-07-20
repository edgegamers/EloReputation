using CounterStrikeSharp.API;

namespace EloReputation.plugin.utils;

public static class NameUtil {
  private static readonly Dictionary<ulong, string> NAME_CACHE = new();

  public static async Task<string[]> GetPlayerNamesFromSteam(
    IEnumerable<ulong> steamIDs) {
    var names = new List<Task<string>>();
    foreach (var steamID in steamIDs) {
      if (NAME_CACHE.TryGetValue(steamID, out var value)) {
        names.Add(Task.FromResult(value));
        continue;
      }

      names.Add(GetPlayerNameFromSteam(steamID));
    }

    return await Task.WhenAll(names);
  }

  public static void SetPlayerName(ulong steamID, string name) {
    NAME_CACHE[steamID] = name;
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