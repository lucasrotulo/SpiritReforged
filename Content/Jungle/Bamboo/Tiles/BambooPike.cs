using SpiritReforged.Content.Jungle.Bamboo.Buffs;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Jungle.Bamboo.Tiles;

public class BambooPike : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileNoFail[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
		TileObjectData.newTile.AnchorAlternateTiles = [Type];
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		RegisterItemDrop(ModContent.ItemType<BambooPikeItem>());
		AddMapEntry(new Color(80, 140, 35));
		DustType = DustID.JunglePlants;
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Framing.GetTileSafely(i, j);

		bool hasTileAbove = Framing.GetTileSafely(i, j - 1).TileType == Type;
		bool hasTileBelow = Framing.GetTileSafely(i, j + 1).TileType == Type;

		if (!Framing.GetTileSafely(i, j + 1).HasTile) //Has any tile below
		{
			WorldGen.KillTile(i, j, false, false, false);
			return false;
		}

		if (hasTileAbove) //Pick the appropriate frame depending on tile stack
			tile.TileFrameY = (short)(18 * (hasTileBelow ? 1 : 2));
		else
			tile.TileFrameY = 0;

		if (hasTileBelow) //Inherit the same horizontal tile frame as the rest of the stack
			tile.TileFrameX = Framing.GetTileSafely(i, j + 1).TileFrameX;

		return false;
	}

	public static void Strike(Entity entity)
	{
		float damage = MathHelper.Clamp(entity.velocity.Y, 1, 10) * 10f;
		Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath12, entity.Center);

		for (int d = 0; d < 20; d++)
			Dust.NewDustPerfect(entity.Bottom + Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f), DustID.Blood);

		if (entity is NPC npc)
		{
			npc.SimpleStrikeNPC((int)damage, 1, false, 0);
			npc.AddBuff(ModContent.BuffType<Impaled>(), 500);
		}
		else if (entity is Player player)
		{
			player.Hurt(PlayerDeathReason.ByOther(3), (int)damage, 0);
			player.AddBuff(ModContent.BuffType<Impaled>(), 500);
			player.velocity = new Vector2(0, 1);
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Framing.GetTileSafely(i, j);
		Texture2D texture = TextureAssets.Tile[Type].Value;
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

		Vector2 offset = Lighting.LegacyEngine.Mode > 1 && Main.GameZoomTarget == 1 ? Vector2.Zero : Vector2.One * 12;
		Vector2 drawPos = (new Vector2(i, j) + offset) * 16 - Main.screenPosition + new Vector2(tile.TileFrameX / 18 * 2, 2);

		spriteBatch.Draw(texture, drawPos, source, Lighting.GetColor(i, j), 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
		return false;
	}
}

public class BambooPikeItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<BambooPike>());
		Item.width = Item.height = 36;
		Item.value = Item.sellPrice(copper: 5);
	}

	public override bool CanUseItem(Player player)
	{
		//Allow pikes to be placed when used on any tile in the stack
		var tilePos = new Point((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16));

		if (Framing.GetTileSafely(tilePos).TileType != Item.createTile) //Avoid this custom logic if possible
			return base.CanUseItem(player);

		while (Framing.GetTileSafely(tilePos).TileType == Item.createTile)
			tilePos.Y--;

		WorldGen.PlaceTile(tilePos.X, tilePos.Y, Item.createTile);
		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, tilePos.X, tilePos.Y, 1);

		if (Item.stack == 1)
			Item.TurnToAir();
		else
			Item.stack--;

		return base.CanUseItem(player);
	}

	public override void AddRecipes()
	{
		Recipe recipe = CreateRecipe();
		recipe.AddIngredient(ItemID.BambooBlock, 2);
		recipe.AddTile(TileID.WorkBenches);
		recipe.Register();
	}
}