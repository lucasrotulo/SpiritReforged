using SpiritReforged.Content.Forest.Safekeeper;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Common.WorldGeneration.Micropasses.Discoveries.Passes;

internal class SafekeeperDiscovery : Discovery
{
	public override string WorldGenName => "Safekeeper's Grave";

	public override int GetWorldGenIndexInsert(List<GenPass> passes, List<Discovery> discoveries, ref bool afterIndex)
	{
		afterIndex = false;
		return passes.FindIndex(genpass => genpass.Name.Equals("Piles"));
	}

	public override void Run(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.Discoveries");

		retry:
		int x = WorldGen.genRand.Next(GenVars.leftBeachEnd, GenVars.rightBeachStart);
		int y = (int)(Main.worldSurface * 0.35); //Sky height

		WorldMethods.FindGround(x, ref y);
		if (Main.tile[x, y].TileType != TileID.Grass || Main.tile[x, y - 1].LiquidAmount == 255 || !WorldMethods.AreaClear(x - 1, y - 2, 3, 2))
			goto retry;

		WorldGen.PlaceTile(x, y - 1, TileID.Tombstones, true, true, style: WorldGen.genRand.Next(5));
		if (Main.tile[x, y - 1].TileType != TileID.Tombstones)
			goto retry;

		WorldGen.PlaceTile(x - 1, y, TileID.Dirt, true, true);
		WorldGen.PlaceTile(x - 1, y - 1, ModContent.TileType<SkeletonHand>(), true, true, style: WorldGen.genRand.Next(3));

		GenVars.structures.AddProtectedStructure(new Rectangle(x - 1, y - 2, 3, 3));
	}
}
