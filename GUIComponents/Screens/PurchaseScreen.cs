﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Actors.ShopKeeper;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.GUIObjects.PartyScreen.NPCDisplayBox;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
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
                _mainWindow = new GUIWindow(GUIWindow.RedWin, minWidth, minHeight);
                _mainWindow.CenterOnScreen();
                _mainWindow.PositionAdd(new Vector2(32, 32));

                _liItems = new List<GUIObject>();

                int i = 0;
                foreach (Merchandise m in merch)
                {
                    Item it = ObjectManager.GetItem(m.MerchID);
                    it.ApplyUniqueData(m.UniqueData);

                    _liItems.Add(new BuyItemBox(it, m.MoneyCost, _mainWindow.InnerRectangle().Width));

                    if (i == 0)
                    {
                        _liItems[i].AnchorToInnerSide(_mainWindow, GUIObject.SideEnum.TopLeft);
                    }
                    else
                    {
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
                int i = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            _gMoney.Update(gameTime);
            foreach (BuyItemBox wB in _liItems)
            {
                wB.Update(gameTime);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (BuyItemBox wB in _liItems)
            {
                if (wB.Contains(mouse) && PlayerManager.Money >= wB.Cost)
                {
                    PlayerManager.TakeMoney(wB.Cost);
                    InventoryManager.AddItemToInventory(new Item(wB.itemForSale));

                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (BuyItemBox wB in _liItems)
            {
                wB.Enable(wB.Contains(mouse));
            }

            return rv;
        }
    }

    public class PurchaseBuildingsScreen : GUIScreen
    {
        private List<Merchandise> _liMerchandise;
        private GUIButton _btnNext;
        private GUIButton _btnLast;
        private GUIButton _btnBuy;
        private int _iCurrIndex;

        private BuildingInfoDisplay _bldgWindow;

        public PurchaseBuildingsScreen(List<Merchandise> merch)
        {
            try
            {
                _liMerchandise = merch;
                _iCurrIndex = 0;

                _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
                AddControl(_bldgWindow);

                _btnBuy = new GUIButton("Buy", MINI_BTN_WIDTH, MINI_BTN_HEIGHT, BtnBuy);
                _btnBuy.AnchorAndAlignToObject(_bldgWindow, SideEnum.Bottom, SideEnum.CenterX, 50);
                _bldgWindow.Load();

                _btnLast = new GUIButton("Last", MINI_BTN_WIDTH, MINI_BTN_HEIGHT, BtnLast);
                _btnLast.AnchorAndAlignToObject(_btnBuy, SideEnum.Left, SideEnum.Bottom, 100);
                _btnNext = new GUIButton("Next", MINI_BTN_WIDTH, MINI_BTN_HEIGHT, BtnNext);
                _btnNext.AnchorAndAlignToObject(_btnBuy, SideEnum.Right, SideEnum.CenterY, 100);
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
            base.Update(gameTime);
            _btnLast.Enabled = _iCurrIndex != 0;
            _btnNext.Enabled = _iCurrIndex != _liMerchandise.Count - 1;

            if (InputManager.CheckPressedKey(Keys.Escape))
            {
                GameManager.GoToWorldMap();
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (!Contains(mouse))
            {
                GUIManager.SetScreen(new HUDScreen());
                GameManager.Unpause();
                rv = true;
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            return true;
        }

        #region Buttons
        public void BtnBuy()
        {
            bool create = true;
            create = PlayerManager.Money >= _liMerchandise[_iCurrIndex].MoneyCost;
            if (create)
            {
                foreach (KeyValuePair<int, int> kvp in _liMerchandise[_iCurrIndex].RequiredItems)
                {
                    if (!InventoryManager.HasItemInInventory(kvp.Key, kvp.Value))
                    {
                        create = false;
                    }
                }
            }

            if (create)
            {
                gmMerchandise = _liMerchandise[_iCurrIndex];

                if (gmMerchandise.MerchType == Merchandise.ItemType.Building)
                {
                    RiverHollow.HomeMapPlacement();
                    GraphicCursor.PickUpBuilding(ObjectManager.GetBuilding(gmMerchandise.MerchID));
                    ConstructBuilding();
                }
                else
                {
                    DiUpgrades[gmMerchandise.MerchID].Enabled = true;
                    gmMerchandise = null;
                    BackToMain();
                }
            }
        }
        public void BtnLast()
        {
            _iCurrIndex--;
            Controls.Remove(_bldgWindow);
            _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
            Controls.Add(_bldgWindow);
            _bldgWindow.Load();
        }
        public void BtnNext()
        {
            _iCurrIndex++;
            Controls.Remove(_bldgWindow);
            _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
            Controls.Add(_bldgWindow);
            _bldgWindow.Load();
        }
        #endregion

        public class BuildingInfoDisplay : GUIObject
        {
            private const int margin = 64;

            Building _bldg;
            public Building Building => _bldg;
            GUIImage _giBuilding;
            SpriteFont _font;
            List<GUIObject> _liReqs;
            Merchandise _merch;
            GUIWindow _bldgWindow;
            GUIWindow _infoWindow;
            GUIMoneyDisplay _gMoney;
            GUIText _gtName;
            GUIText _gtDesc;

            public BuildingInfoDisplay(Merchandise merch)
            {
                _liReqs = new List<GUIObject>();

                _merch = merch;

                if (_merch.MerchType == Merchandise.ItemType.Building)
                {
                    _bldg = ObjectManager.GetBuilding(_merch.MerchID);
                    _font = GameContentManager.GetFont(@"Fonts\Font");

                    float newScale = (float)(Scale * 0.75);
                    int width = (int)(TileSize * 9 * newScale);
                    int height = (int)(TileSize * 11 * newScale);
                    _bldgWindow = new GUIWindow(GUIWindow.RedWin, width, height);
                    _bldgWindow.CenterOnScreen();
                    _bldgWindow.PositionSub(new Vector2(_bldgWindow.Width / 2 + TileSize / 2, 0));

                    _giBuilding = new GUIImage(_bldg.SourceRectangle, _bldg.Texture.Width, _bldg.Texture.Height, _bldg.Texture);
                    _giBuilding.SetScale(newScale);
                    _giBuilding.AnchorToInnerSide(_bldgWindow, SideEnum.Bottom);
                    _giBuilding.AlignToObject(_bldgWindow, SideEnum.CenterX);

                    _infoWindow = new GUIWindow(GUIWindow.RedWin, width, height);
                    _infoWindow.AnchorAndAlignToObject(_bldgWindow, SideEnum.Right, SideEnum.Bottom, TileSize / 2);

                    _gtName = new GUIText(_bldg.Name);
                    _gtName.AnchorToInnerSide(_infoWindow, SideEnum.Top);
                    _gtName.AlignToObject(_infoWindow, SideEnum.CenterX);

                    _gtDesc = new GUIText(_bldg.Description);
                    _gtDesc.ParseText(4, _infoWindow.Width);
                    _gtDesc.AnchorToInnerSide(_infoWindow, SideEnum.Left);
                    _gtDesc.AnchorToObject(_gtName, SideEnum.Bottom);
                }
                else
                {
                    _gtName = new GUIText(DiUpgrades[merch.MerchID].Name);
                    int width = 100;
                    int height = 100;
                    int minWidth = width + margin * 2;
                    int minHeight = height + margin * 2;
                    _bldgWindow = new GUIWindow(GUIWindow.RedWin, minWidth, minHeight);
                    _bldgWindow.CenterOnScreen();

                    _infoWindow = new GUIWindow(GUIWindow.RedWin, width, height);
                    _infoWindow.AnchorAndAlignToObject(_bldgWindow, SideEnum.Right, SideEnum.Bottom, TileSize / 2);

                    //Placeholderimage
                    _giBuilding = new GUICoin();
                    _giBuilding.CenterOnObject(_bldgWindow);
                }

                Width = _bldgWindow.Width + _infoWindow.Width + TileSize;
                Height = _bldgWindow.Height;
                Position(_bldgWindow.Position());
            }

            public void Load()
            {
                _gMoney = new GUIMoneyDisplay(_merch.MoneyCost);

                for (int i = 0; i < _merch.RequiredItems.Count; i++)
                {
                    KeyValuePair<int, int> kvp = _merch.RequiredItems[i];
                    GUIItemReq it = new GUIItemReq(kvp.Key, kvp.Value);
                    _liReqs.Add(it);
                }

                CreateSpacedGrid(ref _liReqs, new Vector2(_infoWindow.InnerLeft(), _infoWindow.DrawRectangle.Center.Y), _bldgWindow.Width, 3);

                _gMoney.AnchorToInnerSide(_infoWindow, SideEnum.BottomRight, 10);

                _bldgWindow.AddControl(_gMoney);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                _bldgWindow.Draw(spriteBatch);
                _infoWindow.Draw(spriteBatch);
                _giBuilding.Draw(spriteBatch);
                _gMoney.Draw(spriteBatch);
                foreach (GUIItemReq c in _liReqs)
                {
                    c.Draw(spriteBatch);
                }
            }
        }
    }

    public class PurchaseWorkersScreen : GUIScreen
    {
        GUIMoneyDisplay _gMoney;
        private GUIWindow _mainWindow;
        private List<GUIObject> _liWorkers;

        public PurchaseWorkersScreen(List<Merchandise> merch)
        {
            try
            {
                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

                int minWidth = 64;
                int minHeight = 64;
                _mainWindow = new GUIWindow(GUIWindow.RedWin, minWidth, minHeight);
                _mainWindow.CenterOnScreen();
                _mainWindow.PositionAdd(new Vector2(32, 32));

                _liWorkers = new List<GUIObject>();

                int i = 0;
                foreach (Merchandise m in merch)
                {
                    if (m.MerchType == Merchandise.ItemType.Worker)
                    {
                        WorldAdventurer w = ObjectManager.GetWorker(m.MerchID);
                        WorkerBox wb = new WorkerBox(w, m.MoneyCost);
                        _liWorkers.Add(wb);

                        if (i == 0) { wb.AnchorToInnerSide(_mainWindow, GUIObject.SideEnum.TopLeft); }
                        else
                        {
                            if (i == merch.Count / 2)
                            {
                                wb.AnchorAndAlignToObject(_liWorkers[0], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left, 20);
                            }
                            else
                            {
                                wb.AnchorAndAlignToObject(_liWorkers[i - 1], GUIObject.SideEnum.Right, GUIObject.SideEnum.Top, 20);
                            }
                        }
                        i++;
                    }
                }

                _mainWindow.Resize();
                _mainWindow.CenterOnScreen();

                _gMoney = new GUIMoneyDisplay();
                _gMoney.AnchorAndAlignToObject(_mainWindow, GUIObject.SideEnum.Top, GUIObject.SideEnum.Left);

                Controls.Add(_mainWindow);
                Controls.Add(_gMoney);
                Controls.Add(_gMoney);
            }
            catch (Exception e)
            {

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (WorkerBox wB in _liWorkers)
            {
                wB.Draw(spriteBatch);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (WorkerBox wB in _liWorkers)
            {
                if (wB.Contains(mouse))
                {
                    //If all items are found, then remove them.
                    if (PlayerManager.Buildings.Count > 0 && PlayerManager.Money >= wB.Cost)
                    {
                        ManagementScreen m = new ManagementScreen();
                        m.PurchaseWorker(ObjectManager.GetWorker(wB.ID), wB.Cost);
                        GUIManager.SetScreen(m);

                        rv = true;
                    }
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (WorkerBox wB in _liWorkers)
            {
                wB.Enable(wB.Contains(mouse));
            }

            return rv;
        }
    }

    public class WorkerBox : GUIObject
    {
        CharacterDisplayBox _workerWindow;
        GUIWindow _costWindow;
        GUIMoneyDisplay _gMoney;
        public int Cost;
        public int ID;

        public WorkerBox(WorldAdventurer w, int cost)
        {
            Cost = cost;
            ID = w.WorkerID;
            _workerWindow = new CharacterDisplayBox(w, null);
            _costWindow = new GUIWindow(GUIWindow.RedWin, _workerWindow.Width, 16);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(_costWindow, SideEnum.TopRight);

            _costWindow.Resize();

            Position(_workerWindow.Position());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _workerWindow.Draw(spriteBatch);
            _costWindow.Draw(spriteBatch);
            _gMoney.Draw(spriteBatch);
        }

        public override bool Contains(Point mouse)
        {
            return _workerWindow.Contains(mouse) || _costWindow.Contains(mouse);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _workerWindow.Position(value);
            _costWindow.AnchorAndAlignToObject(_workerWindow, SideEnum.Bottom, SideEnum.Left);

            Width = _workerWindow.Width;
            Height = _workerWindow.Height + _costWindow.Height;
        }

        public override void Enable(bool val)
        {
            _workerWindow.Enable(val);
            _costWindow.Enable(val);
        }
    }

    public class BuyItemBox : GUIObject
    {
        private SpriteFont _font;
        GUIImage _giItem;
        GUIText _gTextName;
        GUIWindow _gWin;
        GUIMoneyDisplay _gMoney;
        public Item itemForSale;
        public int Cost;

        public BuyItemBox(Item i, int cost, int mainWidth)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            Cost = cost;
            itemForSale = i;
            _giItem = new GUIImage(itemForSale.SourceRectangle, TileSize, TileSize, itemForSale.Texture);
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
            if (!InventoryManager.HasSpaceInInventory(itemForSale.ItemID) || PlayerManager.Money < Cost)
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