using SpiritReforged.Common.Particle;
using SpiritReforged.Common.Easing;

namespace SpiritReforged.Content.Particles;

public class ImpactLine : Particle
{
	private readonly Entity _ent = null;

	private Color _color;
	private Vector2 _scaleMod;
	private Vector2 _offset;

	public override ParticleDrawType DrawType => ParticleDrawType.Custom;

	public ImpactLine(Vector2 position, Vector2 velocity, Color color, Vector2 scale, int timeLeft, Entity attatchedEntity = null)
	{
		Position = position;
		Velocity = velocity;
		_color = color;
		_scaleMod = scale;
		MaxTime = timeLeft;
		_ent = attatchedEntity;

		if(_ent != null)
			_offset = Position - _ent.Center;
	}

	public override void Update()
	{
		float opacity = EaseFunction.EaseQuadOut.Ease(EaseFunction.EaseSine.Ease(Progress));
		Color = _color * opacity;
		Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
		Lighting.AddLight(Position, Color.ToVector3() / 2f);

		if(_ent != null)
		{
			if (!_ent.active)
			{
				Kill();
				return;
			}

			Position = _ent.Center + _offset;
			_offset += Velocity;
		}
	}

	public override void CustomDraw(SpriteBatch spriteBatch)
	{
		float progress = EaseFunction.EaseSine.Ease(Progress);
		var scale = new Vector2(0.5f, progress) * _scaleMod;
		var offset = Vector2.Zero;
		var tex = ParticleHandler.GetTexture(Type);
		var origin = new Vector2(tex.Width / 2, tex.Height / 2);

		spriteBatch.Draw(tex, Position + offset - Main.screenPosition, null, Color * (progress / 5 + 0.8f), Rotation, origin, scale, SpriteEffects.None, 0);
	}
}
