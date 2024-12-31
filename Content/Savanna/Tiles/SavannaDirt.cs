using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.CheckItemUse;

namespace SpiritReforged.Content.Savanna.Tiles;

public class SavannaDirt : ModTile, IAutoloadTileItem, ICheckItemUse
{
	public void AddItemRecipes(ModItem item) => item.CreateRecipe()
			.AddIngredient(ItemID.SandBlock)
			.AddIngredient(ItemID.MudBlock)
			.Register();

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileMerge[TileID.Sand][Type] = true;

		Main.tileMerge[TileID.ClayBlock][Type] = true; //Two-way clay merge
		Main.tileMerge[Type][TileID.ClayBlock] = true;

		Main.tileMerge[TileID.Stone][Type] = true; //Two-way stone merge
		Main.tileMerge[Type][TileID.Stone] = true;

		TileID.Sets.ChecksForMerge[Type] = true;

		AddMapEntry(new Color(138, 79, 45));
		MineResist = .5f;
	}

	public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
		=> WorldGen.TileMergeAttempt(-2, TileID.Sand, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);

	public bool? CheckItemUse(int type, int i, int j)
	{
		if (type == ItemID.StaffofRegrowth) //Staff of Regrowth functionality
		{
			WorldGen.PlaceTile(i, j, ModContent.TileType<SavannaGrass>(), forced: true);
			NetMessage.SendTileSquare(-1, i, j);
			return true;
		}

		return null;
	}
}