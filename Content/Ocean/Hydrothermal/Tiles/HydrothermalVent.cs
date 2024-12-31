using SpiritReforged.Common.Easing;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.Particle;
using SpiritReforged.Common.PlayerCommon;
using SpiritReforged.Common.TileCommon;
using SpiritReforged.Content.Particles;
using Terraria.Audio;
using Terraria.DataStructures;
using static SpiritReforged.Common.Misc.ReforgedMultiplayer;

namespace SpiritReforged.Content.Ocean.Hydrothermal.Tiles;

public class HydrothermalVent : ModTile
{
	internal static readonly Dictionary<Point16, int> cooldowns = [];
	internal const int cooldownMax = (int)(Main.dayLength / 5);
	internal const int eruptDuration = 60 * 10;

	private static readonly Point[] tops = [new Point(16, 16), new Point(16, 16), new Point(16, 24), new Point(12, 4), new Point(20, 4), new Point(16, 16), new Point(16, 16), new Point(16, 16)];

	public override void Load() => On_Wiring.UpdateMech += UpdateCooldowns;

	private void UpdateCooldowns(On_Wiring.orig_UpdateMech orig)
	{
		orig();

		foreach (var entry in cooldowns)
		{
			if (!IsValid(entry.Key.X, entry.Key.Y))
			{
				cooldowns.Remove(new Point16(entry.Key.X, entry.Key.Y));
				break;
			}

			if (cooldowns[entry.Key] > 0)
				cooldowns[entry.Key]--;
		}
	}

	public static void Erupt(int i, int j)
	{
		var t = Framing.GetTileSafely(i, j);
		int fullWidth = TileObjectData.GetTileData(t).CoordinateFullWidth;
		var position = new Vector2(i, j) * 16 + tops[t.TileFrameX / fullWidth].ToVector2();

		if (Main.netMode != NetmodeID.MultiplayerClient)
			Projectile.NewProjectile(new EntitySource_Wiring(i, j), position, Vector2.UnitY * -4f, ModContent.ProjectileType<HydrothermalVentPlume>(), 5, 0f);
		if (!Main.dedServ)
		{
			for (int k = 0; k <= 20; k++)
				Dust.NewDustPerfect(position, ModContent.DustType<Dusts.BoneDust>(), new Vector2(0, 6).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1));
			for (int k = 0; k <= 20; k++)
				Dust.NewDustPerfect(position, ModContent.DustType<Dusts.FireClubDust>(), new Vector2(0, 6).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1));

			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Tile/StoneCrack" + Main.rand.Next(1, 3)) { PitchVariance = .6f }, position);
			SoundEngine.PlaySound(SoundID.Drown with { Pitch = -.5f, PitchVariance = .25f, Volume = 1.5f }, position);

			for(int k = 0; k < 4; k++)
				ParticleHandler.SpawnParticle(new TexturedPulseCircle(position, Color.LightYellow.Additive(), Color.Orange.Additive(), 0.8f, 150 + Main.rand.Next(50), 25 + Main.rand.Next(10), "noise",
					new Vector2(2, 0.6f), EaseFunction.EaseCubicOut, false, 0.5f).WithSkew(0.8f, -MathHelper.PiOver2 + Main.rand.NextFloat(-0.1f, 0.1f)));

			for (int x = 0; x < 5; x++) //Large initial smoke plume
			{
				ParticleHandler.SpawnParticle(new DissipatingSmoke(position + Main.rand.NextVector2Unit() * 25f, -Vector2.UnitY,
					new Color(40, 40, 50), Color.Black, Main.rand.NextFloat(.05f, .3f), 150));
			}

			Main.LocalPlayer.SimpleShakeScreen(2, 3, 90, 16 * 10);
		}
	}

	private bool IsValid(int i, int j)
	{
		var t = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(t);

		if (data is null)
			return false;

		return Framing.GetTileSafely(i, j).TileType == Type && t.TileFrameX % data.CoordinateFullWidth == 0 && t.TileFrameY == 0;
	}

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileSpelunker[Type] = true;
		Main.tileLighted[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Height = 4;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
		TileObjectData.newTile.Origin = new(1, 3);
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Gravel>(), ModContent.TileType<Magmastone>(), TileID.Sand];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 8;
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;
		AddMapEntry(new Color(64, 54, 66), CreateMapEntryName());
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		var t = Framing.GetTileSafely(i, j);
		int fullWidth = TileObjectData.GetTileData(t).CoordinateFullWidth;

		if (t.TileFrameX % fullWidth == 0 && t.TileFrameY == 0) //Client effects
		{
			if (Main.gamePaused)
				return;

			var position = new Vector2(i, j) * 16 + tops[t.TileFrameX / fullWidth].ToVector2();
			if (Main.rand.NextBool(5)) //Passive smoke effects
			{
				var velocity = new Vector2(0, -Main.rand.NextFloat(2f, 2.5f));

				ParticleHandler.SpawnParticle(new DissipatingSmoke(position, velocity,
					new Color(40, 40, 50), Color.SlateGray, Main.rand.NextFloat(.005f, .06f), 150)
				{ squash = true });
			}

			if (Main.rand.NextBool()) //Passive ash effects
			{
				float range = Main.rand.NextFloat();
				var velocity = new Vector2(0, -Main.rand.NextFloat(range * 8f)).RotatedByRandom((1f - range) * 1.5f);

				var dust = Dust.NewDustPerfect(position, DustID.Ash, velocity, Alpha: 180);
				dust.noGravity = true;
			}

			BubbleSoundPlayer.StartSound(new Vector2(i, j) * 16);
		}
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (Framing.GetTileSafely(i, j).TileFrameY == 0)
		{
			float pulse = Main.rand.Next(28, 42) * .001f;
			var col = Color.Orange.ToVector3() / 3.5f;

			(r, g, b) = (col.X + pulse, col.Y + pulse, col.Z + pulse);
		}
	}

	public override void RandomUpdate(int i, int j)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			return; //Redundant

		TileExtensions.GetTopLeft(ref i, ref j); //Allows any tile update (not just the top leftmost) to cause an eruption

		var pt = new Point16(i, j);
		if (IsValid(i, j) && !cooldowns.ContainsKey(pt))
			cooldowns.Add(pt, cooldownMax); //Initialize cooldown counters on the server/singleplayer

		Erupt(i, j);
		if (cooldowns[pt] == 0 && WorldGen.PlayerLOS(i, j))
		{
			Erupt(i, j);
			cooldowns[pt] = cooldownMax;

			if (Main.netMode != NetmodeID.SinglePlayer) //Sync vent eruption with clients
			{
				var packet = SpiritReforgedMod.Instance.GetPacket(MessageType.SendVentEruption, 2);
				packet.Write((short)i);
				packet.Write((short)j);
				packet.Send();
			}
		}
	}
}
