using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace SpiritReforged.Content.Snow.FrozenLake;

public class StoredTileEntity : ModTileEntity
{
	private struct StoredData(bool isNPC, int type)
	{
		public bool isNPC = isNPC; //Whether this is an NPC or item
		public int type = type;
	}
	private StoredData stored;

	/// <param name="type"> The item or NPC type (depending on how isNPCType is set). </param>
	/// <param name="isNPCType"> Whether type corresponds to an NPC or item type. </param>
	public static void CreateNew(int x, int y, int type, bool isNPCType = false)
	{
		PlaceEntityNet(x, y, ModContent.TileEntityType<StoredTileEntity>());
		(ByPosition[new Point16(x, y)] as StoredTileEntity).stored = new StoredData(isNPCType, type);
	}

	public void Draw()
	{
		var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		var position = Position.ToVector2() * 16 - Main.screenPosition + zero + new Vector2(8);
		var color = Color.White * .2f;

		if (stored.isNPC)
		{
			Main.instance.LoadNPC(stored.type);

			var texture = TextureAssets.Npc[stored.type].Value;
			var source = texture.Frame(1, Main.npcFrameCount[stored.type]);

			Main.EntitySpriteDraw(texture, position, source, color, 0, source.Size() / 2, 1, SpriteEffects.None);
		}
		else
		{
			Main.instance.LoadItem(stored.type);

			var texture = TextureAssets.Item[stored.type].Value;
			Main.EntitySpriteDraw(texture, position, null, color, 0, texture.Size() / 2, 1, SpriteEffects.None);
		}
	}

	public override void Update()
	{
		if (!IsTileValidForEntity(Position.X, Position.Y))
			Kill(Position.X, Position.Y);
	}

	public override void OnKill()
	{
		var source = new EntitySource_TileBreak(Position.X, Position.Y);

		if (stored.isNPC)
			NPC.NewNPC(source, Position.X * 16, Position.Y * 16, stored.type);
		else
			Item.NewItem(source, Position.ToVector2() * 16, stored.type);
	}

	public override bool IsTileValidForEntity(int x, int y)
	{
		var tile = Framing.GetTileSafely(x, y);
		return tile.HasTile && tile.TileType == ModContent.TileType<ClearIce>();
	}

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendTileSquare(Main.myPlayer, i, j);
			NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
			return -1;
		}

		return Place(i, j);
	}

	public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(stored.isNPC);
		writer.Write(stored.type);
	}

	public override void NetReceive(BinaryReader reader)
	{
		stored.isNPC = reader.ReadBoolean();
		stored.type = reader.ReadInt32();
	}

	public override void SaveData(TagCompound tag)
	{
		tag[nameof(stored.isNPC)] = stored.isNPC;
		tag[nameof(stored.type)] = stored.type;
	}

	public override void LoadData(TagCompound tag)
	{
		stored.isNPC = tag.GetBool(nameof(stored.isNPC));
		stored.type = tag.GetInt(nameof(stored.type));
	}
}
