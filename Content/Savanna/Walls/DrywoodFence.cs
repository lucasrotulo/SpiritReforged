using SpiritReforged.Common.WallCommon;

namespace SpiritReforged.Content.Savanna.Walls;

public class DrywoodFence : ModWall, IAutoloadWallItem
{
	public void AddItemRecipes(ModItem item)
	{
		var mod = SpiritReforgedMod.Instance; //Mod is null here, so get the instance manually

		item.CreateRecipe(4)
			.AddIngredient(mod.Find<ModItem>("DrywoodFenceItem").Type)
			.AddTile(TileID.WorkBenches)
			.Register();

		//Allow fence items to be crafted back into base materials
		Recipe.Create(mod.Find<ModItem>("DrywoodFenceItem").Type)
			.AddIngredient(item.Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
	}

	public override void SetStaticDefaults()
	{
		WallID.Sets.AllowsPlantsToGrow[Type] = true;
		Main.wallHouse[Type] = true;
		DustType = DustID.WoodFurniture;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}