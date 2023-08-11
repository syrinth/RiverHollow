﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items.Tools
{
    internal class Scythe : HitboxTool
    {
        public Scythe(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
            DirectionEnum dir = PlayerManager.PlayerActor.Facing;

            if (ReadyToHit() && !_bTriggered)
            {
                _bTriggered = true;
                Hitbox = GetHitbox(dir);

                RHMap map = MapManager.CurrentMap;
                var tiles = map.GetTilesFromRectangleIncludeEdgePoints(Hitbox);
                tiles.AddRange(map.GetTilesFromRectangleIncludeEdgePoints(PlayerManager.PlayerActor.CollisionBox));

                bool hit = false;
                var areaTiles = tiles.FindAll(x => x != null && x.CollisionBox.Intersects(Hitbox));
                foreach (var t in areaTiles)
                {
                    if (t.DamageObject(this))
                    {
                        hit = true;
                    }
                }

                if (hit)
                {
                    PlayerManager.ToolLoseEnergy();
                }
            }

            if (CheckFinishTool(dir))
            {
                Hitbox = Rectangle.Empty;
            }
        }
    }
}
