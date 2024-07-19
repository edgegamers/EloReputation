using EloReputation.api;
using EloReputation.plugin.commands.elo;
using EloReputation.plugin.services;
using MySqlConnector;

namespace EloReputation.plugin;

using CounterStrikeSharp.API.Core;

public class EloReputation : BasePlugin, IEloPlugin {
  public override string ModuleName => "EloReputation";
  public override string ModuleVersion => "0.0.1";
  public EloConfig Config { get; set; } = null!;
  private IReputationService repService = null!;
  private IRateLimiter<ulong> rateLimiter = null!;

  public override void Load(bool hotReload) {
    repService  = new ReputationService(this);
    rateLimiter = new RateLimiter(this);

    registerCmds();
  }

  private void registerCmds() {
    AddCommand("css_up", "", new UpCommand(this).OnCommand);
    AddCommand("css_down", "", new DownCommand(this).OnCommand);
  }

  public void OnConfigParsed(EloConfig config) { Config = config; }
  public BasePlugin getBase() { throw new NotImplementedException(); }
  public IReputationService getReputationService() { return repService; }
  public IRateLimiter<ulong> getRateLimiter() { return rateLimiter; }
}