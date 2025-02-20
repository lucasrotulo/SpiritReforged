using SpiritReforged.Common.TileCommon;
using SpiritReforged.Content.Jungle.Bamboo.Items;

namespace SpiritReforged.Content.Jungle.Bamboo.Tiles;

public class BambooPot : ModTile, IAutoloadTileItem
{
	public void SetItemDefaults(ModItem item)
	{
		item.Item.value = 50;
		item.Item.width = item.Item.height = 16;
	}

	public void AddItemRecipes(ModItem item) => item.CreateRecipe()
		.AddIngredient(ModContent.ItemType<StrippedBamboo>(), 5).AddTile(TileID.Sawmill).Register();

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.Height = 1;
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(100, 100, 60));
		DustType = DustID.PalmWood;
		AdjTiles = [TileID.ClayPot];

		PlanterHandler.PlanterTypes.Add(Type);
	}
}