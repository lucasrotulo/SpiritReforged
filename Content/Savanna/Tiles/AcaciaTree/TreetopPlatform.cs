using SpiritReforged.Common.SimpleEntity;
using SpiritReforged.Common.TileCommon.CustomTree;
using SpiritReforged.Common.TileCommon.TileSway;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Tiles.AcaciaTree;

public class TreetopPlatform : SimpleEntity
{
	public Point16? TreePosition { get; private set; }

	public override string TexturePath => "Terraria/Images/Projectile_0";

	public override void Load()
	{
		width = 288;
		height = 8;
	}

	public override void Update()
	{
		TreePosition ??= Center.ToPoint16(); //Initialize
		var tilePos = TreePosition.Value;
		//Use a position convenient to acacia treetops
		Center = tilePos.ToVector2() * 16 + new Vector2(8, -112) + TreeHelper.GetPalmTreeOffset(tilePos.X, tilePos.Y);

		if (!ModContent.GetInstance<AcaciaTree>().IsTreeTop(TreePosition.Value.X, TreePosition.Value.Y, true))
			Kill();
	}

	public void UpdateStanding(Entity entity)
	{
		if (TreePosition is null)
			return;

		var pos = TreePosition.Value;
		float rotation = AcaciaTree.GetSway(pos.X, pos.Y);

		//The difference in rotation from last tick, used to control how much the entity displaces horizontally
		float diff = rotation - AcaciaTree.GetSway(pos.X, pos.Y, ModContent.GetInstance<AcaciaPlatformDetours>().OldTreeWindCounter);
		//Scalar based on the entity's distance from platform center
		float strength = (entity.Center.X - Center.X) / (width * .5f);
		//How much the entity is displaced by the previous factors
		float disp = (entity is NPC) ? 5f : 10f;

		entity.velocity.Y = 0;

		var newPosition = new Vector2(entity.position.X + diff * disp, Hitbox.Top + 10 - entity.height + rotation * strength * disp);
		if (!Collision.SolidCollision(newPosition, entity.width, entity.height))
			entity.position = newPosition;

		if (entity is Player player)
		{
			player.fullRotation = rotation * .07f;
			player.fullRotationOrigin = new Vector2(player.width * .5f, player.height);
		}
	}
}

internal class AcaciaPlatformPlayer : ModPlayer
{
	private bool wasOnPlatform;
	private bool pressedDown;

	public override void PreUpdateMovement()
	{
		bool onPlatform = false;

		foreach (var p in AcaciaTree.Platforms)
		{
			var lowRect = Player.getRect() with { Height = Player.height / 2, Y = (int)Player.position.Y + Player.height / 2 };
			if (lowRect.Intersects(p.Hitbox) && Player.velocity.Y >= 0)
			{
				if (!pressedDown)
				{
					p.UpdateStanding(Player);

					if (Player.controlDown)
						pressedDown = true;
				}
				
				onPlatform = wasOnPlatform = true;
				break; //It would be redundant to check for other platforms when the player is already on one
			}
		}

		if (!onPlatform && wasOnPlatform) //Reset rotation when the player just leaves a platform
		{
			Player.fullRotation = 0;

			pressedDown = false;
			wasOnPlatform = false;
		}
	}
}

internal class AcaciaPlatformDetours : ILoadable
{
	public double OldTreeWindCounter { get; private set; }

	public void Load(Mod mod)
	{
		On_Projectile.AI_007_GrapplingHooks += CheckGrappling;
		On_NPC.UpdateCollision += CheckNPCCollision;
		TileSwaySystem.PreUpdateWind += PreserveWindCounter;
	}

	private void CheckGrappling(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self)
	{
		if (self.type != ProjectileID.SquirrelHook) //Only allow the Squirrel Hook to grapple platforms
		{
			orig(self);
			return;
		}

		foreach (var p in AcaciaTree.Platforms)
		{
			if (self.getRect().Intersects(p.Hitbox) && !Collision.SolidCollision(self.Bottom, self.width, 8))
			{
				var owner = Main.player[self.owner];

				self.ai[0] = 2f;
				self.velocity *= 0;
				self.netUpdate = true;

				owner.grappling[0] = self.whoAmI;
				owner.grapCount++;
				owner.GrappleMovement();

				if (Main.netMode != NetmodeID.SinglePlayer && self.owner == Main.myPlayer)
					NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, self.owner);
			}
		}

		orig(self);
	}

	private void CheckNPCCollision(On_NPC.orig_UpdateCollision orig, NPC self)
	{
		if (!self.noGravity)
		{
			foreach (var p in AcaciaTree.Platforms)
				if (self.getRect().Intersects(p.Hitbox) && self.velocity.Y >= 0)
					p.UpdateStanding(self);
		}

		orig(self);
	}

	private void PreserveWindCounter() => OldTreeWindCounter = TileSwaySystem.Instance.TreeWindCounter;

	public void Unload() { }
}
