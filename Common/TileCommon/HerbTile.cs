using SpiritReforged.Common.TileCommon.CheckItemUse;
using SpiritReforged.Content.Forest.Botanist.Items;
using System.Linq;
using Terraria.GameContent.Metadata;

namespace SpiritReforged.Common.TileCommon;

public abstract class HerbTile : ModTile, ICheckItemUse
{
	public enum PlantStage : byte
	{
		Planted,
		Growing,
		Grown
	}

	private const int FrameWidth = 18; // A constant for readability and to kick out those magic numbers

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileObsidianKill[Type] = true;
		Main.tileCut[Type] = true;
		Main.tileNoFail[Type] = true;
		TileID.Sets.ReplaceTileBreakUp[Type] = true;
		TileID.Sets.IgnoredInHouseScore[Type] = true;
		TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
		TileID.Sets.SwaysInWindBasic[Type] = true;
		TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

		HitSound = SoundID.Grass;
		DustType = DustID.Grass;

		StaticDefaults();
	}

	public virtual void StaticDefaults() { }

	public virtual bool CanBeHarvested(int i, int j) => Main.tile[i, j].HasTile && GetStage(i, j) == PlantStage.Grown;

	public bool? CheckItemUse(int type, int i, int j)
	{
		if (type == ItemID.StaffofRegrowth)
		{
			int herbType = TagGlobalTile.HarvestableHerbs.FirstOrDefault(x => x == Main.tile[i, j].TileType);

			if (herbType != default)
			{
				var herb = ModContent.GetModTile(type) as HerbTile;

				if (herb.CanBeHarvested(i, j))
				{
					WorldGen.KillTile(i, j);
					return true;
				}
			}
		}

		return null;
	}

	public override bool CanPlace(int i, int j)
	{
		Tile tile = Framing.GetTileSafely(i, j);

		if (!tile.HasTile)
			return true;

		int tileType = tile.TileType;
		if (tileType == Type)
		{
			PlantStage stage = GetStage(i, j);
			return stage == PlantStage.Grown;
		}
		else
		{
			if (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType] || tileType == TileID.WaterDrip || tileType == TileID.LavaDrip || tileType == TileID.HoneyDrip || tileType == TileID.SandDrip)
			{
				bool foliageGrass = tileType is TileID.Plants or TileID.Plants2;
				bool moddedFoliage = tileType >= TileID.Count && (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType]);
				bool harvestableVanillaHerb = Main.tileAlch[tileType] && WorldGen.IsHarvestableHerbWithSeed(tileType, tile.TileFrameX / 18);

				if (foliageGrass || moddedFoliage || harvestableVanillaHerb)
				{
					WorldGen.KillTile(i, j);

					if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient)
						NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);

					return true;
				}
			}

			return false;
		}
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		=> spriteEffects = (i % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = -2;

	public sealed override IEnumerable<Item> GetItemDrops(int i, int j)
	{
		PlantStage stage = GetStage(i, j);

		int herbItemStack = 0;
		int seedItemStack = 0;

		if (stage != PlantStage.Planted)
		{
			Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
			Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];

			if (nearestPlayer.active && nearestPlayer.HeldItem.type == ItemID.StaffofRegrowth) // Increased yields with Staff of Regrowth, even when not fully grown
			{
				if (stage == PlantStage.Grown)
					(herbItemStack, seedItemStack) = (2, Main.rand.Next(2, 5));
				else if (stage == PlantStage.Growing)
					seedItemStack = Main.rand.Next(1, 3);
			}
			else if (stage == PlantStage.Grown)
				(herbItemStack, seedItemStack) = (1, Main.rand.Next(1, 4));
			else if (stage == PlantStage.Growing)
				herbItemStack = 1;

			if (nearestPlayer.GetModPlayer<BotanistPlayer>().active && stage != PlantStage.Planted)
			{
				seedItemStack += 2;
				herbItemStack++;
			}
		}

		return ItemDrops(i, j, herbItemStack, seedItemStack);
	}

	public virtual IEnumerable<Item> ItemDrops(int i, int j, int herbStack, int seedStack) => null;

	public override bool IsTileSpelunkable(int i, int j) => GetStage(i, j) == PlantStage.Grown;

	public override void RandomUpdate(int i, int j)
	{
		Tile tile = Framing.GetTileSafely(i, j);
		PlantStage stage = GetStage(i, j);

		if (stage == PlantStage.Planted) //Grow only if just planted
		{
			tile.TileFrameX += FrameWidth;

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, i, j, 1);
		}
	}

	// A helper method to quickly get the current stage of the herb (assuming the tile at the coordinates is our herb)
	public static PlantStage GetStage(int i, int j)
	{
		Tile tile = Framing.GetTileSafely(i, j);
		return (PlantStage)(tile.TileFrameX / FrameWidth);
	}

	// A helper method to quickly set the current stage of the herb (assuming the tile at the coordinates is our herb)
	public static void SetStage(int i, int j, PlantStage stage)
	{
		Tile tile = Framing.GetTileSafely(i, j);
		tile.TileFrameX = (short)(FrameWidth * (int)stage);
	}
}
