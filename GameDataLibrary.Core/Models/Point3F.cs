namespace GameDataLibrary.Core.Models;

/// <summary>
/// Represents a 3D point with float coordinates (x, z, y)
/// </summary>
public class Point3F
{
    public float X { get; set; }
    public float Z { get; set; }
    public float Y { get; set; }

    public Point3F() { }

    public Point3F(float x, float z, float y)
    {
        X = x;
        Z = z;
        Y = y;
    }

    public override string ToString()
    {
        return $"{{x={X},z={Z},y={Y}}}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is Point3F other)
        {
            return Math.Abs(X - other.X) < float.Epsilon &&
                   Math.Abs(Z - other.Z) < float.Epsilon &&
                   Math.Abs(Y - other.Y) < float.Epsilon;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Z, Y);
    }
}