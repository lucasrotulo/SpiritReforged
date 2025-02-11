﻿using SpiritReforged.Common.TileCommon;
using SpiritReforged.Content.Savanna.Items;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Tiles;

public class SavannaRockLarge : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoFail[Type] = true;

		TileID.Sets.BreakableWhenPlacing[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new(2, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(100, 92, 80));
		DustType = DustID.Stone;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;
	public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) => wormChance = 6;
}

public class SavannaRockSmall : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoFail[Type] = true;

		TileID.Sets.BreakableWhenPlacing[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.newTile.Origin = new(1, 0);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(100, 92, 80));
		DustType = DustID.Stone;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;
	public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) => wormChance = 6;
}

public class SavannaRockLargeRubble : SavannaRockLarge, IRubble
{
	public override string Texture => base.Texture.Remove(base.Texture.Length - 6, 6); //Remove "Rubble"
	IRubble.RubbleData IRubble.Data => new(ModContent.ItemType<SavannaGrassSeeds>(), IRubble.RubbleSize.Large);

	public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) { }
}

public class SavannaRockSmallRubble : SavannaRockSmall, IRubble
{
	public override string Texture => base.Texture.Remove(base.Texture.Length - 6, 6); //Remove "Rubble"
	IRubble.RubbleData IRubble.Data => new(ModContent.ItemType<SavannaGrassSeeds>(), IRubble.RubbleSize.Small);

	public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) { }
}