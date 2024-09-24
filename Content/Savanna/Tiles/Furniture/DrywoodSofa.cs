using SpiritReforged.Common.TileCommon;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;

namespace SpiritReforged.Content.Savanna.Tiles.Furniture;

public class DrywoodSofa : ModTile, IAutoloadTileItem
{
	private static bool WithinRange(int i, int j, Player player) => player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

	public void SetItemDefaults(ModItem item) => item.Item.value = Item.sellPrice(copper: 60);

	public void AddItemRecipes(ModItem item) => item.CreateRecipe()
		.AddIngredient<Items.Drywood.Drywood>(5)
		.AddIngredient(ItemID.Silk, 2)
		.AddTile(TileID.Sawmill)
		.Register();

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.addTile(Type);
		TileID.Sets.CanBeSatOnForNPCs[Type] = true;
		TileID.Sets.CanBeSatOnForPlayers[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.HasOutlines[Type] = true;

		AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bench"));
		AdjTiles = [TileID.Benches];
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
		}

		return true;
	}

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		if (WithinRange(i, j, player))
		{
			player.noThrow = 2;
			player.cursorItemIconID = Mod.Find<ModItem>(Name + "Item").Type;
			player.cursorItemIconEnabled = true;
		}
	}
}
