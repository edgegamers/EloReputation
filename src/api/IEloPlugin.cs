using CounterStrikeSharp.API.Core;

namespace EloReputation.api;

public interface IEloPlugin : IPluginConfig<EloConfig> {
  BasePlugin getBase();
  IReputationService getReputationService();
  IRateLimiter<ulong> getRateLimiter();
}