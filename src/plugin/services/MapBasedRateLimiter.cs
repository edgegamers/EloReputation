using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using EloReputation.api;

namespace EloReputation.plugin.services;

public class MapBasedRateLimiter : DictionaryListener {
  public MapBasedRateLimiter(IEloPlugin plugin) : base(plugin) {
    plugin.GetBase().RegisterListener<Listeners.OnMapStart>(_ => Reset());
  }

  override protected int GetLimit(CCSPlayerController player) {
    if (player.AuthorizedSteamID == null)
      throw new ArgumentNullException(nameof(player.AuthorizedSteamID));

    return (from entry in Plugin.Config.EloGrantedPerMap
        where string.IsNullOrEmpty(entry.Key)
          || AdminManager.PlayerHasPermissions(player, entry.Key)
        select entry.Value).Prepend(0)
     .Max();
  }
}