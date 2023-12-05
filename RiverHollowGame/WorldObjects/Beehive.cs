using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.WorldObjects
{
    public class Beehive : Buildable
    {
        int _iPeriod = -1;
        int _iDaysToHoney = -1;
        int _iItemID = -1;

        int _iHoneyToGather = -1;
        bool _bReady = false;
        public Beehive(int id) : base(id)
        {
            Sprite.AddAnimation(AnimationEnum.Action_Finished, _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _pSize);

            _iItemID = GetIntByIDKey("ItemID");
            _iPeriod = GetIntByIDKey("Period");
            _iDaysToHoney = _iPeriod;
        }

        public override bool ProcessRightClick()
        {
            bool rv = false;

            if (_bReady)
            {
                rv = true;
                InventoryManager.AddToInventory(_iHoneyToGather);
                _bReady = false;
                _iDaysToHoney = _iPeriod;
                _iHoneyToGather = -1;
                Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            }

            return rv;
        }

        public override void Rollover()
        {
            if (_iDaysToHoney == 0 && !_bReady)
            {
                RHTile closestPlantTile = Tiles[0];
                foreach (RHTile t in MapManager.Maps[Tiles[0].MapName].GetAllTilesInRange(Tiles[0], 7))
                {
                    if (t.WorldObject != null && t.WorldObject is Plant plantObj)
                    {
                        if (plantObj.HoneyID != -1 && plantObj.FinishedGrowing() && (closestPlantTile == Tiles[0] || Util.GetRHTileDelta(Tiles[0], t) < Util.GetRHTileDelta(Tiles[0], closestPlantTile)))
                        {
                            closestPlantTile = t;
                        }
                    }
                }

                if (closestPlantTile == Tiles[0]) { _iHoneyToGather = _iItemID; }
                else { _iHoneyToGather = ((Plant)closestPlantTile.WorldObject).HoneyID; }

                _bReady = true;
                Sprite.PlayAnimation(AnimationEnum.Action_Finished);
            }
            else
            {
                _iDaysToHoney--;
            }
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData += _bReady + "/";
            data.stringData += _iDaysToHoney + "/";
            data.stringData += (_bReady ? _iHoneyToGather : -1);

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);

            _bReady = bool.Parse(strData[0]);
            _iDaysToHoney = int.Parse(strData[1]);
            _iHoneyToGather = int.Parse(strData[2]);

            if (_iHoneyToGather != -1)
            {
                _bReady = true;
                Sprite.PlayAnimation(AnimationEnum.Action_Finished);
            }
        }
    }
}
