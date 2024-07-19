using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using EloReputation.api;
using EloReputation.plugin.extensions;
using plugin.commands;

namespace EloReputation.plugin.commands.elo;

public class UpCommand(IEloPlugin elo) : Command(elo) {
  public override void OnCommand(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null) return;

    var target = info.GetArgTargetResult(1);
    if (!target.Any()) {
      info.ReplyToCommand("No players matched");
      return;
    }

    if (target.Count() > 1) {
      info.ReplyToCommand("Multiple players matched");
      return;
    }

    var source   = executor.AuthorizedSteamID?.SteamId64;
    var receiver = target.First().AuthorizedSteamID?.SteamId64;

    if (source == null || receiver == null) {
      info.ReplyToCommand("Invalid source or receiver (SteamID)");
      return;
    }

    if (!elo.getRateLimiter().Decrement(source.Value)) {
      info.ReplyToCommand("You are being rate limited");
      return;
    }

    elo.getReputationService()
     .AddReputation(source.Value, receiver.Value, true);
  }
}