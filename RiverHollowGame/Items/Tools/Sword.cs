using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items.Tools
{
    public class Sword : HitboxTool
    {
        public Sword(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
            DirectionEnum dir = PlayerManager.PlayerActor.Facing;

            if (ToolAnimation.CurrentFrame == 1)
            {
                Hitbox = GetHitbox(dir);

                RHMap map = MapManager.CurrentMap;
                map.TestHitboxOnMobs(this);
            }

            if (FinishTool(dir))
            {
                Hitbox = Rectangle.Empty;
            }
        }
    }
}
