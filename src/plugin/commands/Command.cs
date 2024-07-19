using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using EloReputation.api;
using EloReputation.plugin.extensions;

namespace plugin.commands;

public abstract class Command(IEloPlugin elo) {
  protected readonly IEloPlugin elo = elo;

  public string? Description => null;

  public abstract void OnCommand(CCSPlayerController? executor,
    CommandInfo info);

  protected TargetResult? GetTarget(CommandInfo command, int argIndex = 1,
    Func<CCSPlayerController, bool>? predicate = null) {
    var matches = command.GetArgTargetResult(argIndex);

    matches.Players = matches.Players.Where(player => player is {
        IsValid: true, Connected: PlayerConnectedState.PlayerConnected
      })
     .ToList();
    if (predicate != null)
      matches.Players = matches.Players.Where(predicate).ToList();

    if (!matches.Any()) {
      command.ReplyLocalized(elo.getBase().Localizer, "player_not_found",
        command.GetArg(argIndex));
      return null;
    }

    if (matches.Count() > 1 && command.GetArg(argIndex).StartsWith('@'))
      return matches;

    if (matches.Count() == 1 || !command.GetArg(argIndex).StartsWith('@'))
      return matches;

    command.ReplyLocalized(elo.getBase().Localizer, "player_found_multiple",
      command.GetArg(argIndex));
    return null;
  }

  private TargetResult? getVulnerableTarget(CommandInfo command,
    int argIndex = 1, Func<CCSPlayerController, bool>? predicate = null) {
    return GetTarget(command, argIndex,
      p => (command.CallingPlayer == null || command.CallingPlayer.CanTarget(p))
        && p.IsReal() && (predicate == null || predicate(p)));
  }

  private TargetResult? getSingleTarget(CommandInfo command, int argIndex = 1) {
    var matches = command.GetArgTargetResult(argIndex);

    if (!matches.Any()) {
      command.ReplyLocalized(elo.getBase().Localizer, "player_not_found",
        command.GetArg(argIndex));
      return null;
    }

    if (matches.Count() <= 1) return matches;
    command.ReplyLocalized(elo.getBase().Localizer, "player_found_multiple",
      command.GetArg(argIndex));
    return null;
  }

  internal string GetTargetLabel(CommandInfo info, int argIndex = 1) {
    switch (info.GetArg(argIndex)) {
      case "@all":
        return "all players";
      case "@bots":
        return "all bots";
      case "@humans":
        return "all humans";
      case "@alive":
        return "alive players";
      case "@dead":
        return "dead players";
      case "@!me":
        return "all except self";
      case "@me":
        return info.CallingPlayer == null ?
          "Console" :
          info.CallingPlayer.PlayerName;
      case "@ct":
        return "all CTs";
      case "@t":
        return "all Ts";
      case "@spec":
        return "all spectators";
      default:
        var player = info.GetArgTargetResult(argIndex).FirstOrDefault();
        return player != null ? player.PlayerName : "unknown";
    }
  }
}