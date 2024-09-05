﻿using SpiritReforged.Common.PrimitiveRendering;
using System.IO;
using Terraria.DataStructures;

namespace SpiritReforged.Common.Misc;

public static class ReforgedMultiplayer
{
	public enum MessageType : byte
	{
		SendVentPoint,
		SpawnTrail,
        SendPlatform
    }

	public static void HandlePacket(BinaryReader reader, int whoAmI)
	{
		var id = (MessageType)reader.ReadByte();

		switch (id)
		{
			case MessageType.SendVentPoint:
				{
					int i = reader.ReadInt32();
					int j = reader.ReadInt32();

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.SendVentPoint, 2);
						packet.Write(i);
						packet.Write(j);
						packet.Send(ignoreClient: whoAmI); //Relay to other clients
					}

					Content.Ocean.Tiles.VentSystem.VentPoints.Add(new Point16(i, j));
					break;
				}
			case MessageType.SpawnTrail:
				{
					int proj = reader.ReadInt32();

					if (Main.netMode == NetmodeID.Server)
					{
						//If received by the server, send to all clients instead
						ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.SpawnTrail, 1);
						packet.Write(proj);
						packet.Send();
						break;
					}

					if (Main.projectile[proj].ModProjectile is IManualTrailProjectile trailProj)
						trailProj.DoTrailCreation(AssetLoader.VertexTrailManager);
					break;
				}
			case MessageType.SendPlatform:
				{
					//Should only be received by the server
					Vector2 center = reader.ReadVector2();
					int width = reader.ReadInt32();
					bool remove = reader.ReadBoolean();

					var platform = new Content.Savanna.Tiles.AcaciaTree.CustomPlatform(center, width);

					if (remove)
						Content.Savanna.Tiles.AcaciaTree.AcaciaTreeSystem.Instance.platforms.Remove(platform);
					else
						Content.Savanna.Tiles.AcaciaTree.AcaciaTreeSystem.AddPlatform(platform);

					break;
				}
		}
	}
}
