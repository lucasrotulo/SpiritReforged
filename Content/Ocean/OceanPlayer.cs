﻿namespace SpiritReforged.Content.Ocean;

public class OceanPlayer : ModPlayer
{
    /// <summary>
    /// Helper method that checks how far underwater the player is, continuously. If a tile above the player is not watered enough but is solid, it will still count as submerged.
    /// </summary>
    /// <param name="tileDepth">Depth in tiles for the player to be under.</param>
    public bool Submerged(int tileDepth, out int realDepth, bool countRealDepth = false)
    {
        realDepth = 0;

        if (!Collision.WetCollision(Player.position, Player.width, 8))
            return false;

        Point tPos = Player.Center.ToTileCoordinates();
        for (int i = 0; i < tileDepth; ++i)
        {
            realDepth = i;

            if (!WorldGen.InWorld(tPos.X, tPos.Y - i))
                return true;

            if (!countRealDepth && WorldGen.SolidTile(tPos.X, tPos.Y - i))
                return true; //Fully submerged to the point where the player should not be able to breathe
            else if (countRealDepth && WorldGen.SolidTile(tPos.X, tPos.Y - i))
                continue;

            if (Framing.GetTileSafely(tPos.X, tPos.Y - i).LiquidAmount < 255)
                return false;
        }

        return true;
    }

    public bool Submerged(int tileDepth) => Submerged(tileDepth, out int _);
}