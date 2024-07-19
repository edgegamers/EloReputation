using CounterStrikeSharp.API.Core;
using EloReputation.plugin;

namespace EloReputation.api;

public interface IEloPlugin : IPluginConfig<EloConfig> {
  BasePlugin GetBase();
  IReputationService GetReputationService();
  IRateLimiter<ulong> GetMapLimiter();
  IRateLimiter<ulong> GetPeriodLimiter();
}