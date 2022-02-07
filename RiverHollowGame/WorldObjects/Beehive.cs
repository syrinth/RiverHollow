using RiverHollow.Game_Managers;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using Microsoft.Xna.Framework;

namespace RiverHollow.WorldObjects
{
    public class Beehive : Buildable
    {
        int _iPeriod = -1;
        int _iDaysToHoney = -1;
        int _iItemID = -1;

        int _iHoneyToGather = -1;
        bool _bReady = false;
        public Beehive(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _sprite.AddAnimation(AnimationEnum.Action_Finished, _pImagePos.X + TILE_SIZE, _pImagePos.Y, _uSize);

            Util.AssignValue(ref _iItemID, "ItemID", stringData);
            Util.AssignValue(ref _iPeriod, "Period", stringData);
            _iDaysToHoney = _iPeriod;
        }

        public override void ProcessRightClick()
        {
            if (_bReady)
            {
                InventoryManager.AddToInventory(_iHoneyToGather);
                _bReady = false;
                _iDaysToHoney = _iPeriod;
                _iHoneyToGather = -1;
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            }
        }

        public override void Rollover()
        {
            if (_iDaysToHoney == 0 && !_bReady)
            {
                RHTile closestFlowerTile = Tiles[0];
                foreach (RHTile t in MapManager.Maps[Tiles[0].MapName].GetAllTilesInRange(Tiles[0], 7))
                {
                    if (t.WorldObject != null && t.WorldObject.CompareType(ObjectTypeEnum.Garden))
                    {
                        Plant p = ((Garden)t.WorldObject).GetPlant();
                        if (p != null && p.HoneyID != -1 && p.FinishedGrowing() && (closestFlowerTile == Tiles[0] || Util.GetRHTileDelta(Tiles[0], t) < Util.GetRHTileDelta(Tiles[0], closestFlowerTile)))
                        {
                            closestFlowerTile = t;
                        }
                    }
                }

                if (closestFlowerTile == Tiles[0]) { _iHoneyToGather = _iItemID; }
                else { _iHoneyToGather = ((Garden)closestFlowerTile.WorldObject).GetPlant().HoneyID; }

                _bReady = true;
                _sprite.PlayAnimation(AnimationEnum.Action_Finished);
            }
            else
            {
                _iDaysToHoney--;
            }
        }

        public BeehiveData SaveData()
        {
            BeehiveData data = new BeehiveData
            {
                ID = this.ID,
                x = (int)this.CollisionBox.X,
                y = (int)this.CollisionBox.Y,
                timeLeft = this._iDaysToHoney,
                ready = this._bReady,
                honeyType = _bReady ? _iHoneyToGather : -1
            };

            return data;
        }
        public void LoadData(BeehiveData data)
        {
            _iID = data.ID;
            SnapPositionToGrid(new Vector2(data.x, data.y));
            _bReady = data.ready;
            _iDaysToHoney = data.timeLeft;
            _iHoneyToGather = data.honeyType;

            if (_iHoneyToGather != -1)
            {
                _bReady = true;
                _sprite.PlayAnimation(AnimationEnum.Action_Finished);
            }
        }
    }
}
