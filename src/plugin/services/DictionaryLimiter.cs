using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EloReputation.api;

namespace EloReputation.plugin.services;

public abstract class DictionaryListener(IEloPlugin plugin)
  : IRateLimiter<ulong> {
  protected readonly Dictionary<ulong, int> Limits = new();
  protected readonly IEloPlugin Plugin = plugin;

  public void Reset() { Limits.Clear(); }
  public void Reset(ulong key) { Limits.Remove(key); }

  public int Increment(ulong key) {
    Limits[key] = Limits.GetValueOrDefault(key, 0) + 1;
    return Limits[key];
  }

  public int Get(ulong key) {
    if (Limits.TryGetValue(key, out var value)) return value;

    var player = Utilities.GetPlayerFromSteamId(key);
    if (player == null) return 0;

    try { Limits[key] = GetLimit(player); } catch (ArgumentNullException e) {
      Console.WriteLine(e);
    }

    return Limits.GetValueOrDefault(key, 0);
  }

  public bool Decrement(ulong key) {
    if (Get(key) <= 0) return false;
    Limits[key]--;
    return true;
  }

  /// <summary>
  ///   Gets the max configured number of rep points to give to a player
  ///   0 if the player is not authorized or otherwise configured
  /// </summary>
  /// <param name="player"></param>
  /// <exception cref="ArgumentNullException"></exception>
  /// <returns></returns>
  abstract protected int GetLimit(CCSPlayerController player);
}