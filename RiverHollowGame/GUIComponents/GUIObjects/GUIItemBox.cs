using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
       public Item BoxItem => _guiItem?.ItemObject;
        GUIItem _guiItem;

        public int ColumnID { get; }
        public int RowID { get; }

        public GUIItemBox(Item it = null, ItemBoxDraw e = ItemBoxDraw.OnlyStacks) : base(GUIUtils.ITEM_BOX)
        {
            SetItem(it, e);
        }

        public GUIItemBox(int row, int col, Item item) : base(GUIUtils.ITEM_BOX)
        {
            SetItem(item);
            
            ColumnID = col;
            RowID = row;
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

        public void SetItem(Item it, ItemBoxDraw e = ItemBoxDraw.OnlyStacks)
        {
            if (it != null)
            {
                if (_guiItem == null || (_guiItem != null && _guiItem.ItemObject != it))
                {
                    RemoveControl(_guiItem);
                    _guiItem = new GUIItem(it, e);
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

            public SpecializedBox(ItemEnum itemType, Item item = null, OpenItemWindow del = null)
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
