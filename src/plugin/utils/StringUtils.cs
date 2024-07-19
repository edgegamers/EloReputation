using CounterStrikeSharp.API.Modules.Utils;

namespace EloReputation.plugin.utils;

public static class StringUtils {
  public static string ReplaceChatColors(string message) {
    if (!message.Contains('{')) return message;
    var modifiedValue = message;
    foreach (var field in typeof(ChatColors).GetFields()) {
      var pattern = $"{{{field.Name}}}";
      if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
        modifiedValue = modifiedValue.Replace(pattern,
          field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    return modifiedValue;
  }
}