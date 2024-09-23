using SpiritReforged.Common.TileCommon;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;

namespace SpiritReforged.Content.Savanna.Tiles.Furniture;

public class DrywoodChair : ModTile, IAutoloadTileItem
{
	private static bool WithinRange(int i, int j, Player player) => player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.CanBeSatOnForNPCs[Type] = true;
		TileID.Sets.CanBeSatOnForPlayers[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.CoordinateWidth = 18;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		RegisterItemDrop(Mod.Find<ModItem>(Name + "Item").Type);
		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
		AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Chair"));
		AdjTiles = [TileID.Chairs];
		DustType = -1;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => WithinRange(i, j, settings.player);

	public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		if (WithinRange(i, j, player))
		{
			player.GamepadEnableGrappleCooldown();
			player.sitting.SitDown(player, i, j);
			return true;
		}

		return false;
	}

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		if (WithinRange(i, j, player))
		{
			player.noThrow = 2;
			player.cursorItemIconID = Mod.Find<ModItem>(Name + "Item").Type;
			player.cursorItemIconEnabled = true;

			if (Framing.GetTileSafely(i, j).TileFrameX / 20 < 1)
				player.cursorItemIconReversed = true;
		}
	}

	public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info) => info.TargetDirection = (Framing.GetTileSafely(i, j).TileFrameX == 0) ? -1 : 1;
}
