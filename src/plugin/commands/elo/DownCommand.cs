using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using EloReputation.api;
using EloReputation.plugin.extensions;

namespace EloReputation.plugin.commands.elo;

public class DownCommand(IEloPlugin elo) : Command(elo) {
  public override void OnCommand(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null) return;

    var target = info.GetArgTargetResult(1);
    if (!target.Any()) {
      info.ReplyLocalized(Elo.GetBase().Localizer, "player_not_found",
        info.GetArg(1));
      return;
    }

    if (target.Count() > 1) {
      info.ReplyLocalized(Elo.GetBase().Localizer, "player_found_multiple",
        info.GetArg(1));
      return;
    }

    var source   = executor.AuthorizedSteamID?.SteamId64;
    var receiver = target.First().AuthorizedSteamID?.SteamId64;

    if (source == null || receiver == null) {
      info.ReplyLocalized(Elo.GetBase().Localizer, "player_invalid",
        "You or they");
      return;
    }

    if (!Elo.GetPeriodLimiter().Decrement(source.Value)) {
      info.ReplyLocalized(Elo.GetBase().Localizer, "out_of_rep_period");
      return;
    }

    if (!Elo.GetMapLimiter().Decrement(source.Value)) {
      info.ReplyLocalized(Elo.GetBase().Localizer, "out_of_rep_map");
      return;
    }

    info.ReplyLocalized(Elo.GetBase().Localizer, "rep_taken",
      target.First().PlayerName);

    if (source.Value == receiver.Value) return;

    Elo.GetReputationService()
     .AddReputation(source.Value, receiver.Value, false);
  }
}