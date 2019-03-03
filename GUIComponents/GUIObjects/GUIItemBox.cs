using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.Game_Managers.ObjectManager;
using System.Collections.Generic;
using System;
using static RiverHollow.WorldObjects.Clothes;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.WorldObjects.Item;
using RiverHollow.Actors;
using static RiverHollow.Game_Managers.GUIObjects.PartyScreen;
using static RiverHollow.Game_Managers.GUIObjects.PartyScreen.NPCDisplayBox;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        public static Rectangle RECT_IMG = new Rectangle(288, 32, 32, 32);
        static Rectangle RECT_SELECT_IMG = new Rectangle(288, 0, 32, 32);
        private Item _item;
        public Item Item => _item;
        protected bool _bHover;
        GUITextWindow _textWindow;
        GUIWindow _reqWindow;
        GUIText _gTextNum;
        GUIImage _gItem;

        GUIImage _gSelected = new GUIImage(RECT_SELECT_IMG, 64, 64, @"Textures\Dialog");

        Item _itToCraft;
        List<GUIObject> _liItemReqs;

        bool _bCrafting;
        bool _bSelected;

        int _iCol;
        public int Col => _iCol;
        int _iRow;
        public int Row => _iRow;

        public GUIItemBox(Item it = null) : base(RECT_IMG, 64, 64, @"Textures\Dialog")
        {
            SetItem(it);
        }

        public GUIItemBox(Rectangle sourceRect, int width, int height, int row, int col, string texture, Item item, bool crafting = false) : base(sourceRect, width, height, texture)
        {
            _bCrafting = crafting;
            SetItem(item);
            
            _iCol = col;
            _iRow = row;
        }

        public GUIItemBox(Rectangle sourceRect, int width, int height, string texture, Item item, bool crafting = false) : this( sourceRect, width, height, 0, 0, texture, item)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_bSelected)
            {
                _gSelected.Draw(spriteBatch);
            }
            if (_gItem != null)
            {
                _gItem.Draw(spriteBatch);
                if (_gTextNum != null) { _gTextNum.Draw(spriteBatch); }

                //MAR?
                if (_bHover)
                {
                    if (_textWindow != null) { _textWindow.Draw(spriteBatch); }
                }
            }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                string text = string.Empty;
                if (Item.IsFood())                  //Text has a {0} parameter so Item.Name fills it out
                {
                    text = string.Format(GameContentManager.GetGameText("FoodConfirm"), Item.Name);
                }
                else if (Item.IsClassItem())        //Class Change handler
                {
                    text = GameContentManager.GetGameText("ClassItemConfirm");
                }
                else if (Item.IsConsumable())       //If the item is a Consumable, construct the selection options from the party
                {
                    int i = 0;
                    text = string.Format("Use {0} on who? [", Item.Name);
                    foreach (CombatAdventurer adv in PlayerManager.GetParty())
                    {
                        text += adv.Name + ":" + i++ + "|";
                    }
                    text += "Cancel:Cancel]";
                }

                //If we have a text string after handling, set the active item and open a new textWindow
                if (!string.IsNullOrEmpty(text))
                {
                    GameManager.gmActiveItem = Item;
                    GUIManager.OpenTextWindow(text);
                }
            }

            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                _bHover = true;
                if (_item != null)
                {
                    _textWindow = new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), _item.GetDescription());

                    if (_bCrafting)
                    {
                        _reqWindow = new GUIWindow(GUIWindow.RedWin, _liItemReqs[0].Width, _liItemReqs[0].Height * _liItemReqs.Count);
                        _reqWindow.Position(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                        _reqWindow.AnchorAndAlignToObject(_textWindow, SideEnum.Bottom, SideEnum.Left);
                        for (int i=0; i < _liItemReqs.Count; i++)
                        {
                            GUIObject r = _liItemReqs[i];
                            if (i == 0) { r.AnchorToInnerSide(_reqWindow, SideEnum.TopLeft); }
                            else
                            {
                                r.AnchorAndAlignToObject(_liItemReqs[i - 1], SideEnum.Bottom, SideEnum.Left);
                            }
                        }
                        _reqWindow.Resize();
                    }
                }
                rv = true;
            }
            else
            {
                _bHover = false;
                _textWindow = null;
                _reqWindow = null;
            }
            return rv;
        }

        public void CloseDescription()
        {
            _bHover = false;
            _textWindow = null;
        }
        public bool DrawDescription(SpriteBatch spriteBatch)
        {
            if (_textWindow != null)
            {
                _textWindow.Draw(spriteBatch);
            }
            if (_reqWindow != null)
            {
                _reqWindow.Draw(spriteBatch);
            }

            return _bHover;
        }

        public void SetItem(Item it)
        {
            _item = it;
            if (_gTextNum != null) { _gTextNum.SetText(""); }
            if(_item != null)
            {
                _gItem = new GUIImage(_item.SourceRectangle, Width, Height, _item.Texture);
                _gItem.Position(Position());

                if (_item.DoesItStack)
                {
                    _gTextNum = new GUIText(_item.Number.ToString(), true, @"Fonts\DisplayFont");
                    _gTextNum.SetColor(Color.White);
                    _gTextNum.AnchorToInnerSide(this, SideEnum.BottomRight, 10);
                }
                if (_bCrafting)
                {
                    _liItemReqs = new List<GUIObject>();
                    _itToCraft = it;
                    foreach (KeyValuePair<int, int> kvp in _itToCraft.GetIngredients())
                    {
                        _liItemReqs.Add(new GUIItemReq(kvp.Key, kvp.Value));
                    }
                }
            }
            else
            {
                _gItem = null;
            }
        }

        public void Select(bool val)
        {
            _bSelected = val;
        }

        public void SetAlpha(float val)
        {
            Alpha = val;
            if (_gSelected != null)
            {
                _gSelected.Alpha = val;
            }
            SetItemAlpha(val);
        }
        public void SetItemAlpha(float val)
        {
            if (_gItem != null)
            {
                _gItem.Alpha = val;
            }
            if(_gTextNum != null)
            {
                _gTextNum.Alpha = val;
            }
        }
        public float GetItemAlpha() { return _gItem.Alpha; }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            if (_gItem != null) { _gItem.Position(value); }
            if(_gTextNum != null) { _gTextNum.AnchorToInnerSide(this, SideEnum.BottomRight, 10); }
            _gSelected.Position(value);
        }

        public class SpecializedBox : GUIItemBox
        {
            ItemEnum _itemType;
            ArmorEnum _armorType;
            ClothesEnum _clothesType;
            WeaponEnum _weaponType;

            #region Getters
            public ItemEnum ItemType => _itemType;
            public ArmorEnum ArmorType => _armorType;
            public ClothesEnum ClothingType => _clothesType;
            public WeaponEnum WeaponType => _weaponType;
            #endregion

            public delegate void OpenItemWindow(SpecializedBox itemBox);
            private OpenItemWindow _delOpenItemWindow;

            public SpecializedBox(ItemEnum itemType, Item item = null, OpenItemWindow del = null) : base()
            {
                SetItem(item);
                _itemType = itemType;
                _delOpenItemWindow = del;
            }
            public SpecializedBox(ArmorEnum armorType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Equipment, item, del)
            {
                _armorType = armorType;
            }
            public SpecializedBox(ClothesEnum clothesType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Clothes, item, del)
            {
                _clothesType = clothesType;
            }
            public SpecializedBox(WeaponEnum weaponType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Equipment, item, del)
            {
                _weaponType = weaponType;
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (Contains(mouse))
                {
                    _bHover = false;
                    _delOpenItemWindow(this);
                }
                return rv;
            }
        }
    }

    public class GUIItemReq : GUIObject
    {
        GUIImage _gImg;
        GUIText _gText;

        public GUIItemReq(int id, int number)
        {
            Item it = ObjectManager.GetItem(id);
            _gImg = new GUIImage(it.SourceRectangle, it.SourceRectangle.Width, it.SourceRectangle.Height, it.Texture);
            _gImg.SetScale(Scale);
            _gText = new GUIText("999"); 
            Width = _gImg.Width + _gText.Width;
            Height = Math.Max(_gImg.Height, _gText.Height);

            _gText.SetText(number.ToString());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gImg.Draw(spriteBatch);
            _gText.Draw(spriteBatch);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gImg.Position(value);
            _gText.AnchorAndAlignToObject(_gImg, SideEnum.Right, SideEnum.Bottom);
        }
    }
}
