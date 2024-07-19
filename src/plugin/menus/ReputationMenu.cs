using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using EloReputation.api;

namespace EloReputation.plugin.menus;

public class ReputationMenu : CenterHtmlMenu {
  public ReputationMenu(IEloPlugin plugin, IEnumerable<(ulong, double)> ranks) :
    base("Reputation Leaderboard", plugin.GetBase()) {
    var i = 1;
    foreach (var entry in ranks) {
      var name = Utilities.GetPlayerFromSteamId(entry.Item1)?.PlayerName
        ?? entry.Item1.ToString();

      AddMenuOption(
        $"{ChatColors.Green}{i++}{ChatColors.Grey}. {ChatColors.Blue + name + ChatColors.Grey} - {ChatColors.Yellow}{Math.Round(entry.Item2, 2)}",
        (_, _) => { }, true);
    }
  }

  public override sealed ChatMenuOption AddMenuOption(string display,
    Action<CCSPlayerController, ChatMenuOption> onSelect,
    bool disabled = false) {
    return base.AddMenuOption(display, onSelect, disabled);
  }
}