using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Destructible : WorldObject
    {
        private AnimatedSprite _spriteKO;
        public int HP {get; protected set;}

        protected int _iAltSprite = -1;

        public ToolEnum NeededTool => GetEnumByIDKey<ToolEnum>("Tool");

        public int NeededToolLevel => GetIntByIDKey("ReqLvl");

        public Destructible(int id) : base(id)
        {
            HP = GetIntByIDKey("Hp", 1);
        }

        protected override void LoadSprite()
        {
            var value = 0;
            var imageStr = GetStringParamsByIDKey("Image");

            //If -1, it's either unset or no alts present
            //Else, an alt has been saved
            if (_iAltSprite == -1)
            {
                //If only length of one, no alts present
                if (imageStr.Length > 1)
                {
                    _iAltSprite = RHRandom.Instance().Next(0, int.Parse(imageStr[1]));
                    value = _iAltSprite;
                }
            }
            else { value = _iAltSprite; }
            
            string[] split = GetStringParamsByIDKey("Image");

            _pImagePos = Util.MultiplyPoint(Util.ParsePoint(split[0]) + new Point(value, 0), Constants.TILE_SIZE);
            base.LoadSprite();

            if (GetBoolByIDKey("DestructionAnim"))
            {
                string[] splitString = GetStringArgsByIDKey("DestructionAnim");
                _spriteKO = new AnimatedSprite(DataManager.FILE_MISC_SPRITES);
                _spriteKO.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]) * Constants.TILE_SIZE, int.Parse(splitString[1]) * Constants.TILE_SIZE, Constants.TILE_SIZE, Constants.TILE_SIZE, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HP <= 0)
            {
                _spriteKO?.Draw(spriteBatch);
            }
            else
            {
                base.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _spriteKO?.Update(gTime);

            //Destructibles move when hit, so reset position
            SetSpritePos(MapPosition);
            if (HP <= 0)
            {
                if (_spriteKO == null || _spriteKO.AnimationFinished(AnimationEnum.KO))
                {
                    MapManager.Maps[Tiles[0].MapName].RemoveWorldObject(this);
                }
            }
        }

        public override bool ProcessLeftClick()
        {
            bool rv = false;

            if (Constants.AUTO_TOOL)
            {
                Tool playerTool = PlayerManager.RetrieveTool(NeededTool);
                if (HP > 0 && playerTool != null && playerTool.IsAutomatic)
                {
                    rv = true;
                    PlayerManager.FaceCursor();
                    PlayerManager.SetTool(playerTool);
                }
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

                    if (HP > 0)
                    {
                        HP -= toolUsed.ToolLevel;

                        if (HP <= 0)
                        {
                            _bWalkable = true;
                            if (_spriteKO != null)
                            {
                                _spriteKO.Position = Sprite.Position;
                                _spriteKO.PlayAnimation(AnimationEnum.KO);
                            }

                            MapManager.DropItemsOnMap(GetDroppedItems(), CollisionBox.Location);
                        }
                        else
                        {
                            NudgeObject(false);
                        }
                    }
                }
                else
                {
                    GUIManager.OpenTextWindow("Weak_Tool");
                }
            }
        }
        protected void NudgeObject(bool playEffect)
        {
            //Nudge the Object in the direction of the 'attack'
            Point nudgePoint = Util.GetPointFromDirection(Util.GetOppositeDirection(Util.GetDirectionOf(CollisionCenter, PlayerManager.PlayerActor.Center)));
            Sprite.Position = new Point(Sprite.Position.X + nudgePoint.X, Sprite.Position.Y + nudgePoint.Y);

            if (playEffect)
            {
                SoundManager.PlayEffectAtLoc(SoundEffectEnum.Scythe, CurrentMap.Name, CollisionCenter, this);
            }
        }

        protected virtual string Loot()
        {
            return GetStringByIDKey("ItemID");
        }
        protected List<Item> GetDroppedItems()
        {
            var items = Loot();
            string[] strParams = Util.FindParams(items);
            var dropList = new List<Tuple<int, int>>();
            for (int i = 0; i < strParams.Length; i++)
            {
                int number = 0;
                string[] split = Util.FindArguments(strParams[i]);

                if (split.Length == 1) { number = 1; }
                else if (split.Length == 2) { number = int.Parse(split[1]); }
                else if (split.Length == 3) { number = RHRandom.Instance().Next(int.Parse(split[1]), int.Parse(split[2])); }

                var tup = new Tuple<int, int>(int.Parse(split[0]), number);
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

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData = _iAltSprite.ToString();

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            _iAltSprite = int.Parse(Util.FindParams(data.stringData)[0]);

            LoadSprite();
        }
    }
}
