using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items.Tools
{
    public class FishingRod : Tool
    {
        public FishingRod(int id) : base(id)
        {
            string[] par = FindParamsByIDKey("ReelAnimation");
            Vector2 start = Util.FindVectorArguments(par[0]);
            int[] sz = Util.FindIntArguments(par[1]);
            Point size = new Point(sz[0], sz[1]);
            int frames = int.Parse(par[2]);
            float frameSpeed = float.Parse(par[3]);

            int xCrawl = 0;
            foreach (DirectionEnum dir in GetEnumArray<DirectionEnum>())
            {
                if (dir == DirectionEnum.None) { continue; }

                _sprite.AddAnimation(VerbEnum.FinishTool, dir, (int)start.X + xCrawl, (int)start.Y, size.X * Constants.TILE_SIZE, size.Y * Constants.TILE_SIZE, frames, frameSpeed, false, true);
                xCrawl += size.X * Constants.TILE_SIZE * frames;
            }
        }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public void ReelItIn()
        {
            _bTriggered = true;
            ToolSprite.PlayAnimation(VerbEnum.FinishTool, PlayerManager.PlayerActor.Facing);
        }
    }
}
