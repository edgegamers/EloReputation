namespace EloReputation.api;

/// <summary>
///  Basically a semaphore
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRateLimiter<in T> {
  void Reset();
  void Reset(T key);

  [Obsolete("This is a semaphore, are you sure you want to increment?")]
  int Increment(T key);

  int Get(T key);
  bool Decrement(T key);
}