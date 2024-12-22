using SpiritReforged.Content.Ocean.Items.Blunderbuss;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Common.WorldGeneration.Micropasses.Discoveries.Passes;

internal class BlunderbussDiscovery : Discovery
{
	public override string WorldGenName => "Buried Blunderbuss";

	public override int GetWorldGenIndexInsert(List<GenPass> passes, List<Discovery> discoveries, ref bool afterIndex)
	{
		afterIndex = true;
		return passes.FindIndex(genpass => genpass.Name.Equals("Pots"));
	}

	public override void Run(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.Discoveries");

		retry:
		int x = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(GenVars.rightBeachStart, Main.maxTilesX) : WorldGen.genRand.Next(0, GenVars.leftBeachEnd);
		int y = (int)(Main.worldSurface * 0.35); //Sky height

		int type = ModContent.TileType<BlunderbussTile>();

		WorldMethods.FindGround(x, ref y);
		if (Main.tile[x, y - 1].LiquidAmount == 255)
			goto retry;

		WorldGen.PlaceTile(x, y - 1, type);

		if (Main.tile[x, y - 1].TileType != type)
			goto retry;
	}
}
