﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;

using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Items.Item;
using static RiverHollow.Items.Clothes;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        public static Rectangle RECT_IMG = new Rectangle(254, 14, 20, 20);
        static Rectangle RECT_SELECT_IMG = new Rectangle(286, 14, 20, 20);
        private Item _item;
        public Item Item => _item;
        GUIWindow _reqWindow;
        GUIText _gTextNum;
        GUIImage _gItem;

        public bool DrawNum = true;

        GUIImage _gSelected = new GUIImage(RECT_SELECT_IMG, ScaleIt(RECT_SELECT_IMG.Width), ScaleIt(RECT_SELECT_IMG.Height), @"Textures\Dialog");

        Item _itToCraft;
        List<GUIObject> _liItemReqs;

        bool _bCrafting;
        bool _bSelected;

        int _iCol;
        public int Col => _iCol;
        int _iRow;
        public int Row => _iRow;

        public GUIItemBox(Item it = null) : base(RECT_IMG, ScaleIt(RECT_IMG.Width), ScaleIt(RECT_IMG.Height), @"Textures\Dialog")
        {
            SetItem(it);

            AddControl(_gSelected);
        }

        public GUIItemBox(int row, int col, string texture, Item item, bool crafting = false) : base(RECT_IMG, ScaleIt(RECT_IMG.Width), ScaleIt(RECT_IMG.Height), texture)
        {
            _bCrafting = crafting;
            SetItem(item);
            
            _iCol = col;
            _iRow = row;

            AddControl(_gSelected);
        }

        public GUIItemBox(string texture, Item item, bool crafting = false) : this(0, 0, texture, item)
        {
            AddControl(_gSelected);
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
                if (DrawNum && _gTextNum != null) { _gTextNum.Draw(spriteBatch); }
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (_item!= null && _item.Number == 0) { SetItem(null); }
            _gTextNum?.SetText(_item == null ? string.Empty : _item.DoesItStack ? _item.Number.ToString() : string.Empty);
            _gTextNum?.AnchorToInnerSide(this, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                string text = string.Empty;
                if (Item.CompareType(ItemEnum.Food))                  //Text has a {0} parameter so Item.Name fills it out
                {
                    text = string.Format(DataManager.GetGameText("FoodConfirm"), Item.Name);
                }
                else if (Item.CompareSpecialType(SpecialItemEnum.Class))        //Class Change handler
                {
                    text = DataManager.GetGameText("ClassItemConfirm");
                }
                else if (Item.CompareType(ItemEnum.MonsterFood))        //Class Change handler
                {
                    if (MapManager.CurrentMap.IsCombatMap)
                    {
                        if (MapManager.CurrentMap.PrimedFood != null) { text = DataManager.GetGameText("MonsterFood_Duplicate"); }
                        else { text = string.Format(DataManager.GetGameText("MonsterFood_Confirm"), Item.Name); }
                    }
                    else { text = DataManager.GetGameText("MonsterFood_False"); }
                }
                else if (Item.CompareType(ItemEnum.Tool))
                {
                    Tool t = (Tool)Item;
                    if (t.ToolType == GameManager.ToolEnum.Harp)
                    {
                        Spirit s = MapManager.CurrentMap.FindSpirit();
                        if (s != null)
                        {
                            HarpManager.NewSong(s);
                        }
                    }
                    else if (t.ToolType == GameManager.ToolEnum.Return)
                    {
                        if (DungeonManager.CurrentDungeon != null)
                        {
                            if (t.HasCharges()) { text = string.Format(DataManager.GetGameText("Rune_of_Return_Use"), Item.Name); }
                            else { text = string.Format(DataManager.GetGameText("Rune_of_Return_Empty"), Item.Name); }
                        }
                        else
                        {
                            text = string.Format(DataManager.GetGameText("Rune_of_Return_No_Dungeon"), Item.Name);
                        }
                    }
                }
                else if (Item.CompareType(ItemEnum.Consumable))       //If the item is a Consumable, construct the selection options from the party
                {
                    int i = 0;
                    text = string.Format(DataManager.GetGameText("ItemConfirm"), Item.Name);
                    foreach (ClassedCombatant adv in PlayerManager.GetParty())
                    {
                        text += adv.Name + ":" + i++ + "|";
                    }
                    text += "Cancel:Cancel]";
                }

                //If we have a text string after handling, set the active item and open a new textWindow
                if (!string.IsNullOrEmpty(text))
                {
                    GameManager.gmActiveItem = Item;
                    GUIManager.OpenTextWindow(text, false);
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                if (_item != null)
                {
                    GUIManager.OpenHoverWindow(new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), _item.GetDescription()), this);
                    if (_bCrafting)
                    {
                        _reqWindow = new GUIWindow(GUIWindow.RedWin, _liItemReqs[0].Width, _liItemReqs[0].Height * _liItemReqs.Count);
                        _reqWindow.Position(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                        //_reqWindow.AnchorAndAlignToObject(_textWindow, SideEnum.Bottom, SideEnum.Left);
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
                _reqWindow = null;
            }
            return rv;
        }

        public void SetItem(Item it)
        {
            _item = it;
            if(_item != null)
            {
                _gItem = new GUIImage(_item.SourceRectangle, ScaledTileSize, ScaledTileSize, _item.Texture);
                _gItem.CenterOnObject(this);
                AddControl(_gItem);

                if (_item.DoesItStack)
                {
                    _gTextNum = new GUIText(_item.Number.ToString(), true, FONT_NUMBER_DISPLAY);
                    _gTextNum.SetColor(Color.White);
                    _gTextNum.AnchorToInnerSide(this, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);
                    AddControl(_gTextNum);
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
            Alpha(val);
            if (_gSelected != null)
            {
                _gSelected.Alpha(val);
            }
            SetItemAlpha(val);
        }
        public void SetItemAlpha(float val)
        {
            if (_gItem != null)
            {
                _gItem.Alpha(val);
            }
            if(_gTextNum != null)
            {
                _gTextNum.Alpha(val);
            }
        }
        public float GetItemAlpha() { return _gItem.Alpha(); }

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
            Item it = DataManager.GetItem(id);
            _gImg = new GUIImage(it.SourceRectangle, it.SourceRectangle.Width, it.SourceRectangle.Height, it.Texture);
            _gImg.SetScale(Scale);
            _gText = new GUIText("999");

            AddControl(_gImg);
            AddControl(_gText);

            Width = _gImg.Width + _gText.Width;
            Height = Math.Max(_gImg.Height, _gText.Height);

            _gText.SetText(number.ToString());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gImg.Draw(spriteBatch);
            _gText.Draw(spriteBatch);
        }
    }
}
