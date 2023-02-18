using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Destructible : WorldObject
    {
        protected int _iHP = 1;
        public int HP => _iHP;

        protected int _iAltSprite = 0;

        public ToolEnum NeededTool => DataManager.GetEnumByIDKey<ToolEnum>(ID, "Tool", DataType.WorldObject);

        public int NeededToolLevel => DataManager.GetIntByIDKey(ID, "ReqLvl", DataType.WorldObject);

        public Destructible(int id, Dictionary<string, string> stringData, bool loadSprite = true) : base(id)
        {
            LoadDictionaryData(stringData, loadSprite);

            ReloadAlternateSprite(RHRandom.Instance().Next(0, Util.FindParams(stringData["Image"]).Length - 1), stringData["Image"]);

            if (stringData.ContainsKey("ItemID"))
            {
                string[] split = stringData["ItemID"].Split('-');
                int itemID = int.Parse(split[0]);
                int num = 1;

                if (split.Length == 2) { num = int.Parse(split[1]); }
                _kvpDrop = new KeyValuePair<int, int>(itemID, num);
            }

            if (stringData.ContainsKey("Hp")) { _iHP = int.Parse(stringData["Hp"]); }

            if (loadSprite && stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                Sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), Constants.TILE_SIZE, Constants.TILE_SIZE, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //Destructibles move when hit, so reset position
            Sprite.Position = MapPosition;
            if (_iHP <= 0)
            {
                if (!Sprite.ContainsAnimation(AnimationEnum.KO) || Sprite.AnimationFinished(AnimationEnum.KO))
                {
                    MapManager.Maps[Tiles[0].MapName].RemoveWorldObject(this);
                }
            }
        }

        public override bool ProcessLeftClick()
        {
            bool rv = false;

            if (_iHP > 0)
            {
               // PlayerManager.SetTool(PlayerManager.RetrieveTool(NeededTool));
            }

            return rv;
        }

        public void DealDamage(Tool toolUsed)
        {
            if (NeededTool == toolUsed.ToolType)
            {
                if (toolUsed.ToolLevel >= NeededToolLevel)
                {
                    SoundManager.PlayEffectAtLoc(toolUsed.SoundEffect, MapName, CollisionCenter);

                    if (_iHP > 0)
                    {
                        _iHP -= toolUsed.ToolLevel;

                        if (_iHP <= 0)
                        {
                            _bWalkable = true;
                            Sprite.PlayAnimation(AnimationEnum.KO);

                            MapManager.DropItemsOnMap(GetDroppedItems(), CollisionBox.Location);
                            CurrentMap.AlertSpawnPoint(this);
                        }
                        else
                        {
                            //Nudge the Object in the direction of the 'attack'
                            Point nudgePoint = Util.GetPointFromDirection(PlayerManager.PlayerActor.Facing);
                            Sprite.Position = new Point(Sprite.Position.X + nudgePoint.X, Sprite.Position.Y + nudgePoint.Y);
                        }
                    }
                }
                else
                {
                    GUIManager.OpenTextWindow("Weak_Tool");
                }
            }
        }

        private void ReloadAlternateSprite(int altSprite, string imageSprite)
        {
            _iAltSprite = altSprite;
            string[] split = Util.FindParams(imageSprite);

            string[] splitVal = split[_iAltSprite].Split('-');
            _pImagePos = new Point(int.Parse(splitVal[0]), int.Parse(splitVal[1]));

            Sprite.SetAlternate(_pImagePos, AnimationEnum.ObjectIdle);
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData = _iAltSprite.ToString();

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);

            ReloadAlternateSprite(int.Parse(data.stringData), DataManager.GetStringByIDKey(ID, "Image", DataType.WorldObject));
        }
    }
}
