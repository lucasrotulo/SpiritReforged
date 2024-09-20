using SpiritReforged.Common.SimpleEntity;
using SpiritReforged.Common.TileCommon.TileSway;
using Terraria.Audio;
using static SpiritReforged.Common.Misc.ReforgedMultiplayer;

namespace SpiritReforged.Content.Ocean.Items.Buoys;

public class Buoy : ModItem
{
	private static bool WaterBelow()
	{
		for (int i = 0; i < 2; i++)
		{
			var tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY + i);
			if (i == 0 && tile.LiquidAmount >= 255)
				return false; //Already completely submerged

			if (tile.LiquidAmount > 20 && tile.LiquidType == LiquidID.Water)
				return true;
		}

		return false;
	}

	public override void SetDefaults()
	{
		Item.width = Item.height = 14;
		Item.useAnimation = 15;
		Item.useTime = 10;
		Item.maxStack = Item.CommonMaxStack;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTurn = true;
		Item.autoReuse = true;
		Item.consumable = true;
		Item.value = Item.sellPrice(silver: 1);
	}

	public override void HoldItem(Player player)
	{
		if (CanUseItem(player))
		{
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = Type;
		}
	}

	public override bool CanUseItem(Player player) => WaterBelow() && player.IsTargetTileInItemRange(Item);

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer && player.ItemAnimationJustStarted)
		{
			int type = SimpleEntitySystem.types[typeof(BuoyEntity)];
			var position = Main.MouseWorld;

			SimpleEntitySystem.NewEntity(type, position);

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.SpawnSimpleEntity, 2);
				packet.Write(type);
				packet.WriteVector2(position);
				packet.Send();
			}

			return true;
		}

		return null;
	}

	public override void AddRecipes() => CreateRecipe()
			.AddRecipeGroup(RecipeGroupID.IronBar, 3)
			.AddIngredient(ItemID.Wire, 5)
			.AddIngredient(ItemID.Glass, 5)
			.AddTile(TileID.Anvils)
			.Register();
}

public class BuoyEntity : SimpleEntity
{
	private static Asset<Texture2D> GlowTexture;

	public virtual Texture2D Glowmask => GlowTexture.Value;

	public override void Load()
	{
		if (!Main.dedServ)
			GlowTexture = ModContent.Request<Texture2D>(TexturePath + "_Glow");

		saveMe = true;
		width = 16;
		height = 18;
	}

	public override void Update()
	{
		if (Collision.WetCollision(position, width, height + 8))
			velocity.Y -= .05f;
		else if (!Collision.WetCollision(position, width, height + 12))
			velocity.Y += .1f;
		else
			velocity.Y *= .75f;

		position += velocity;

		Lighting.AddLight(Top, .3f, .1f, .1f);

		#region pickaxe check
		var player = Main.LocalPlayer;
		var heldItem = player.HeldItem;

		if (heldItem != null && Hitbox.Contains(Main.MouseWorld.ToPoint()) && player.IsTargetTileInItemRange(heldItem) && player.HeldItem.pick > 0 && player.ItemAnimationJustStarted)
		{
			Kill();

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.KillSimpleEntity, 1);
				packet.Write(whoAmI);
				packet.Send();
			}
		}
		#endregion
	}

	public override void OnKill()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient)
			Item.NewItem(GetSource_Death(), Hitbox, ModContent.ItemType<Buoy>());

		SoundEngine.PlaySound(SoundID.Dig, Center);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float Sin(float numerator) => (float)Math.Sin((Main.timeForVisualEffects + Center.X) / numerator);

		var texture = Texture.Value;
		var origin = new Vector2(texture.Width / 2, texture.Height);
		var drawPosition = position - Main.screenPosition + new Vector2(0, Sin(30f)) + origin;
		var color = Lighting.GetColor((int)(Center.X / 16), (int)(Center.Y / 16));

		float rotation = Main.instance.TilesRenderer.GetWindCycle((int)(position.X / 16), (int)(position.Y / 16), TileSwaySystem.Instance.SunflowerWindCounter);
		rotation += TileSwayHelper.GetHighestWindGridPushComplex((int)(position.X / 16), (int)(position.Y / 16), 2, 3, 120, 1f, 5, true);

		spriteBatch.Draw(texture, drawPosition, null, color, rotation * .1f, origin, 1, SpriteEffects.None, 0f);

		var glowColor = Color.White * (1f - Sin(10f) * .3f);
		spriteBatch.Draw(Glowmask, drawPosition, null, glowColor, rotation * .1f, origin, 1, SpriteEffects.None, 0f);
	}
}