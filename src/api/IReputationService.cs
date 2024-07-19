namespace EloReputation.api;

public interface IReputationService {
  Task<double> GetReputation(ulong steamId);

  Task<IEnumerable<(ulong, double)>>
    GetReputation(IEnumerable<ulong> steamIds) {
    return GetReputation(steamIds.ToArray());
  }

  Task<IEnumerable<(ulong, double)>> GetReputation(params ulong[] steamIds);

  Task<IEnumerable<(ulong, double)>> GetTopReputation(int limit = 10,
    int offset = 0);
  
  Task<(int, int)> GetReputationPosition(ulong steamId);

  Task AddReputation(ulong giver, ulong receiver, bool positive = true);
}