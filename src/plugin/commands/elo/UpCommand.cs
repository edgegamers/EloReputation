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
      info.ReplyLocalized(elo.getBase().Localizer, "player_not_found",
        info.GetArg(1));
      return;
    }

    if (target.Count() > 1) {
      info.ReplyLocalized(elo.getBase().Localizer, "player_found_multiple",
        info.GetArg(1));
      return;
    }

    var source   = executor.AuthorizedSteamID?.SteamId64;
    var receiver = target.First().AuthorizedSteamID?.SteamId64;

    if (source == null || receiver == null) {
      info.ReplyLocalized(elo.getBase().Localizer, "player_invalid",
        "You or they");
      return;
    }

    if (!elo.getRateLimiter().Decrement(source.Value)) {
      info.ReplyLocalized(elo.getBase().Localizer, "out_of_rep");
      return;
    }

    elo.getReputationService()
     .AddReputation(source.Value, receiver.Value);
    info.ReplyLocalized(elo.getBase().Localizer, "rep_given",
      target.First().PlayerName);
  }
}