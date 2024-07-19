namespace EloReputation.api;

public interface IReputationService {
  Task<float> GetReputation(ulong steamId);

  Task<IEnumerable<(ulong, float)>> GetReputation(IEnumerable<ulong> steamIds) {
    return GetReputation(steamIds.ToArray());
  }

  Task<IEnumerable<(ulong, float)>> GetReputation(params ulong[] steamIds);

  Task<IEnumerable<(ulong, float)>> GetTopReputation(int limit = 10,
    int offset = 0);

  Task AddReputation(ulong giver, ulong receiver, bool positive = true);
}