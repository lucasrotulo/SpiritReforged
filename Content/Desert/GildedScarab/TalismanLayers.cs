using Terraria.DataStructures;

namespace SpiritReforged.Content.Desert.GildedScarab;

internal abstract class ScarabLayerBase : PlayerDrawLayer
{
	private static Asset<Texture2D> gildedScarabTexture;

	public override void Load()
	{
		if (!Main.dedServ)
			gildedScarabTexture = ModContent.Request<Texture2D>("SpiritReforged/Content/Desert/GildedScarab/GildedScarab_player");
	}

	protected abstract bool DrawConditions(ref PlayerDrawSet drawInfo);

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		if (drawInfo.shadow != 0f || !DrawConditions(ref drawInfo))
			return;

		Player drawPlayer = drawInfo.drawPlayer;
		Texture2D texture = gildedScarabTexture.Value;
		var size = new Vector2(68, 54);
		Vector2 origin = size / 2;
		Vector2 drawPos = drawPlayer.Center;
		Point tileLocation = Main.LocalPlayer.Center.ToTileCoordinates();

		var drawData = new DrawData(texture, drawPos - Main.screenPosition, new Rectangle(0, 56 * (drawPlayer.GetModPlayer<GildedScarabPlayer>().scarabTimer / 4), 68, 54), Lighting.GetColor(tileLocation), 0, origin, 1, SpriteEffects.None, 0);
		drawInfo.DrawDataCache.Add(drawData);
	}
}

internal class ScarabFrontLayer : ScarabLayerBase
{
	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ProjectileOverArm);

	protected override bool DrawConditions(ref PlayerDrawSet drawInfo)
	{
		Player drawPlayer = drawInfo.drawPlayer;
		return drawPlayer.HasBuff(ModContent.BuffType<GildedScarabBuff>()) && (drawPlayer.GetModPlayer<GildedScarabPlayer>().scarabTimer <= 12 || drawPlayer.GetModPlayer<GildedScarabPlayer>().scarabTimer >= 24);
	}
}

internal class ScarabBackLayer : ScarabLayerBase
{
	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ForbiddenSetRing);

	protected override bool DrawConditions(ref PlayerDrawSet drawInfo)
	{
		Player drawPlayer = drawInfo.drawPlayer;
		return drawPlayer.HasBuff(ModContent.BuffType<GildedScarabBuff>()) && (drawPlayer.GetModPlayer<GildedScarabPlayer>().scarabTimer >= 13 && drawPlayer.GetModPlayer<GildedScarabPlayer>().scarabTimer <= 23);
	}
}