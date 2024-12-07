using SpiritReforged.Common.PrimitiveRendering.PrimitiveShape;
using SpiritReforged.Common.PrimitiveRendering;
using SpiritReforged.Common.ProjectileCommon;
using System.IO;
using static Terraria.Player;
using static SpiritReforged.Common.Easing.EaseFunction;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Audio;
using SpiritReforged.Common.Visuals.Glowmasks;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Particles;

namespace SpiritReforged.Content.Desert.Scarabeus.Items.Projectiles;

//[AutoloadGlowmask("255,255,255", false)]
public class AdornedBowHeld() : BaseChargeBow(2, 1.5f, 30)
{

}