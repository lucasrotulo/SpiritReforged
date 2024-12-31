using SpiritReforged.Common.Easing;

namespace SpiritReforged.Content.Particles;
public class SmokeCloud : DissipatingImage
{
	private readonly EaseFunction _acceleration;
	private readonly Vector2 _initialVel;

	public SmokeCloud(Vector2 position, Vector2 velocity, Color color, float scale, EaseFunction acceleration, int maxTime, bool useLightColor = true) : base(position, color, Main.rand.NextFloatDirection(), scale, 0.6f, "Smoke", maxTime)
	{
		UseLightColor = useLightColor;
		Velocity = velocity;
		_initialVel = velocity;
		_acceleration = acceleration;
	}

	public override void Update()
	{
		base.Update();
		Velocity = (1 - _acceleration.Ease(Progress)) * _initialVel;
	}
}
