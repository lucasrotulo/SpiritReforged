using SpiritReforged.Common.WallCommon;

namespace SpiritReforged.Content.Savanna.Walls;

public class DrywoodWall : ModWall, IAutoloadWallItem
{
	public void AddItemRecipes(ModItem item)
	{
		var mod = SpiritReforgedMod.Instance; //Mod is null here, so get the instance manually

		item.CreateRecipe(4)
			.AddIngredient(mod.Find<ModItem>("DrywoodWallItem").Type)
			.AddTile(TileID.WorkBenches)
			.Register();

		//Allow wall items to be crafted back into base materials
		Recipe.Create(mod.Find<ModItem>("DrywoodWallItem").Type)
			.AddIngredient(item.Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
	}

	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = true;
		DustType = DustID.WoodFurniture;
		AddMapEntry(new Color(107, 70, 50));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}