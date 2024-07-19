using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using EloReputation.api;

namespace EloReputation.plugin.services;

public class RateLimiter(IEloPlugin plugin) : IRateLimiter<ulong> {
  private readonly Dictionary<ulong, int> limits = new();

  /// <summary>
  /// Gets the max configured number of rep points to give to a player
  /// 0 if the player is not authorized or otherwise configured
  /// </summary>
  /// <param name="player"></param>
  /// <exception cref="ArgumentNullException"></exception>
  /// <returns></returns>
  private int getLimit(CCSPlayerController player) {
    if (player.AuthorizedSteamID == null)
      throw new ArgumentNullException(nameof(player.AuthorizedSteamID));

    return (from entry in plugin.Config.EloGrantedPerMap
        where string.IsNullOrEmpty(entry.Key)
          || AdminManager.PlayerHasPermissions(player, entry.Key)
        select entry.Value).Prepend(0)
     .Max();
  }

  public void Reset() { limits.Clear(); }
  public void Reset(ulong key) { limits.Remove(key); }

  public int Increment(ulong key) {
    limits[key] = limits.GetValueOrDefault(key, 0) + 1;
    return limits[key];
  }

  public int Get(ulong key) {
    if (limits.TryGetValue(key, out var value)) return value;

    var player = Utilities.GetPlayerFromSteamId(key);
    if (player == null) return 0;

    try { limits[key] = getLimit(player); } catch (ArgumentNullException e) {
      Console.WriteLine(e);
    }

    return limits.GetValueOrDefault(key, 0);
  }

  public bool Decrement(ulong key) {
    if (Get(key) <= 0) return false;
    limits[key]--;
    return true;
  }
}