using System.Numerics;

namespace GameHelper.Utils;

public static class MathHelper
{
	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
	{
		return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
	}
}
