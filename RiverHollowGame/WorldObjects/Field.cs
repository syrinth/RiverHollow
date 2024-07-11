using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    public class Field : Structure
    {
        const int HARVEST_SIZE = 18;
        int _iPlantID = -1;
        int _iTimeElapsed = 0;
        readonly List<Plant> _liPlants;
        MapItem _dropDisplay;

        public Field(int id) : base(id)
        {
            _liPlants = new List<Plant>();

            Unique = true;
            _ePlacement = Enums.ObjectPlacementEnum.Floor;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (var item in _liPlants)
            {
                item.Draw(spriteBatch);   
            }

            if (_dropDisplay != null)
            {
                var targetTile = CurrentMap.GetTileByPixelPosition(_dropDisplay.CollisionBox.Center);
                _dropDisplay.Draw(spriteBatch, targetTile.WorldObject.Sprite.LayerDepth + 1);
            }
        }

        public override void Rollover()
        {
            base.Rollover();
            _iTimeElapsed++;
            _liPlants.ForEach(item => { item.SetTotalDays(_iTimeElapsed); });
        }

        public override bool ProcessRightClick()
        {
            if (_iPlantID == -1)
            {
                GUIManager.OpenMainObject(new HUDFieldDisplay(this));
            }
            else
            {
                var p = _liPlants[0];
                if (p.FinishedGrowing())
                {
                    var dropID = p.GetIntByIDKey("ItemID");
                    var itemDrop = DataManager.GetItem(dropID);

                    for (int i = 0; i < HARVEST_SIZE; i++)
                    {
                        MapManager.DropItemOnMap(itemDrop, _dropDisplay.CollisionBox.Center);
                    }

                    _iPlantID = -1;
                    _liPlants.Clear();
                    _dropDisplay = null;
                }
            }

            return true;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map, ignoreActors);

            PlacePlants(_iPlantID);

            return rv;
        }

        public override void RemoveSelfFromTiles()
        {
            base.RemoveSelfFromTiles();
            _liPlants.Clear();
            _dropDisplay = null;
        }

        public override void SelectObject(bool val, bool selectParent = true)
        {
            base.SelectObject(val);
            _liPlants.ForEach(x => x.SelectObject(val));
            _dropDisplay?.SelectObject(val);
        }

        public void SetSeedID(int id)
        {
            _iPlantID = id;
            _iTimeElapsed = 0;

            PlacePlants(_iPlantID);
        }

        private void PlacePlants(int id)
        {
            if (id > 0)
            {
                var specialData = GetStringParamsByIDKey("Special");
                for (int y = Constants.TILE_SIZE; y <= int.Parse(specialData[1]) * Constants.TILE_SIZE; y += Constants.TILE_SIZE)
                {
                    for (int x = Constants.TILE_SIZE; x <= int.Parse(specialData[0]) * Constants.TILE_SIZE; x += Constants.TILE_SIZE)
                    {
                        var p = new Plant(id);
                        p.SetTotalDays(_iTimeElapsed);
                        p.SnapPositionToGrid(new Point(BaseRectangle.X + x, BaseRectangle.Y + y - (p.Height - Constants.TILE_SIZE)));
                        _liPlants.Add(p);
                    }
                }

                var offset = Util.FindIntArguments(specialData[2]);
                _dropDisplay = new MapItem(new Item(DataManager.GetItem(DataManager.GetIntByIDKey(id, "ItemID", Enums.DataType.WorldObject))))
                {
                    PickupState = Enums.ItemPickupState.None,
                    Position = new Point(BaseRectangle.X + offset[0], BaseRectangle.Y + offset[1])
                };
            }
        }

        public override SaveManager.WorldObjectData SaveData()
        {
            var data = base.SaveData();

            data.stringData = string.Format("{0}-{1}", _iPlantID, _iTimeElapsed);

            return data;
        }

        public override void LoadData(SaveManager.WorldObjectData data)
        {
            base.LoadData(data);
            var strData = Util.FindArguments(data.stringData);
            _iPlantID = int.Parse(strData[0]);
            _iTimeElapsed = int.Parse(strData[1]);
        }
    }
}
