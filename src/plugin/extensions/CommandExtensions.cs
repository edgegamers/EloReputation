using CounterStrikeSharp.API.Modules.Commands;
using EloReputation.plugin.utils;
using Microsoft.Extensions.Localization;

namespace EloReputation.plugin.extensions;

public static class CommandExtensions {
  public static void ReplyLocalized(this CommandInfo cmd,
    IStringLocalizer localizer, string local, params object[] args) {
    string message = localizer[local, args];
    message = message.Replace("%prefix%", localizer["prefix"]);
    message = StringUtils.ReplaceChatColors(message);
    cmd.ReplyToCommand(message);
  }

  public static void Reply(this CommandInfo cmd, string message,
    params object[] args) {
    message = string.Format(message, args);
    message = StringUtils.ReplaceChatColors(message);
    cmd.ReplyToCommand(message);
  }
}