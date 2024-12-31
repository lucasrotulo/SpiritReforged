using System.IO;
using Terraria.ModLoader.IO;

namespace SpiritReforged.Content.Forest.ButterflyStaff;

internal class ButterflySystem : ModSystem
{
	public readonly HashSet<Rectangle> butterflyZones = [];

	public override void SaveWorldData(TagCompound tag)
	{
		if (butterflyZones is null || butterflyZones.Count == 0)
			return;

		int count = 0;

		foreach (var zone in butterflyZones)
		{
			tag[$"butterflyZone{count}"] = zone;
			count++;
		}

		tag["numButterflyZones"] = count;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		int count = tag.GetInt("numButterflyZones");

		for (int i = 0; i < count; i++)
		{
			var zone = tag.Get<Rectangle>($"butterflyZone{count}");

			if (zone != default)
				butterflyZones.Add(zone);
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((short)butterflyZones.Count);

		foreach (var zone in butterflyZones)
		{
			writer.Write((short)zone.Location.X);
			writer.Write((short)zone.Location.Y);
			writer.Write((short)zone.Width);
			writer.Write((short)zone.Height);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		butterflyZones.Clear();
		short count = reader.ReadInt16();

		for (int i = 0; i < count; ++i)
			butterflyZones.Add(new(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()));
	}
}
