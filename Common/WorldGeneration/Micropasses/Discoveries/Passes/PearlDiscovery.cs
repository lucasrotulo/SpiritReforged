using SpiritReforged.Content.Ocean.Items.Pearl;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Common.WorldGeneration.Micropasses.Discoveries.Passes;

internal class PearlDiscovery : Discovery
{
	public override string WorldGenName => "Pearl String";

	public override int GetWorldGenIndexInsert(List<GenPass> passes, List<Discovery> discoveries, ref bool afterIndex)
	{
		afterIndex = true;
		return passes.FindIndex(genpass => genpass.Name.Equals("Pots"));
	}

	public override void Run(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.Discoveries");

		while (true)
		{
			int x = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(GenVars.rightBeachStart, Main.maxTilesX) : WorldGen.genRand.Next(0, GenVars.leftBeachEnd);
			int y = (int)(Main.worldSurface * 0.35); //Sky height

			int type = ModContent.TileType<PearlStringTile>();

			WorldMethods.FindGround(x, ref y);
			if (Main.tile[x, y - 1].LiquidAmount == 255)
				continue;

			WorldGen.PlaceTile(x, y - 1, type);

			if (Main.tile[x, y - 1].TileType != type)
				continue;

			break;
		}
	}
}
