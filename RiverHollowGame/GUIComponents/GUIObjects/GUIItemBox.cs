using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using static RiverHollow.Characters.TalkingActor;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Items.Item;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        public static Rectangle RECT_IMG = new Rectangle(254, 14, 20, 20);
        static Rectangle RECT_SELECT_IMG = new Rectangle(286, 14, 20, 20);
        public Item BoxItem => _guiItem?.ItemObject;
        GUIItem _guiItem;

        public bool DrawNum = true;

        GUIImage _gSelected = new GUIImage(RECT_SELECT_IMG, ScaleIt(RECT_SELECT_IMG.Width), ScaleIt(RECT_SELECT_IMG.Height), DataManager.DIALOGUE_TEXTURE);

        bool _bSelected;
        public int Columns { get; }
        public int Rows { get; }

        public GUIItemBox(Item it = null) : base(RECT_IMG, ScaleIt(RECT_IMG.Width), ScaleIt(RECT_IMG.Height), DataManager.DIALOGUE_TEXTURE)
        {
            SetItem(it);

            AddControl(_gSelected);
        }

        public GUIItemBox(int row, int col, string texture, Item item) : base(RECT_IMG, ScaleIt(RECT_IMG.Width), ScaleIt(RECT_IMG.Height), texture)
        {
            SetItem(item);
            
            Columns = col;
            Rows = row;

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

            _guiItem?.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (BoxItem != null && BoxItem.Number == 0) { SetItem(null); }
            _guiItem?.Update(gTime);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                TextEntry entry = null;
                if (BoxItem.CompareType(ItemEnum.Food))                  //Text has a {0} parameter so Item.Name fills it out
                {
                    entry.FormatText(DataManager.GetGameTextEntry("FoodConfirm"), BoxItem.Name);
                }
                else if (BoxItem.CompareSpecialType(SpecialItemEnum.Class))        //Class Change handler
                {
                    entry = DataManager.GetGameTextEntry("ClassItemConfirm");
                }
                else if (BoxItem.CompareType(ItemEnum.MonsterFood))        //Class Change handler
                {
                    if (MapManager.CurrentMap.IsCombatMap)
                    {
                        if (MapManager.CurrentMap.PrimedFood != null) { entry = DataManager.GetGameTextEntry("MonsterFood_Duplicate"); }
                        else { entry.FormatText(DataManager.GetGameTextEntry("MonsterFood_Confirm"), BoxItem.Name); }
                    }
                    else { entry = DataManager.GetGameTextEntry("MonsterFood_False"); }
                }
                else if (BoxItem.CompareType(ItemEnum.Tool))
                {
                    Tool t = (Tool)BoxItem;
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
                            if (t.HasCharges()) { entry.FormatText(DataManager.GetGameTextEntry("Rune_of_Return_Use"), BoxItem.Name); }
                            else { entry.FormatText(DataManager.GetGameTextEntry("Rune_of_Return_Empty"), BoxItem.Name); }
                        }
                        else
                        {
                            entry.FormatText(DataManager.GetGameTextEntry("Rune_of_Return_No_Dungeon"), BoxItem.Name);
                        }
                    }
                }
                else if (BoxItem.CompareType(ItemEnum.Consumable))       //If the item is a Consumable, construct the selection options from the party
                {
                    int i = 0;
                    entry.FormatText(DataManager.GetGameTextEntry("ItemConfirm"), BoxItem.Name);
                    entry.AppendParty();
                }

                //If we have a text string after handling, set the active item and open a new textWindow
                if (entry != null)
                {
                    GameManager.CurrentItem = BoxItem;
                    GUIManager.OpenTextWindow(entry, false);
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                _guiItem?.ProcessHover(mouse);
                rv = true;
            }
            return rv;
        }

        public void SetItem(Item it)
        {
            if (it != null)
            {
                if (_guiItem == null || (_guiItem != null && _guiItem.ItemObject != it))
                {
                    _guiItem = new GUIItem(it);
                    _guiItem.CenterOnObject(this);
                    if (GameManager.CurrentInventoryDisplay == DisplayTypeEnum.Gift && !it.Giftable()) { _guiItem.Alpha(0.5f); }
                    AddControl(_guiItem);
                }
            }
            else
            {
                _guiItem = null;
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
            _guiItem?.Alpha(val);
        }
        public float GetItemAlpha() { return _guiItem.Alpha(); }

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
}
