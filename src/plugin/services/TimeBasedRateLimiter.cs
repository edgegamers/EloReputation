using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using EloReputation.api;

namespace EloReputation.plugin.services;

public class TimeBasedRateLimiter : DictionaryListener {
  public TimeBasedRateLimiter(IEloPlugin plugin) : base(plugin) {
    plugin.GetBase().AddTimer(60 * 5, incrementLimits, TimerFlags.REPEAT);
  }

  private void incrementLimits() {
    foreach (var entry in Limits) {
      var player = Utilities.GetPlayerFromSteamId(entry.Key);
      if (player == null || player.AuthorizedSteamID == null) continue;
      var limit                                  = GetLimit(player);
      if (entry.Value < limit) Limits[entry.Key] = entry.Value + 1;
    }
  }

  override protected int GetLimit(CCSPlayerController player) {
    if (player.AuthorizedSteamID == null)
      throw new ArgumentNullException(nameof(player.AuthorizedSteamID));

    return (from entry in Plugin.Config.MaxPeriodicElo
        where string.IsNullOrEmpty(entry.Key)
          || AdminManager.PlayerHasPermissions(player, entry.Key)
        select entry.Value).Prepend(0)
     .Max();
  }
}