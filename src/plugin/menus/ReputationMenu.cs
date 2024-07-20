using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using EloReputation.api;
using EloReputation.plugin.utils;

namespace EloReputation.plugin.menus;

public class ReputationMenu : CenterHtmlMenu {
  public ReputationMenu(IEloPlugin plugin, IEnumerable<(ulong, double)> ranks) :
    base("Reputation Leaderboard", plugin.GetBase()) {
    var entries = ranks.ToList();

    Server.NextFrameAsync(async () => {
    var names = await NameUtil.GetPlayerNamesFromSteam(entries.Select(p => p.Item1));

      Server.NextFrame(() => {
        for (var i = 0; i < entries.Count; i++) {
          var name = names[i];
          var rep  = Math.Round(entries[i].Item2, 2);
          var pos  = i;
          AddMenuOption(
            $"{ChatColors.Green}{i + 1}{ChatColors.Grey}: {ChatColors.Blue}{name} {ChatColors.Grey}- {ChatColors.Yellow}{rep}",
            (player, _) => {
              player.ExecuteClientCommandFromServer(
                $"css_rep {entries[pos].Item1}");
            });
        }
      });
    });
  }

  public override sealed ChatMenuOption AddMenuOption(string display,
    Action<CCSPlayerController, ChatMenuOption> onSelect,
    bool disabled = false) {
    return base.AddMenuOption(display, onSelect, disabled);
  }
}