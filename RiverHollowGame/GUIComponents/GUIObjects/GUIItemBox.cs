﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        public Item BoxItem => _guiItem?.ItemObject;
        GUIItem _guiItem;
        GUIImage _imgIcon;
        public EquipmentEnum EquipmentType { get; private set; } = EquipmentEnum.None;

        public int ColumnID { get; }
        public int RowID { get; }

        bool _bAlignUnder;

        public GUIItemBox(Item it = null, ItemBoxDraw e = ItemBoxDraw.OnlyStacks, bool alignUnder = true) : base(GUIUtils.ITEM_BOX)
        {
            _bAlignUnder = alignUnder;
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_imgIcon != null) {
                _imgIcon.Show(BoxItem == null);
                _imgIcon.Draw(spriteBatch);
            }
        }

        protected override void BeginHover()
        {
            if (BoxItem != null)
            {
                var win = new GUIItemDescriptionWindow(BoxItem, new Point(DrawRectangle.Left, DrawRectangle.Bottom));
                if (_bAlignUnder) { win.AnchorAndAlignWithSpacing(this, SideEnum.Bottom, SideEnum.CenterX, -1); }
                else { win.AnchorToScreen(SideEnum.BottomRight); }
                GUIManager.OpenHoverObject(win, DrawRectangle, true);
            }
        }

        public void SetEquipmentType(EquipmentEnum e)
        {
            EquipmentType = e;
            Rectangle icon = Rectangle.Empty;
            switch (EquipmentType)
            {
                case EquipmentEnum.Hat:
                    icon = GUIUtils.INVENTORY_ICON_HAT;
                    break;
                case EquipmentEnum.Shirt:
                    icon = GUIUtils.INVENTORY_ICON_SHIRT;
                    break;
                case EquipmentEnum.Pants:
                    icon = GUIUtils.INVENTORY_ICON_PANTS;
                    break;
                case EquipmentEnum.Neck:
                    icon = GUIUtils.INVENTORY_ICON_NECK;
                    break;
                case EquipmentEnum.Ring:
                    icon = GUIUtils.INVENTORY_ICON_RING;
                    break;
                default:
                    icon = Rectangle.Empty;
                    break;
            }

            _imgIcon = new GUIImage(icon);
            _imgIcon.CenterOnObject(this, GUIUtils.ParentRuleEnum.ForceToObject);
        }

        public void SetItem(Item it, ItemBoxDraw e = ItemBoxDraw.OnlyStacks)
        {
            HoverControls = false;

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
    }

    public class GUIItemBoxHover : GUIItemBox
    {
        public delegate void HoverMethod(GUIItemBoxHover obj);
        private HoverMethod _delAction;

        public GUIItemBoxHover(Item it = null, ItemBoxDraw e = ItemBoxDraw.OnlyStacks, HoverMethod action = null) : base(it, e)
        {
            _delAction = action;
            SetItem(it, e);
        }

        protected override void BeginHover()
        {
            _delAction(this);
        }
    }
}
