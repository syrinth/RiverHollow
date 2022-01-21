using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Items;

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
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (BoxItem != null && BoxItem.Number == 0) {
                SetItem(null);
            }
            _guiItem?.Update(gTime);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                BoxItem.ItemBeingUsed();                
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
                RemoveControl(_guiItem);
                _guiItem = null;
            }
        }

        public override void SetColor(Color c)
        {
            _guiItem?.SetColor(c);
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
            public ItemEnum ItemType { get; }
            public ArmorTypeEnum ArmorType { get; }
            public ClothingEnum ClothingType { get; }
            public WeaponEnum WeaponType { get; }

            public delegate void OpenItemWindow(SpecializedBox itemBox);
            private OpenItemWindow _delOpenItemWindow;

            public SpecializedBox(ItemEnum itemType, Item item = null, OpenItemWindow del = null) : base()
            {
                SetItem(item);
                ItemType = itemType;
                _delOpenItemWindow = del;
            }
            public SpecializedBox(ArmorTypeEnum armorType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Equipment, item, del)
            {
                ArmorType = armorType;
            }
            public SpecializedBox(ClothingEnum clothesType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Clothes, item, del)
            {
                ClothingType = clothesType;
            }
            public SpecializedBox(WeaponEnum weaponType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Equipment, item, del)
            {
                WeaponType = weaponType;
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
