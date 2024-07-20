using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EloReputation.api;

namespace EloReputation.plugin.listeners;

public class MVPAssignerListener {
  private readonly IEloPlugin plugin;

  public MVPAssignerListener(IEloPlugin plugin) {
    this.plugin = plugin;
    plugin.GetBase().RegisterEventHandler<EventRoundStart>(OnRoundStart);
  }

  private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    var players = Utilities.GetPlayers()
     .Where(p => p.AuthorizedSteamID != null)
     .Select(p => p.AuthorizedSteamID!.SteamId64);

    Server.NextFrameAsync(async () => {
      var elos = await plugin.GetReputationService().GetReputation(players);
      Server.NextFrame(() => {
        foreach (var elo in elos) {
          var player = Utilities.GetPlayerFromSteamId(elo.Item1);
          if (player == null || !player.IsValid) continue;
          var stats = player.ActionTrackingServices?.MatchStats;
          if (stats == null) continue;

          player.MVPs = (int)Math.Floor(elo.Item2);
          Utilities.SetStateChanged(player, "CCSPlayerController", "m_iMVPs");
        }
      });
    });

    return HookResult.Continue;
  }
}