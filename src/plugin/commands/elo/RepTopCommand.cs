using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using EloReputation.api;
using EloReputation.plugin.extensions;
using EloReputation.plugin.menus;
using EloReputation.plugin.utils;

namespace EloReputation.plugin.commands.elo;

public class RepTopCommand(IEloPlugin elo) : Command(elo) {
  public override void OnCommand(CCSPlayerController? executor,
    CommandInfo info) {
    var id = executor?.AuthorizedSteamID?.SteamId64;

    if (executor == null) {
      Server.NextFrameAsync(async () => {
        var top = (await Elo.GetReputationService().GetTopReputation())
         .ToList();
        var names =
          await NameUtil.GetPlayerNamesFromSteam(top.Select(p => p.Item1));

        Server.NextFrame(() => {
          var i = 1;
          foreach (var entry in top) {
            var name = names[i - 1];
            var rep  = Math.Round(entry.Item2, 2);
            printTo(executor,
              $"{ChatColors.Green}{i++}{ChatColors.Grey}: {ChatColors.Blue}{name} {ChatColors.Grey}- {ChatColors.Yellow}{rep}");
          }
        });
      });
      return;
    }

    if (id == null) return;
    Server.NextFrameAsync(async () => {
      var top = await Elo.GetReputationService().GetTopReputation(50);
      Server.NextFrame(() => {
        var menu = new ReputationMenu(Elo, top);
        MenuManager.OpenCenterHtmlMenu(Elo.GetBase(), executor, menu);
      });
      var pos = await Elo.GetReputationService()
       .GetReputationPosition(id.Value);
      Server.NextFrame(() => {
        executor.PrintLocalizedChat(Elo.GetBase().Localizer, "rep_ranking",
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