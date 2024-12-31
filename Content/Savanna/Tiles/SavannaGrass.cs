using SpiritReforged.Common.TileCommon;
using System.Linq;

namespace SpiritReforged.Content.Savanna.Tiles;

public class SavannaGrass : ModTile
{
	private static int DirtType => ModContent.TileType<SavannaDirt>();

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileMerge[Type][Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileNoFail[Type] = true;

		Main.tileMerge[DirtType][Type] = true;
		Main.tileMerge[Type][DirtType] = true;

		TileID.Sets.Grass[Type] = true;
		TileID.Sets.NeedsGrassFramingDirt[Type] = DirtType;
		TileID.Sets.CanBeDugByShovel[Type] = true;

		AddMapEntry(new Color(104, 156, 70));

		var data = TileObjectData.GetTileData(TileID.Sunflower, 0);
		data.AnchorValidTiles = data.AnchorValidTiles.Concat([Type]).ToArray(); //Allow sunflowers to be planted on this tile
	}

	public override bool CanExplode(int i, int j)
	{
		WorldGen.KillTile(i, j, false, false, true); //Makes the tile completely go away instead of reverting to dirt
		return true;
	}

	public override void RandomUpdate(int i, int j)
	{
		if (SpreadHelper.Spread(i, j, Type, 4, DirtType) && Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, i, j, 3, TileChangeType.None); //Try spread grass
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (!fail) //Change self into dirt
		{
			fail = true;
			Framing.GetTileSafely(i, j).TileType = (ushort)DirtType;
		}
	}

	public override void FloorVisuals(Player player)
	{
		if (player.flowerBoots) //Flower Boots functionality
		{
			var pos = ((player.Bottom - new Vector2(0, 8 * player.gravDir)) / 16).ToPoint16();

			if (!Main.tile[pos.X, pos.Y].HasTile)
			{
				WorldGen.PlaceTile(pos.X, pos.Y, ModContent.TileType<SavannaFoliage>(), true, style: Main.rand.Next(5));
				NetMessage.SendTileSquare(-1, pos.X, pos.Y);
			}
		}
	}
}
