﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIObject
    {
        public Item BoxItem => _guiItem?.ItemObject;
        private readonly GUIImage _gBackground;
        private GUIItem _guiItem;
        private GUIImage _imgIcon;
        public EquipmentEnum EquipmentType { get; private set; } = EquipmentEnum.None;

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

        public void SetEquipmentType(EquipmentEnum e)
        {
            EquipmentType = e;
            Rectangle icon;
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

        public void SetItem(Item it, ItemBoxDraw e = ItemBoxDraw.MoreThanOne)
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

        public bool CompareNumToInventory()
        {
            return _guiItem.CompareNumToInventory(null);
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

        public void SetItemColor(Color c)
        {
            _guiItem?.SetImageColor(c);
        }

        public void DrawShadow(bool drawShadow)
        {
            _guiItem?.DrawShadow(drawShadow);
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

        protected override void BeginHover()
        {
            _delAction(this);
        }
    }
}
