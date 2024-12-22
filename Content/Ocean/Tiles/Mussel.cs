using SpiritReforged.Common.TileCommon;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Ocean.Tiles;

public class Mussel : ModTile, IAutoloadTileItem
{
	private const int styleRange = 3;

	public void SetItemDefaults(ModItem item)
	{
		item.Item.width = item.Item.height = 16;
		item.Item.rare = ItemRarityID.Blue;
	}

	public void AddItemRecipes(ModItem item) => item.CreateRecipe()
			.AddIngredient(ItemID.Coral, 2)
			.AddIngredient(ItemID.Wood, 1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileNoAttach[Type] = true;
		Main.tileNoFail[Type] = true;
		Main.tileWaterDeath[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.AnchorAlternateTiles = [TileID.WoodenBeam];
		TileObjectData.newTile.RandomStyleRange = styleRange;
		TileObjectData.newTile.StyleHorizontal = true;

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(styleRange);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(styleRange * 2);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(styleRange * 3);

		TileObjectData.addTile(Type);

		RegisterItemDrop(Mod.Find<ModItem>("MusselItem").Type);
		AddMapEntry(new Color(200, 200, 200));
		DustType = DustID.Stone;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(1, 3);
}
