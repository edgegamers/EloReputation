using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using EloReputation.api;
using EloReputation.plugin.extensions;
using EloReputation.plugin.utils;

namespace EloReputation.plugin.commands.elo;

public class RepCommand(IEloPlugin elo) : Command(elo) {
  public override void OnCommand(CCSPlayerController? executor,
    CommandInfo info) {
    IEnumerable<ulong> target = [];

    if (info.ArgCount != 2) {
      if (executor == null) {
        info.ReplyToCommand("console must specify a target");
        return;
      }

      if (executor.AuthorizedSteamID != null)
        target = target.Append(executor.AuthorizedSteamID.SteamId64);
    } else {
      target = info.GetArgTargetResult(1)
       .Players.Where(p => p.AuthorizedSteamID != null)
       .Select(p => p.AuthorizedSteamID!.SteamId64);
    }

    target = target.ToList();

    if (!target.Any()) {
      if (info.GetArg(1).All(char.IsDigit)) {
        target = target.Append(ulong.Parse(info.GetArg(1)));
      } else {
        info.ReplyLocalized(Elo.GetBase().Localizer, "player_not_found",
          info.GetArg(1));
        return;
      }
    }

    var steamIds = target.ToList();
    info.ReplyLocalized(Elo.GetBase().Localizer, "rep_fetching", steamIds.Count,
      steamIds.Count == 1 ? "" : "s");
    Server.NextFrameAsync(async () => {
      var reps = (await Elo.GetReputationService().GetReputation(steamIds))
       .ToList();
      var names =
        await NameUtil.GetPlayerNamesFromSteam(reps.Select(p => p.Item1));
      Server.NextFrame(() => {
        for (var i = 0; i < reps.Count; i++) {
          var name = names[i];
          var rep  = Math.Round(reps[i].Item2, 2);
          executor.PrintLocalizedChat(Elo.GetBase().Localizer, "rep_status",
            name, rep);
        }
      });
    });
  }
}