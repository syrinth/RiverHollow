using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Destructible : WorldObject
    {
        protected int _iHP = 1;
        public int HP => _iHP;

        protected int _iAltSprite = 0;

        public ToolEnum NeededTool => GetEnumByIDKey<ToolEnum>("Tool");

        public int NeededToolLevel => GetIntByIDKey("ReqLvl");

        public Destructible(int id, Dictionary<string, string> stringData, bool loadSprite = true) : base(id)
        {
            LoadDictionaryData(stringData, loadSprite);

            ReloadAlternateSprite(RHRandom.Instance().Next(0, Util.FindParams(stringData["Image"]).Length - 1), stringData["Image"]);

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
                    SoundManager.PlayEffectAtLoc(toolUsed.GetEnumByIDKey<SoundEffectEnum>("SoundEffect"), MapName, CollisionCenter);

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

        private List<Item> GetDroppedItems()
        {
            string items = GetStringByIDKey("ItemID");
            string[] strParams = Util.FindParams(items);
            var dropList = new List<Tuple<int, int>>();
            for (int i = 0; i < strParams.Length; i++)
            {
                string[] split = Util.FindArguments(strParams[i]);
                var tup = new Tuple<int, int>(int.Parse(split[0]), split.Length > 1 ? int.Parse(split[1]) : 1);
                dropList.Add(tup);
            }

            items = GetStringByIDKey("BonusItemID");
            if (!string.IsNullOrEmpty(items))
            {
                var bonusItemDictionary = new Dictionary<RarityEnum, List<Tuple<int, int>>>();
                Util.AddToListDictionary(ref bonusItemDictionary, RarityEnum.C, new Tuple<int, int>(-1, 0));

                string[] bonusItems = Util.FindParams(items);
                for (int i = 0; i < bonusItems.Length; i++)
                {
                    string[] arguments = Util.FindArguments(bonusItems[i]);

                    RarityEnum rarity = arguments.Length > 2 ? Util.ParseEnum<RarityEnum>(arguments[2]) : RarityEnum.C;
                    var tuple = new Tuple<int, int>(int.Parse(arguments[0]), int.Parse(arguments[1]));

                    Util.AddToListDictionary(ref bonusItemDictionary, rarity, tuple);
                }

                var chosenTuple = Util.RollOnRarityTable(bonusItemDictionary);
                if (chosenTuple.Item1 > -1)
                {
                    dropList.Add(chosenTuple);
                }
            }

            List<Item> itemList = new List<Item>();
            dropList.ForEach(x => { 
                for(int i = 0; i < x.Item2; i++)
                {
                    itemList.Add(DataManager.GetItem(x.Item1));
                }
            });

            return itemList;
        }

        private void ReloadAlternateSprite(int altSprite, string imageSprite)
        {
            _iAltSprite = altSprite;
            string[] split = Util.FindParams(imageSprite);

            _pImagePos = Util.ParsePoint(split[0]) + new Point(altSprite * Constants.TILE_SIZE, 0);
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

            ReloadAlternateSprite(int.Parse(data.stringData), GetStringByIDKey("Image"));
        }
    }
}
