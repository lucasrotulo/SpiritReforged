using SpiritReforged.Common.TileCommon;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;

namespace SpiritReforged.Content.Forest.ButterflyStaff;

public class ButterflyStump : ModTile
{
	private static Asset<Texture2D> glowTexture;
	private const int FrameHeight = 18 * 4;

	private static int ItemType => ModContent.ItemType<ButterflyStaff>();
	private static bool HasItem(int i, int j) => Framing.GetTileSafely(i, j).TileFrameY < FrameHeight;
	private static bool TopHalf(int i, int j) => Framing.GetTileSafely(i, j).TileFrameY % FrameHeight < 18 * 2;

	public override void Load() => glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");

	public override void SetStaticDefaults()
	{
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileWaterDeath[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.InteractibleByNPCs[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Origin = new Point16(1, 3);
		TileObjectData.newTile.Height = 4;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(212, 125, 93));
		RegisterItemDrop(ItemType);

		AnimationFrameHeight = FrameHeight;
		DustType = DustID.WoodFurniture;
	}

	public override bool CreateDust(int i, int j, ref int type) => !TopHalf(i, j);
	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => HasItem(i, j);
	public override bool CanKillTile(int i, int j, ref bool blockDamaged) => !TopHalf(i, j) || HasItem(i, j);
	public override bool CanDrop(int i, int j) => HasItem(i, j);

	public override void MouseOver(int i, int j)
	{
		if (!HasItem(i, j))
			return;

		Player player = Main.LocalPlayer;
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
		player.cursorItemIconID = ItemType;
	}

	public override bool RightClick(int i, int j)
	{
		if (HasItem(i, j))
		{
			TileExtensions.GetTopLeft(ref i, ref j);

			for (int x = i; x < i + 2; x++)
				for (int y = j; y < j + 4; y++)
					Main.tile[x, y].TileFrameY += FrameHeight;

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, i, j, 2, 4);

			int item = Item.NewItem(null, new Vector2(i + 1, j) * 16, ItemType);
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendData(MessageID.SyncItem, number: item);

			return true;
		}

		return false;
	}

	public override void AnimateTile(ref int frame, ref int frameCounter)
	{
		if (++frameCounter >= 4)
		{
			frameCounter = 0;
			frame = ++frame % 8;
		}
	}

	public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
	{
		if (HasItem(i, j))
			frameYOffset = Main.tileFrame[type] * FrameHeight;
		else
			frameYOffset = FrameHeight * 7; //Don't animate
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (!HasItem(i, j))
			return;

		var color = new Vector3(255, 125, 255) * .001f;
		(r, g, b) = (color.X, color.Y, color.Z);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		var tile = Main.tile[i, j];
		if (!TileDrawing.IsVisible(tile))
			return;

		if (HasItem(i, j))
		{
			var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
			int addFrameX = 0, addFrameY = 0;

			TileLoader.SetAnimationFrame(Type, i, j, ref addFrameX, ref addFrameY);

			var source = new Rectangle(tile.TileFrameX, tile.TileFrameY + addFrameY, 16, 16);
			var position = new Vector2(i, j) * 16 - Main.screenPosition + zero + new Vector2(0, 2);

			spriteBatch.Draw(glowTexture.Value, position, source, (Color.White with { A = 0 }) * .2f, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}
	}
}