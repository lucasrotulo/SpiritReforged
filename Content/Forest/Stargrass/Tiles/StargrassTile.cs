using SpiritReforged.Common.Particle;
using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.Visuals.Glowmasks;
using SpiritReforged.Common.WorldGeneration;
using SpiritReforged.Content.Particles;
using System.Linq;

namespace SpiritReforged.Content.Forest.Stargrass.Tiles;

[AutoloadGlowmask("Method:Content.Forest.Stargrass.Tiles.StargrassTile Glow")]
internal class StargrassTile : ModTile
{
	public static Color Glow(object obj) 
	{
		var pos = (Point)obj;
		float sine = (float)((Math.Sin(NoiseSystem.Perlin(pos.X * 1.2f, pos.Y * 0.2f) * 3f + Main.GlobalTimeWrappedHourly * 1.3f) + 1f) * 0.5f);
		return Color.White * MathHelper.Lerp(0.2f, 1f, sine);
	}

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileMerge[Type][Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileMerge[TileID.Dirt][Type] = true;
		Main.tileMerge[Type][TileID.Dirt] = true;
		Main.tileMerge[TileID.Grass][Type] = true;
		Main.tileMerge[Type][TileID.Grass] = true;

		TileID.Sets.Grass[Type] = true;
		TileID.Sets.Conversion.Grass[Type] = true;
		TileID.Sets.CanBeDugByShovel[Type] = true;

		AddMapEntry(new Color(28, 216, 151));
		DustType = DustID.Flare_Blue;

		var data = TileObjectData.GetTileData(TileID.Sunflower, 0);
		data.AnchorValidTiles = data.AnchorValidTiles.Concat([Type]).ToArray(); //Allow sunflowers to be planted on this tile
	}

	public override void FloorVisuals(Player player)
	{
		int chance = (int)Math.Clamp(50 - 7.5f * player.velocity.Length(), 1, 50);

		if (chance >= 1 && Main.rand.NextBool(chance))
			SpawnParticles(player);
	}

	internal static void SpawnParticles(Player player)
	{
		if (Main.rand.NextBool(5))
		{
			int type = DustID.YellowStarDust;
			Dust.NewDust(player.Bottom, player.width, 4, type, Main.rand.NextFloat(-1f, 1), Main.rand.NextFloat(-2f, -1f));
		}
		else
		{
			Vector2 velocity = new Vector2(0, -1).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.9f, 1.5f);
			bool left = true;

			ParticleHandler.SpawnParticle(new GlowParticle(player.Bottom + new Vector2(Main.rand.Next(player.width), 0), velocity,
				new Color(0, 157, 227) * 0.66f, Main.rand.NextFloat(0.35f, 0.5f), 60, 10, p =>
				{
					p.Velocity = p.Velocity.RotatedBy(left ? 0.1f : -0.1f);

					if (p.Velocity.Y > 0)
					{
						left = !left;
						p.Velocity = p.Velocity.RotatedBy(left ? 0.1f : -0.1f);
					}
				}));
		}
	}

	public override bool CanExplode(int i, int j)
	{
		WorldGen.KillTile(i, j, false, false, true); //Makes the tile completely go away instead of reverting to dirt
		return true;
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		//Surrounded by solid tiles
		if (WorldGen.SolidOrSlopedTile(i, j - 1) && WorldGen.SolidOrSlopedTile(i, j + 1) && WorldGen.SolidOrSlopedTile(i - 1, j) && WorldGen.SolidOrSlopedTile(i + 1, j))
		{
			Main.tile[i, j].TileType = TileID.Grass;
			return false;
		}

		return true;
	}

	public override void RandomUpdate(int i, int j)
	{
		if (!Framing.GetTileSafely(i, j - 1).HasTile && Main.rand.NextBool(4))
		{
			int style = Main.rand.Next(26);
			WorldGen.PlaceObject(i, j - 1, ModContent.TileType<StargrassFlowers>(), true, style);
			NetMessage.SendObjectPlacement(-1, i, j - 1, ModContent.TileType<StargrassFlowers>(), style, 0, -1, -1);
		}

		if (SpreadHelper.Spread(i, j, Type, 4, TileID.Dirt) && Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, i, j, 3, TileChangeType.None);
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => (r, g, b) = (0.05f, 0.2f, 0.5f);

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (!fail)
		{
			fail = true;
			Framing.GetTileSafely(i, j).TileType = TileID.Dirt;
		}
	}
}
