using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIObject
    {
        public Item BoxItem => _gItem?.ItemObject;
        private readonly GUIImage _gBackground;
        protected GUIItem _gItem;
        private GUIImage _imgIcon;

        public int ColumnID { get; }
        public int RowID { get; }

        public GUIItemBox(Item it = null, ItemBoxDraw e = ItemBoxDraw.MoreThanOne)
        {
            _gBackground = new GUIImage(GUIUtils.ITEM_BOX);
            AddControl(_gBackground);

            SetSize();
            SetItem(it, e);
        }

        public GUIItemBox(int row, int col, Item item, bool drawBox = true)
        {
            if (drawBox)
            {
                _gBackground = new GUIImage(GUIUtils.ITEM_BOX);
                AddControl(_gBackground);
            }
            else
            {
                AddControl(_gBackground);
            }
            SetSize();
            SetItem(item);
            
            ColumnID = col;
            RowID = row;
        }

        private void SetSize()
        {
            if(_gBackground != null)
            {
                Width = _gBackground.Width;
                Height = _gBackground.Height;
            }
            else
            {
                Width = GameManager.ScaledTileSize;
                Height = GameManager.ScaledTileSize;
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (BoxItem != null && BoxItem.Number == 0) {
                SetItem(null);
            }
            _gItem?.Update(gTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_imgIcon != null) {
                _imgIcon.Show(BoxItem == null);
                _imgIcon.Draw(spriteBatch);
            }
        }

        public void SetItem(Item it, ItemBoxDraw e = ItemBoxDraw.MoreThanOne)
        {
            if (it != null)
            {
                if (_gItem == null || (_gItem != null && _gItem.ItemObject != it))
                {
                    RemoveControl(_gItem);
                    _gItem = new GUIItem(it, e);
                    _gItem.CenterOnObject(this);
                }
            }
            else
            {
                RemoveControl(_gItem);
                _gItem = null;
            }
        }

        public override void SetColor(Color c)
        {
            _gItem?.SetColor(c);
        }

        public bool CompareNumToInventory()
        {
            return _gItem.SetNumberComparison(InventoryManager.GetNumberInInventory(_gItem.ItemObject.ID, InventoryManager.PlayerInventory));
        }

        public void SetAlpha(float val)
        {
            Alpha(val);
            SetItemAlpha(val);
        }
        public void SetItemAlpha(float val)
        {
            _gItem?.Alpha(val);
        }

        public void SetItemColor(Color c)
        {
            _gItem?.SetImageColor(c);
        }

        public void DrawShadow(bool drawShadow)
        {
            _gItem?.DrawShadow(drawShadow);
        }
    }

    public class GUIItemBoxHover : GUIItemBox
    {
        public delegate void HoverMethod(GUIItemBoxHover obj);
        private readonly HoverMethod _delAction;

        public GUIItemBoxHover(Item it = null, ItemBoxDraw e = ItemBoxDraw.MoreThanOne, HoverMethod action = null) : base(it, e)
        {
            _delAction = action;
            SetItem(it, e);
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (Contains(mouse))
            {
                rv = true;
                _gItem?.ProcessHover(mouse);
                BeginHover();
            }

            return rv;
        }

        protected override void BeginHover()
        {
            _delAction(this);
        }
    }
}
