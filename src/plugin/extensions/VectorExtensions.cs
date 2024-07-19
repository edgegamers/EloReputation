using CounterStrikeSharp.API.Modules.Utils;

namespace EloReputation.plugin.extensions;

public static class VectorExtensions {
  public static Vector Clone(this Vector vector) {
    return new Vector(vector.X, vector.Y, vector.Z);
  }

  public static Vector Multiply(this Vector vector, float value) {
    vector.X *= value;
    vector.Y *= value;
    vector.Z *= value;
    return vector;
  }

  public static Vector Subtract(this Vector vector, Vector other) {
    vector.X -= other.X;
    vector.Y -= other.Y;
    vector.Z -= other.Z;
    return vector;
  }
}