using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using EloReputation.api;
using plugin.commands;

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
        target = target.Append<ulong>(executor.AuthorizedSteamID.SteamId64);
    } else {
      target = info.GetArgTargetResult(1)
       .Players.Where(p => p.AuthorizedSteamID != null)
       .Select(p => p.AuthorizedSteamID!.SteamId64);
    }

    target = target.ToList();

    if (!target.Any()) {
      info.ReplyToCommand("No players matched");
      return;
    }

    foreach (var steamId in target) {
      Server.NextFrameAsync(async () => {
        var rep = await elo.getReputationService().GetReputation(steamId);
        var name = Utilities.GetPlayerFromSteamId(steamId)?.PlayerName
          ?? steamId.ToString();
        info.ReplyToCommand($"{name} has {rep} reputation");
      });
    }
  }
}