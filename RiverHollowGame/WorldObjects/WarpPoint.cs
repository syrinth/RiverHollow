using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.Map_Handling;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;

namespace RiverHollow.WorldObjects
{
    public class WarpPoint : WorldObject
    {
        public bool Active { get; private set; } = false;
        private string _sDungeonName;

        public WarpPoint(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData);
            _sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _uSize);
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            bool rv = base.PlaceOnMap(pos, map);
            _sDungeonName = map.DungeonName;
            DungeonManager.AddWarpPoint(this, _sDungeonName);

            return rv;
        }

        public override void ProcessRightClick()
        {
            if (!Active)
            {
                Active = true;
                _sprite.PlayAnimation(AnimationEnum.Action1);
            }
            else
            {
                GUIManager.OpenMainObject(new WarpPointWindow(this));
            }
        }

        public WarpPointData SaveData()
        {
            WarpPointData w = new WarpPointData
            {
                ID = this.ID,
                x = (int)this.CollisionBox.X,
                y = (int)this.CollisionBox.Y,
                active = this.Active
            };

            return w;
        }
        public void LoadData(WarpPointData warpPt)
        {
            _iID = warpPt.ID;
            SnapPositionToGrid(new Vector2(warpPt.x, warpPt.y));
            Active = warpPt.active;

            if (Active) { _sprite.PlayAnimation(AnimationEnum.Action1); }
        }
    }
}
