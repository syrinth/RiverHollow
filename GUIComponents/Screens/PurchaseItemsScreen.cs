using RiverHollow.Actors.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.WorldObjects;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects.Screens
{
    public class PurchaseItemsScreen : GUIScreen
    {
        GUIMoneyDisplay _gMoney;
        GUIWindow _mainWindow;
        List<GUIObject> _liItems;

        public PurchaseItemsScreen(List<Merchandise> merch)
        {
            try
            {
                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

                int minWidth = 64 * merch.Count + 64;
                int minHeight = 128 + 64;
                _mainWindow = new GUIWindow(GUIObject.PosFromCenter(center, minWidth, minHeight), GUIWindow.RedWin, minWidth, minHeight);

                _mainWindow.PositionAdd(new Vector2(32, 32));
                _liItems = new List<GUIObject>();

                int i = 0;
                foreach (Merchandise m in merch)
                {
                    Item it = ObjectManager.GetItem(m.MerchID);
                    it.ApplyUniqueData(m.UniqueData);

                    _liItems.Add(new ItemBox(it, m.MoneyCost, _mainWindow.InnerRectangle().Width));

                    if(i == 0)
                    {
                        _liItems[i].AnchorToInnerSide(_mainWindow, GUIObject.SideEnum.TopLeft);
                    }
                    else {
                        _liItems[i].AnchorAndAlignToObject(_liItems[i - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);
                    }
                    i++;
                }

                _mainWindow.Resize();

                _gMoney = new GUIMoneyDisplay();
                _gMoney.AnchorAndAlignToObject(_mainWindow, GUIObject.SideEnum.Top, GUIObject.SideEnum.Left);

                Controls.Add(_mainWindow);
                Controls.Add(_gMoney);
            }
            catch (Exception e)
            {

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            _gMoney.Update(gameTime);
            foreach (ItemBox wB in _liItems)
            {
                wB.Update(gameTime);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (ItemBox wB in _liItems)
            {
                if (wB.Contains(mouse) && PlayerManager.Money >= wB.Cost)
                {
                    PlayerManager.TakeMoney(wB.Cost);
                    InventoryManager.AddItemToInventory(new Item(wB.itemForSale));

                    rv = true;
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (ItemBox wB in _liItems)
            {
                wB.Enable(wB.Contains(mouse));
            }

            return rv;
        }
    }

    public class ItemBox : GUIObject
    {
        private SpriteFont _font;
        GUIImage _giItem;
        GUIText _gTextName;
        GUIWindow _gWin;
        GUIMoneyDisplay _gMoney;
        public Item itemForSale;
        public int Cost;

        public ItemBox(Item i, int cost, int mainWidth)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            Cost = cost;
            itemForSale = i;
            _giItem = new GUIImage(Vector2.Zero, itemForSale.SourceRectangle, TileSize, TileSize, itemForSale.Texture);
            _giItem.SetColor(i.ItemColor);
            _gTextName = new GUIText(itemForSale.Name);
            _gWin = new GUIWindow(GUIWindow.BrownWin, mainWidth, 16);

            _giItem.AnchorToInnerSide(_gWin, SideEnum.TopLeft);
            _gTextName.AnchorToObject(_giItem, SideEnum.Right);
            _gTextName.AnchorToInnerSide(_gWin, SideEnum.Top);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(_gWin, SideEnum.TopRight);

            _giItem.AlignToObject(_gTextName, SideEnum.CenterY);

            _gWin.Resize();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            _gWin.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if(!InventoryManager.HasSpaceInInventory(itemForSale.ItemID) || PlayerManager.Money < Cost)
            {
                _gMoney.SetColor(Color.Red);
            }
            else
            {
                _gMoney.SetColor(Color.White);
            }
        }

        public override bool Contains(Point mouse)
        {
            return _gWin.Contains(mouse);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gWin.Position(value);

            Width = _gWin.Width;
            Height = _gWin.Height;
        }

        public override void Enable(bool val)
        {
            _gWin.Enable(val);
        }
    }
}
