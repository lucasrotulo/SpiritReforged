using SpiritReforged.Common.TileCommon;

namespace SpiritReforged.Content.Savanna.Tiles;

public class DrywoodPlatform : ModTile, IAutoloadTileItem
{
	public void AddItemRecipes(ModItem item)
	{
		item.CreateRecipe(2)
			.AddIngredient(ModContent.ItemType<Items.Drywood.Drywood>())
			.Register();

		//Allow platform items to be crafted back into base materials
		Recipe.Create(ModContent.ItemType<Items.Drywood.Drywood>())
			.AddIngredient(item.Type, 2)
			.AddTile(TileID.WorkBenches)
			.Register();
	}

	public override void SetStaticDefaults()
	{
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileSolidTop[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.Platforms[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleMultiplier = 27;
		TileObjectData.newTile.StyleWrapLimit = 27;
		TileObjectData.newTile.UsesCustomCanPlace = false;
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
		AddMapEntry(new Color(179, 146, 107));
		DustType = DustID.WoodFurniture;
		AdjTiles = [TileID.Platforms];
	}

	public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 9;
}