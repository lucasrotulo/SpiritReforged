using SpiritReforged.Common.TileCommon;
using SpiritReforged.Content.Jungle.Bamboo.Items;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader.IO;

namespace SpiritReforged.Content.Jungle.Bamboo.Tiles;

public class BambooBirdCage : ModTile, IAutoloadTileItem
{
	private int ItemType => Mod.Find<ModItem>(Name + "Item").Type;

	private static readonly int[] BirdTypes = [ItemID.Cardinal, ItemID.BlueJay, ItemID.GoldBird, ItemID.Bird, ItemID.Seagull, ItemID.BlueMacaw, ItemID.GrayCockatiel];

	/// <returns> Whether the multitile at the given position has a tile entity. </returns>
	private static bool HasEntity(int i, int j, out BambooBirdCageEntity entity)
	{
		TileExtensions.GetTopLeft(ref i, ref j);

		if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity value) && value is BambooBirdCageEntity)
		{
			entity = value as BambooBirdCageEntity;
			return true;
		}

		entity = null;
		return false;
	}

	public void SetItemDefaults(ModItem item)
	{
		item.Item.DefaultToPlaceableTile(ModContent.TileType<BambooBirdCage>());
		item.Item.width = 20;
		item.Item.height = 32;
		item.Item.value = 50;
	}

	public void AddItemRecipes(ModItem item) => item.CreateRecipe()
		.AddIngredient(ModContent.ItemType<StrippedBamboo>(), 14)
		.AddTile(TileID.Sawmill).Register();

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Origin = new Point16(1, 2);
		TileObjectData.newTile.Width = 2;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
		ModTileEntity tileEntity = ModContent.GetInstance<BambooBirdCageEntity>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.StyleHorizontal = true;

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		LocalizedText name = CreateMapEntryName();
		AddMapEntry(new Color(100, 100, 60), name);
		DustType = -1;
		RegisterItemDrop(ItemType);
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
		if (tileFrameX >= 18 * 2) //Ceiling mounted
			offsetY = -4;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

	public override IEnumerable<Item> GetItemDrops(int i, int j)
	{
		var drops = base.GetItemDrops(i, j);

		if (HasEntity(i, j, out var entity) && entity.item != null)
			drops = drops.Concat(new[] { entity.item });

		return drops;
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		if (HasEntity(i, j, out var entity))
			entity.Kill(i, j);
	}

	public override bool RightClick(int i, int j)
	{
		static void CageItem(ref Item item, Player player)
		{
			//Consume an item from the player's hand
			item = ItemLoader.TransferWithLimit(Main.mouseItem is null || Main.mouseItem.IsAir ? player.inventory[player.selectedItem] : Main.mouseItem, 1);

			//Interaction effects
			player.releaseUseItem = false;
			player.mouseInterface = true;
			player.PlayDroppedItemAnimation(20);
		}

		if (HasEntity(i, j, out var entity))
		{
			var player = Main.LocalPlayer;

			if (BirdTypes.Contains(player.HeldItem.type) && entity.item is null) //player has a bird in their hand and the cage is empty
			{
				CageItem(ref entity.item, player);

				return true;
			}
			else if (entity.item is not null) //The cage is not empty
			{
				player.QuickSpawnItem(new EntitySource_TileInteraction(player, i, j), entity.item);
				entity.item = null;

				if (BirdTypes.Contains(player.HeldItem.type))
					CageItem(ref entity.item, player);

				return true;
			}
		}

		return false;
	}

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;

		if (HasEntity(i, j, out var entity) && entity.item != null)
			player.cursorItemIconID = entity.item.type;
		else
			player.cursorItemIconID = ItemType;

		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (closer && HasEntity(i, j, out var entity) && entity.item != null && Main.rand.NextBool(100))
			SoundEngine.PlaySound(SoundID.Bird, new Vector2(i * 16, j * 16));
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		var tile = Main.tile[i, j];
		if (tile.TileFrameX % (18 * 2) == 0 && tile.TileFrameY == 0 && HasEntity(i, j, out var entity) && entity.item != null)
		{
			var bird = TextureAssets.Item[entity.item.type];
			Vector2 offset = Lighting.LegacyEngine.Mode > 1 && Main.GameZoomTarget == 1 ? Vector2.Zero : Vector2.One * 12;
			Vector2 position = (new Vector2(i, j) + offset) * 16 - Main.screenPosition;

			position += new Vector2(16, tile.TileFrameX >= 18 * 2 ? 42 : 50);

			if (((int)Main.GlobalTimeWrappedHourly + i + j) % 10 == 0)
				position.Y -= 2;

			spriteBatch.Draw(bird.Value, position, null, Lighting.GetColor(i, j), 0, bird.Frame().Bottom(), 1, SpriteEffects.None, 0);
		}

		return true;
	}
}

public class BambooBirdCageEntity : ModTileEntity
{
	public Item item = null;

	public override bool IsTileValidForEntity(int x, int y)
	{
		Tile tile = Framing.GetTileSafely(x, y);
		return tile.HasTile && tile.TileType == ModContent.TileType<BambooBirdCage>();
	}

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
	{
		var tile = Main.tile[i, j];
		(i, j) = (i - tile.TileFrameX % (18 * 2) / 18, j - tile.TileFrameY / 18);

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
			NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
			return -1;
		}

		return Place(i, j);
	}

	public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);

	public override void NetSend(BinaryWriter writer) => ItemIO.Send(item, writer);

	public override void NetReceive(BinaryReader reader) => item = ItemIO.Receive(reader);

	public override void SaveData(TagCompound tag)
	{
		if (item != null)
			tag[nameof(item)] = item;
	}

	public override void LoadData(TagCompound tag) => item = tag.Get<Item>(nameof(item));
}
