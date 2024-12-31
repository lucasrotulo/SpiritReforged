using Terraria.DataStructures;

namespace SpiritReforged.Content.Desert.GildedScarab;

internal abstract class ScarabLayerBase : PlayerDrawLayer
{
	private static Asset<Texture2D> gildedScarabTexture;

	private float opacity;
	private float visualCounter;

	public override void Load() => gildedScarabTexture = Mod.Assets.Request<Texture2D>("Content/Desert/GildedScarab/GildedScarab_Player");

	protected abstract bool CanDraw(int visualCounter);

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		const int numFramesY = 8;
		const int frameDuration = 8;

		if (drawInfo.shadow != 0f)
			return;

		var player = drawInfo.drawPlayer;

		if (player.HasBuff<GildedScarabBuff>())
			opacity = MathHelper.Min(opacity + .05f, 1);
		else
			opacity = MathHelper.Max(opacity - .05f, 0);

		if (opacity > 0)
		{
			if (!Main.gamePaused)
				visualCounter = (visualCounter + 1f / frameDuration) % numFramesY;

			if (!CanDraw((int)visualCounter))
				return;

			var texture = gildedScarabTexture.Value;
			var source = texture.Frame(1, numFramesY, 0, (int)visualCounter, sizeOffsetY: -2);
			var position = player.Center - Main.screenPosition + new Vector2(-(float)Math.Sin(visualCounter / numFramesY * MathHelper.TwoPi) * 22f, 0);
			var color = Lighting.GetColor(player.Center.ToTileCoordinates()) * opacity;

			var drawData = new DrawData(texture, position, source, color, 0, source.Size() / 2, 1, SpriteEffects.None, 0);
			drawInfo.DrawDataCache.Add(drawData);
		}
	}
}

internal class ScarabFrontLayer : ScarabLayerBase
{
	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ProjectileOverArm);
	protected override bool CanDraw(int visualCounter) => visualCounter is 0 or 1 or 2 or 7;
}

internal class ScarabBackLayer : ScarabLayerBase
{
	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ForbiddenSetRing);
	protected override bool CanDraw(int visualCounter) => visualCounter is 3 or 4 or 5 or 6;
}