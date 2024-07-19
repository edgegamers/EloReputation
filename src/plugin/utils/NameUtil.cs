using CounterStrikeSharp.API;

namespace EloReputation.plugin.utils;

public static class NameUtil {
  private static readonly Dictionary<ulong, string> NAME_CACHE = new();

  // public static void GetPlayerName(ulong steamID, Action<string> action) {
  //   if (NAME_CACHE.TryGetValue(steamID, out var value)) {
  //     action(value);
  //     return;
  //   }
  //
  //   Server.NextFrame(() => {
  //     var player = Utilities.GetPlayerFromSteamId(steamID);
  //     if (player == null || !player.IsValid) {
  //       Server.NextFrameAsync(async () => {
  //         var name = await GetPlayerNameFromSteam(steamID);
  //         action(name);
  //       });
  //       return;
  //     }
  //
  //     action(player.PlayerName);
  //   });
  // }

  public static async Task<string> GetPlayerNameAsync(ulong steamID) {
    if (NAME_CACHE.TryGetValue(steamID, out var value)) { return value; }

    // Simulate waiting for the next frame
    await Task.Yield();

    var player = Utilities.GetPlayerFromSteamId(steamID);
    if (player == null || !player.IsValid) {
      // Simulate waiting for the next frame again before async call
      await Task.Yield();
      var name = await GetPlayerNameFromSteam(steamID);
      return name;
    }

    return player.PlayerName;
  }

  private static async Task<string> GetPlayerNameFromSteam(ulong steamID) {
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