using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.MainObjects;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.Utilities;

namespace RiverHollow.WorldObjects
{
    public class Garden : AdjustableObject
    {
        Plant _objPlant;
        AnimatedSprite _sprWatered;
        bool _bWatered;

        public Garden(int id, Dictionary<string, string> stringData) : base(id)
        {
            OutsideOnly = true;
            _eObjectType = ObjectTypeEnum.Garden;

            LoadDictionaryData(stringData, false);

            LoadAdjustableSprite(ref _sprite, DataManager.FILE_WORLDOBJECTS);
            _pImagePos.Y += Constants.TILE_SIZE;

            LoadAdjustableSprite(ref _sprWatered, DataManager.FILE_WORLDOBJECTS);
            _pImagePos.Y -= Constants.TILE_SIZE;

            WaterGardenBed(EnvironmentManager.IsRaining());
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _objPlant?.Update(gTime);
        }

        /// <summary>
        /// Overriding because weneed to set the Depth to 0 for drawing since
        /// this is a floor object and needs to beon the bottom.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.SetColor(_bSelected ? Color.Green : Color.White);
            _sprWatered.SetColor(_bSelected ? Color.Green : Color.White);

            if (_bWatered) { _sprWatered.Draw(spriteBatch, 0); }
            else { _sprite.Draw(spriteBatch, 0); }

            _objPlant?.Draw(spriteBatch);
        }

        /// <summary>
        /// Override to ensure that _sprWatered stays in sync with _sprite
        /// </summary>
        protected override void AdjustmentHelper(RHTile startTile, bool adjustAdjacent = true)
        {
            base.AdjustmentHelper(startTile, adjustAdjacent);
            _sprWatered.PlayAnimation(_sprite.CurrentAnimation.ToString());
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map);

            if (_objPlant != null)
            {
                _objPlant?.SnapPositionToGrid(new Point(MapPosition.X, MapPosition.Y - (_objPlant.Sprite.Height - Constants.TILE_SIZE)));
                _objPlant?.SyncLightPositions();

                if (_objPlant.FinishedGrowing())
                {
                    //Need to do this here for loading because the plant is
                    //set before it's placed
                    CurrentMap.AddLights(_objPlant?.GetLights());
                }
            }
            return rv;
        }

        public override bool ProcessLeftClick() { return HandleGarden(); }

        public override bool CanPickUp() { return _objPlant != null && _objPlant.FinishedGrowing(); }

        /// <summary>
        /// Handles for when the Garden is clicked on to perform
        /// the work that needs to get done
        /// </summary>
        private bool HandleGarden()
        {
            bool rv = false;
            //If the plant is finished growing, harvest it. Otherwise, water it.
            if (_objPlant != null && _objPlant.FinishedGrowing())
            {
                rv = _objPlant.ProcessLeftClick();
            }
            else if (!_bWatered)
            {
                rv = true;
                WaterGardenBed(true);
            }

            return rv;
        }

        /// <summary>
        /// Assigns a Plant to the Garden
        /// </summary>
        /// <param name="obj">The plant to assign to the garden</param>
        public void SetPlant(Plant obj)
        {
            if (obj != null)
            {
                if (_objPlant != null && _objPlant.FinishedGrowing())
                {
                    CurrentMap?.AddLights(_objPlant?.GetLights());
                }
            }
            else if (_objPlant != null)
            {
                CurrentMap.RemoveLights(_objPlant.GetLights());
            }

            _objPlant = obj;
            _objPlant?.SetGarden(this);
            _objPlant?.SnapPositionToGrid(new Point(MapPosition.X, MapPosition.Y - (_objPlant.Sprite.Height - Constants.TILE_SIZE)));
            _objPlant?.SyncLightPositions();
        }
        public Plant GetPlant() { return _objPlant; }

        /// <summary>
        /// Syncs up the _sprWatered and the plant with the new position
        /// </summary>
        /// <param name="position"></param>
        public override void SnapPositionToGrid(Point position)
        {
            base.SnapPositionToGrid(position);
            _sprWatered.Position = MapPosition;
            _objPlant?.SnapPositionToGrid(new Point(MapPosition.X, MapPosition.Y - (_objPlant.Sprite.Height - Constants.TILE_SIZE)));
        }

        public override void Rollover()
        {
            if (_bWatered) { _objPlant?.Rollover(); }

            WaterGardenBed(EnvironmentManager.IsRaining());
        }

        public void WaterGardenBed(bool value)
        {
            _bWatered = value;
            _sprWatered.PlayAnimation(_sprite.CurrentAnimation.ToString());
        }

        public override List<Light> GetLights()
        {
            if (_objPlant != null) { return _objPlant.GetLights(); }
            else { return base.GetLights(); }
        }

        public override void SyncLightPositions()
        {
            if (_objPlant != null) { _objPlant.SyncLightPositions(); }
            else { base.SyncLightPositions(); }
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();

            if (_objPlant != null) {
                WorldObjectData pData = _objPlant.SaveData();
                string str = string.Empty;
                str += pData.ID + "-";
                str += pData.X + "-";
                str += pData.Y + "-";
                str += pData.stringData;
                data.stringData += str;
            }
            else { data.stringData = string.Empty; };

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);

            if (!string.IsNullOrEmpty(data.stringData))
            {
                string[] strData = Util.FindArguments(data.stringData);
                WorldObjectData pData = new WorldObjectData
                {
                    ID = int.Parse(strData[0]),
                    X = int.Parse(strData[1]),
                    Y = int.Parse(strData[2]),
                    stringData = strData[3]
                };

                _objPlant = (Plant)DataManager.CreateWorldObjectByID(pData.ID);
                _objPlant.LoadData(pData);

                SetPlant(_objPlant);
            }
        }
    }
}
