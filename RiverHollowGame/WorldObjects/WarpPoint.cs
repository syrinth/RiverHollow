﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.WorldObjects
{
    public class WarpPoint : WorldObject
    {
        public bool Active { get; private set; } = false;
        private string _sDungeonName;

        public WarpPoint(int id) : base(id)
        {
            Sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _pSize);
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map);
            _sDungeonName = map.DungeonName;
            DungeonManager.AddWarpPoint(this, _sDungeonName);

            return rv;
        }

        public override bool ProcessRightClick()
        {
            bool rv = false;
            if (!Active)
            {
                rv = true;
                Active = true;
                Sprite.PlayAnimation(AnimationEnum.Action1);
            }
            else
            {
                rv = true;
                GUIManager.OpenMainObject(new WarpPointWindow(this));
            }

            return rv;
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData += Active;

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            Active = bool.Parse(data.stringData);

            if (Active) { Sprite.PlayAnimation(AnimationEnum.Action1); }
        }
    }
}
