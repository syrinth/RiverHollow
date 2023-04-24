using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        public static Rectangle RECT_IMG = new Rectangle(240, 0, 20, 20);
        public Item BoxItem => _guiItem?.ItemObject;
        GUIItem _guiItem;

        public int Columns { get; }
        public int Rows { get; }

        public GUIItemBox(Rectangle r) : base(RECT_IMG, DataManager.DIALOGUE_TEXTURE) { }

        public GUIItemBox(Item it = null) : base(RECT_IMG, DataManager.DIALOGUE_TEXTURE)
        {
            SetItem(it);
        }

        public GUIItemBox(int row, int col, string texture, Item item) : base(RECT_IMG, ScaleIt(RECT_IMG.Width), ScaleIt(RECT_IMG.Height), texture)
        {
            SetItem(item);
            
            Columns = col;
            Rows = row;
        }

        public GUIItemBox(string texture, Item item, bool crafting = false) : this(0, 0, texture, item)
        {
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (BoxItem != null && BoxItem.Number == 0) {
                SetItem(null);
            }
            _guiItem?.Update(gTime);
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
                    RemoveControl(_guiItem);
                    _guiItem = new GUIItem(it);
                    _guiItem.CenterOnObject(this);
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

        public void DrawNumber(ItemBoxDraw val)
        {
            if (_guiItem != null)
            {
                _guiItem.DrawNumbers = val;
            }
        }

        public bool CompareNumToPlayer()
        {
            return _guiItem.SetCompareNumToPlayer();
        }

        public void SetAlpha(float val)
        {
            Alpha(val);
            SetItemAlpha(val);
        }
        public void SetItemAlpha(float val)
        {
            _guiItem?.Alpha(val);
        }

        public class SpecializedBox : GUIItemBox
        {
            public ItemEnum ItemType { get; }
            public ClothingEnum ClothingType { get; }

            public delegate void OpenItemWindow(SpecializedBox itemBox);
            private OpenItemWindow _delOpenItemWindow;

            public SpecializedBox(ItemEnum itemType, Item item = null, OpenItemWindow del = null) : base(new Rectangle(156, 160, 20, 20))
            {
                SetItem(item);
                ItemType = itemType;
                _delOpenItemWindow = del;
            }

            public SpecializedBox(ClothingEnum clothesType, Item item = null, OpenItemWindow del = null) : this(ItemEnum.Clothing, item, del)
            {
                ClothingType = clothesType;
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
