using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using EloReputation.api;
using EloReputation.plugin.extensions;
using EloReputation.plugin.menus;
using plugin.commands;

namespace EloReputation.plugin.commands.elo;

public class RepTopCommand(IEloPlugin elo) : Command(elo) {
  public override void OnCommand(CCSPlayerController? executor,
    CommandInfo info) {
    var id = executor?.AuthorizedSteamID?.SteamId64;

    if (executor == null) {
      Server.NextFrameAsync(async () => {
        var top = await elo.getReputationService().GetTopReputation();
        Server.NextFrame(() => {
          var i = 1;
          foreach (var entry in top) {
            var name = Utilities.GetPlayerFromSteamId(entry.Item1)?.PlayerName
              ?? entry.Item1.ToString();
            var rep = Math.Round(entry.Item2, 2);
            printTo(executor,
              $"{ChatColors.Green}{i++}{ChatColors.Grey}: {ChatColors.Blue}{name} {ChatColors.Grey}- {ChatColors.Yellow}{rep}");
          }
        });
      });
      return;
    }

    Server.NextFrameAsync(async () => {
      if (id == null) return;
      var top = await elo.getReputationService().GetTopReputation(50);
      Server.NextFrame(() => {
        var menu = new ReputationMenu(elo, top);
        MenuManager.OpenCenterHtmlMenu(elo.getBase(), executor!, menu);
      });
      var pos = await elo.getReputationService()
       .GetReputationPosition(id.Value);
      Server.NextFrame(() => {
        executor.PrintLocalizedChat(elo.getBase().Localizer, "rep_ranking",
          pos.Item1, pos.Item2);
      });
    });
  }

  private void printTo(CCSPlayerController? player, string msg,
    params object[] args) {
    if (player == null) {
      Server.PrintToConsole(string.Format(msg, args));
    } else { player.PrintToChat(string.Format(msg, args)); }
  }
}