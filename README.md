# EloReputation

A system for allowing players to hand out reputation points to each other.

This project, its API, documentation, etc. is/are in active development, and thus no stability is guaranteed.
Refer to the [LICENSE](LICENSE) for more information.

## Installation

Our releases follow the [Semantic Versioning](https://semver.org) standard, and can be
found [here](https://github.com/edgegamers/EloReputation/releases).

1. Download the latest release from the [releases page](https://github.com/edgegamers/EloReputation/releases). <br>
   Note: The latest release is not necessarily the one that EdgeGamers is using. We pull the latest build from `main`,
   which is typically ahead of the latest release.
2. Extract the contents of the downloaded archive to your server's `addons/counterstrikesharp/plugins/EloReputation`
   directory.
3. Configure the `plugin/EloReputation/EloReputation.json` file. Of importance is the `DatabaseConnectionString` field.
   This field should be set to the connection string of your MySQL database. The plugin will not work without this. <br>
   Example: `"DatabaseConnectionString": "server=localhost;database=eloreputation;user=root;password=;port=3306;"`

## Features

### Reputation System

As of this writing, there is **one** reputation system, which keeps track of who has given reputation to whom.
Additional support of reputation systems is _planned_, but not guaranteed.

- [X] Harmonic Reputation System
  - All players start with a reputation of 0.
  - When a player does `css_up` on a target, that player's reputation increases by `\sum_{i=1}^{n} \frac{1}{i}`,
    where `n` is the number of times the player has commended the target.
  - Similarly, when a player does `css_down` on a target, that player's reputation decreases
    by `\sum_{i=1}^{n} \frac{1}{i}`,
    where `n` is the number of times the player has condemned the target.
  - Thus, the stats tracked are who has given (distinct positive/negative) reputation to whom.
- [ ] Simple Reputation System
  - All players start with a reputation of 0.
  - A player doing `css_up` on another player will increase the target player's reputation by 1.
  - A player doing `css_down` on another player will decrease the target player's reputation by 1.
  - Thus, the stats tracked are each individual player's reputation.
- [ ] ELO Reputation System
  - All players start with some default ELO value.
  - A player doing `css_up` on another player will increase the target player's ELO by some amount.
  - A player doing `css_down` on another player will decrease the target player's ELO by some amount.
  - The amount by which the ELO changes is determined with the ELO Expected Score formula.<br>
    ![ELO Expected Score Formula](https://latex.codecogs.com/svg.image?%20E_A=%5Cfrac%7B1%7D%7B1&plus;10%5Cleft(%5Cfrac%7BR_B-R_A%7D%7B400%7D%5Cright)%7D)
  - Thus, the stats tracked are each individual player's ELO.

### Commands

`<>` denotes a required argument, `[]` denotes an optional argument.

- `css_up <player>` -
- `css_down <player>`
- `css_reputation [player]`
- `css_top`

### Configurable Cooldowns

To discourage abuse, and encourage thoughtful reputation distribution, there are two cooldowns in place.
Refer to the

**Map Cooldown** resets on map change.

- On map change, all players are given a number of reputation points depending on the config.

**Periodic Cooldown** increments every 5 minutes.

- When a player first joins, they are alloted a number of reputation points depending on the config.
- When a player does `css_up` or `css_down`, they are deducted one reputation point.
- Every 5 minutes, all players are given a reputation point, up to the maximum allowed by the config.

Consequently, you can set both short-burst and long-term cooldowns.

### Example Configuration

```json
{
  "DatabaseConnectionString": "server=localhost;database",
  "DatabaseTablePrefix": "eloreputation_",
  "DatabaseTablePrefix": {
    "": 1,
    "@css/generic": 2,
    "@css/chat": 3,
    "@css/voice": 3,
    "@css/ban": 4,
    "@css/root": 99
  },
  "MaxPeriodicElo" {
    "": 1,
    "@css/generic": 1,
    "@css/chat": 1,
    "@css/voice": 2,
    "@css/ban": 2,
    "@css/root": 5
  }
}
```