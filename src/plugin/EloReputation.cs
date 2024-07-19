using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using EloReputation.api;
using EloReputation.plugin.commands.elo;
using EloReputation.plugin.services;

namespace EloReputation.plugin;

public class EloReputation : BasePlugin, IEloPlugin {
  private IRateLimiter<ulong> rateLimiter = null!;
  private IReputationService repService = null!;
  public override string ModuleName => "EloReputation";
  public override string ModuleVersion => "0.0.1";
  public EloConfig Config { get; set; } = null!;

  public void OnConfigParsed(EloConfig config) { Config = config; }
  public BasePlugin getBase() { throw new NotImplementedException(); }
  public IReputationService getReputationService() { return repService; }
  public IRateLimiter<ulong> getRateLimiter() { return rateLimiter; }

  public override void Load(bool hotReload) {
    repService  = new ReputationService(this);
    rateLimiter = new RateLimiter(this);

    registerCmds();
  }

  private void registerCmds() {
    CommandInfo.CommandCallback up   = new UpCommand(this).OnCommand;
    CommandInfo.CommandCallback down = new DownCommand(this).OnCommand;
    CommandInfo.CommandCallback rep  = new RepCommand(this).OnCommand;
    AddCommand("css_up", "", up);
    AddCommand("css_commend", "", up);
    AddCommand("css_like", "", up);
    AddCommand("css_down", "", down);
    AddCommand("css_denounce", "", down);
    AddCommand("css_dislike", "", down);
    AddCommand("css_rep", "", rep);
    AddCommand("css_reputation", "", rep);
    AddCommand("css_elo", "", rep);
  }
}