using SpiritReforged.Content.Forest.Botanist.Tiles;
using System.Linq;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Common.WorldGeneration.Micropasses.Discoveries.Passes;

internal class ScarecrowDiscovery : Discovery
{
	public override string WorldGenName => "Scarecrow";

	public override int GetWorldGenIndexInsert(List<GenPass> passes, List<Discovery> discoveries, ref bool afterIndex)
	{
		afterIndex = false;
		return passes.FindIndex(genpass => genpass.Name.Equals("Planting Trees")); //Generate before trees so we can have a wide open area
	}

	public override void Run(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.Scarecrow");
		int[] anchors = TileObjectData.GetTileData(ModContent.TileType<Wheatgrass>(), 0).AnchorValidTiles;

		retry:
		int x = WorldGen.genRand.Next(40, Main.maxTilesX / 3);
		if (WorldGen.genRand.NextBool())
			x = WorldGen.genRand.Next(Main.maxTilesX - Main.maxTilesX / 3, Main.maxTilesX - 40);

		int y = (int)(Main.worldSurface * 0.5f);
		if (WorldGen.remixWorldGen)
			y = WorldGen.genRand.Next((int)(Main.maxTilesY / 1.5f), Main.maxTilesY - 200);

		while (!Main.tile[x, y].HasTile || !anchors.Contains(Main.tile[x, y].TileType)) //Loop to valid ground
		{
			y++;
			if (!WorldGen.remixWorldGen && y > Main.worldSurface + 20 || y > Main.maxTilesY - 150)
				goto retry;
		}

		if (!TileObject.CanPlace(x, y - 1, ModContent.TileType<Scarecrow>(), 0, 0, out var _, true)
			|| Collision.WetCollision(new Vector2(x, y - 3) * 16, 16, 16 * 3)
			|| !WorldMethods.AreaClear(x - 1, y - 3, 3, 3))
			goto retry;

		const int Distance = 20;
		if (!GenVars.structures.CanPlace(new Rectangle(x - Distance, y, Distance * 2, 10)))
			goto retry;

		var start = new Point(x, y);
		for (x = start.X - Distance; x < start.X + Distance; x++)
		{
			y = start.Y - 30;
			while (!Main.tile[x, y].HasTile || !anchors.Contains(Main.tile[x, y].TileType)) //Loop to valid ground
				if (++y > Main.worldSurface + 100)
					break;

			if (Main.tile[x, y].HasTile && anchors.Contains(Main.tile[x, y].TileType))
				WorldGen.PlaceTile(x, y - 1, ModContent.TileType<Wheatgrass>(), true, style: Main.rand.Next(6));
		}

		ScarecrowTileEntity.Generate(start.X, start.Y - 1);
		GenVars.structures.AddProtectedStructure(new Rectangle(start.X - Distance, start.Y - 6, Distance * 2, 10), 2);
	}
}
