using SpiritReforged.Common.Misc;
using System.IO;
using Terraria.ModLoader.IO;

namespace SpiritReforged.Common.ItemCommon.Pins;

/// <summary> Stores the pins for the world, and gives helper methods to place and remove them. </summary>
public class PinSystem : ModSystem
{
	public TagCompound pins = [];

	/// <summary> Places the pin of <paramref name="heldPinValue"/> type at the given coordinates. </summary>
	/// <param name="heldPinValue"><see cref="PinItem.PinName"/> of the pin.</param>
	/// <param name="position">Position, in tile coordinates, to place the pin at.</param>
	public static void Place(string heldPinValue, Vector2 position)
	{
		ModContent.GetInstance<PinSystem>().SetPin(heldPinValue, position);

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModPacket packet = SpiritReforgedMod.Instance.GetPacket(ReforgedMultiplayer.MessageType.AddPin, 3);
			packet.WriteVector2(position); // WriteVector2 writes x, y - so this counts as 2
			packet.Write(heldPinValue);
			packet.Send();
		}
	}

	/// <summary> Removes the pin of <paramref name="name"/> type. Since there's only one pin per type per world, this needs only the name. </summary>
	/// <param name="name"><see cref="PinItem.PinName"/> of the pin.</param>
	public static void Remove(string name)
	{
		ModContent.GetInstance<PinSystem>().RemovePin(name);

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModPacket packet = SpiritReforgedMod.Instance.GetPacket(ReforgedMultiplayer.MessageType.RemovePin, 1);
			packet.Write(name);
			packet.Send();
		}
	}

	public void SetPin(string name, Vector2 pos) => pins[name] = pos;
	public void RemovePin(string name) => pins.Remove(name);

	public override void SaveWorldData(TagCompound tag) => tag.Add("pins", pins);
	public override void LoadWorldData(TagCompound tag) => pins = tag.Get<TagCompound>("pins");

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(pins.Count);

		foreach (var pair in pins)
		{
			writer.Write(pair.Key);
			writer.WriteVector2(pins.Get<Vector2>(pair.Key));
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		int count = reader.ReadInt32();

		for (int i = 0; i < count; i++)
			pins[reader.ReadString()] = reader.ReadVector2();
	}

	public override void ClearWorld() => pins = [];
}