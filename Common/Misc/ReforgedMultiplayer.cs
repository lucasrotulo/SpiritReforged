﻿using SpiritReforged.Common.ItemCommon.Pins;
using SpiritReforged.Common.MapCommon;
using SpiritReforged.Common.PrimitiveRendering;
using SpiritReforged.Common.SimpleEntity;
using SpiritReforged.Common.WorldGeneration;
using System.IO;
using Terraria.DataStructures;

namespace SpiritReforged.Common.Misc;

public static class ReforgedMultiplayer
{
	public enum MessageType : byte
	{
		SendVentPoint,
		SpawnTrail,
		SpawnSimpleEntity,
		KillSimpleEntity,
		AddPin,
		RemovePin,
		AskForPointsOfInterest,
		RemovePointOfInterest,
		RevealMap
	}

	public static void HandlePacket(BinaryReader reader, int whoAmI)
	{
		var id = (MessageType)reader.ReadByte();
		SpiritReforgedMod.Instance.Logger.Debug("[Synchronization] Reading incoming: " + id);

		switch (id)
		{
			case MessageType.SendVentPoint:
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

			case MessageType.SpawnTrail:
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

			case MessageType.SpawnSimpleEntity:
				int entityType = reader.ReadInt32();
				Vector2 position = reader.ReadVector2();

				if (Main.netMode == NetmodeID.Server)
				{
					ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.SpawnSimpleEntity, 2);
					packet.Write(entityType);
					packet.WriteVector2(position);
					packet.Send(ignoreClient: whoAmI); //Relay to other clients
				}

				SimpleEntitySystem.NewEntity(entityType, position);
				break;

			case MessageType.KillSimpleEntity:
				int entityWhoAmI = reader.ReadInt32();

				if (Main.netMode == NetmodeID.Server)
				{
					ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.KillSimpleEntity, 1);
					packet.Write(whoAmI);
					packet.Send(ignoreClient: whoAmI); //Relay to other clients
				}

				SimpleEntitySystem.entities[entityWhoAmI].Kill();
				break;

			case MessageType.AddPin:
				Vector2 cursorPos = reader.ReadVector2();
				string pinName = reader.ReadString();

				if (Main.netMode == NetmodeID.Server)
				{
					ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.AddPin, 3);
					packet.WriteVector2(cursorPos);
					packet.Write(pinName);
					packet.Send();
				}

				ModContent.GetInstance<PinSystem>().SetPin(pinName, cursorPos);
				break;

			case MessageType.RemovePin:
				string removePinName = reader.ReadString();

				if (Main.netMode == NetmodeID.Server)
				{
					ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.RemovePin, 1);
					packet.Write(removePinName);
					packet.Send();
				}

				ModContent.GetInstance<PinSystem>().RemovePin(removePinName);
				break;

			case MessageType.AskForPointsOfInterest:
				if (Main.netMode == NetmodeID.Server)
					PointOfInterestSystem.SendAllPoints(reader);
				else
					PointOfInterestSystem.RecieveAllPoints(reader);
				break;

			case MessageType.RemovePointOfInterest:
				var pointType = (InterestType)reader.ReadByte();
				var pointPos = new Point16(reader.ReadInt16(), reader.ReadInt16());

				if (Main.netMode == NetmodeID.Server)
				{
					ModPacket packet = SpiritReforgedMod.Instance.GetPacket(MessageType.RemovePointOfInterest, 3);
					packet.Write((byte)pointType);
					packet.Write(pointPos.X);
					packet.Write(pointPos.Y);
					packet.Send(-1, whoAmI);
				}

				PointOfInterestSystem.RemovePoint(pointPos, pointType, true);
				break;

			case MessageType.RevealMap:
				var syncType = (RevealMap.MapSyncId)reader.ReadByte();
				RevealMap.RecieveSync(syncType, reader);
				break;
		}
	}
}
