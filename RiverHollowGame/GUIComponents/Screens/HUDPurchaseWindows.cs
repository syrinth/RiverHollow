using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;

using static RiverHollow.Characters.ShopKeeper;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox;
using static RiverHollow.GUIComponents.Screens.HUDMenu;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDPurchaseItems : GUIMainObject
    {
        GUIMoneyDisplay _gMoney;
        GUIList _gList;

        public HUDPurchaseItems(List<Merchandise> merch)
        {
            _winMain = SetMainWindow();

            List<GUIObject> items = new List<GUIObject>();

            int i = 0;
            foreach (Merchandise m in merch)
            {
                Item it = DataManager.GetItem(m.MerchID);
                it.ApplyUniqueData(m.UniqueData);

                items.Add(new BuyItemBox(it, m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE));

                if (i == 0) { items[i].AnchorToInnerSide(_winMain, GUIObject.SideEnum.TopLeft); }
                else { items[i].AnchorAndAlignToObject(items[i - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left); }
                i++;
            }

            _gList = new GUIList(items, 10, ScaleIt(2), _winMain.MidHeight());
            _gList.CenterOnObject(_winMain);

            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_winMain, GUIObject.SideEnum.Top, GUIObject.SideEnum.Left);

            AddControl(_gMoney);
            AddControl(_gList);
        }
    }

    //public class HUDPurchaseBuildings : GUIMainObject
    //{
    //    private List<Merchandise> _liMerchandise;
    //    private GUIButton _btnNext;
    //    private GUIButton _btnLast;
    //    private GUIButton _btnBuy;
    //    private int _iCurrIndex;

    //    private BuildingInfoDisplay _bldgWindow;

    //    public HUDPurchaseBuildings(List<Merchandise> merch)
    //    {
    //        _liMerchandise = merch;
    //        _iCurrIndex = 0;

    //        _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
    //        AddControl(_bldgWindow);

    //        _btnBuy = new GUIButton("Buy", GUIManager.MINI_BTN_WIDTH, GUIManager.MINI_BTN_HEIGHT, BtnBuy);
    //        _btnBuy.AnchorAndAlignToObject(_bldgWindow, SideEnum.Bottom, SideEnum.CenterX, 50);
    //        AddControl(_btnBuy);
    //        _bldgWindow.Load();

    //        _btnLast = new GUIButton("Last", GUIManager.MINI_BTN_WIDTH, GUIManager.MINI_BTN_HEIGHT, BtnLast);
    //        _btnLast.AnchorAndAlignToObject(_btnBuy, SideEnum.Left, SideEnum.Bottom, 100);
    //        AddControl(_btnLast);
    //        _btnNext = new GUIButton("Next", GUIManager.MINI_BTN_WIDTH, GUIManager.MINI_BTN_HEIGHT, BtnNext);
    //        _btnNext.AnchorAndAlignToObject(_btnBuy, SideEnum.Right, SideEnum.CenterY, 100);
    //        AddControl(_btnNext);

    //        Width = _bldgWindow.Width;
    //        Height = _bldgWindow.Height;
    //    }

    //    public override void Update(GameTime gTime)
    //    {
    //        base.Update(gTime);
    //        _btnLast.Enable(_iCurrIndex != 0);
    //        _btnNext.Enable(_iCurrIndex != _liMerchandise.Count - 1);

    //        if (InputManager.CheckPressedKey(Keys.Escape))
    //        {
    //            GUIManager.CloseMainObject();
    //        }
    //    }

    //    public override bool ProcessLeftButtonClick(Point mouse)
    //    {
    //        bool rv = false;

    //        foreach (GUIObject c in Controls)
    //        {
    //            rv = c.ProcessLeftButtonClick(mouse);
    //            if (rv) { break; }
    //        }

    //        return rv;
    //    }

    //    public override bool ProcessRightButtonClick(Point mouse)
    //    {
    //        bool rv = true;
    //        if (!Contains(mouse))
    //        {
    //            GUIManager.CloseMainObject();
    //            GameManager.Unpause();
    //            rv = true;
    //        }

    //        return rv;
    //    }

    //    public override bool ProcessHover(Point mouse)
    //    {
    //        return true;
    //    }

    //    #region Buttons
    //    public void BtnBuy()
    //    {
    //        bool create = true;
    //        create = PlayerManager.Money >= _liMerchandise[_iCurrIndex].MoneyCost;
    //        if (create)
    //        {
    //            foreach (KeyValuePair<int, int> kvp in _liMerchandise[_iCurrIndex].RequiredItems)
    //            {
    //                if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
    //                {
    //                    create = false;
    //                }
    //            }
    //        }

    //        if (create)
    //        {
    //            CurrentMerch = _liMerchandise[_iCurrIndex];

    //            if (CurrentMerch.MerchType == Merchandise.ItemType.Building)
    //            {
    //                RiverHollow.EnterBuildMode();
    //                GameManager.PickUpBuilding(DataManager.GetBuilding(CurrentMerch.MerchID));
    //                ConstructBuilding();
    //            }
    //            else
    //            {
    //                DiUpgrades[CurrentMerch.MerchID].Enabled = true;
    //                CurrentMerch = null;
    //                GUIManager.CloseMainObject();
    //            }
    //        }
    //    }
    //    public void BtnLast()
    //    {
    //        _iCurrIndex--;
    //        RemoveControl(_bldgWindow);
    //        _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
    //        AddControl(_bldgWindow);
    //        _bldgWindow.Load();
    //    }
    //    public void BtnNext()
    //    {
    //        _iCurrIndex++;
    //        RemoveControl(_bldgWindow);
    //        _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
    //        AddControl(_bldgWindow);
    //        _bldgWindow.Load();
    //    }
    //    #endregion

    //    public class BuildingInfoDisplay : GUIObject
    //    {
    //        private const int margin = 64;

    //        Building _bldg;
    //        public Building Building => _bldg;
    //        GUISprite _giBuilding;
    //        BitmapFont _font;
    //        List<GUIObject> _liReqs;
    //        Merchandise _merch;
    //        GUIWindow _bldgWindow;
    //        GUIWindow _infoWindow;
    //        GUIMoneyDisplay _gMoney;
    //        GUIText _gtName;
    //        GUIText _gtDesc;

    //        public BuildingInfoDisplay(Merchandise merch)
    //        {
    //            _liReqs = new List<GUIObject>();

    //            _merch = merch;

    //            if (_merch.MerchType == Merchandise.ItemType.Building)
    //            {
    //                _bldg = DataManager.GetBuilding(_merch.MerchID);
    //                _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);

    //                float newScale = (float)(Scale * 0.75);
    //                int width = (int)(TileSize * 9 * newScale);
    //                int height = (int)(TileSize * 11 * newScale);
    //                _bldgWindow = new GUIWindow(GUIWindow.Window_1, width, height);
    //                _bldgWindow.CenterOnScreen();
    //                _bldgWindow.PositionSub(new Vector2(_bldgWindow.Width / 2 + TileSize / 2, 0));

    //                _giBuilding = new GUISprite(_bldg.Sprite);
    //                _giBuilding.SetScale(newScale);
    //                _giBuilding.AnchorToInnerSide(_bldgWindow, SideEnum.Bottom);
    //                _giBuilding.AlignToObject(_bldgWindow, SideEnum.CenterX);

    //                _infoWindow = new GUIWindow(GUIWindow.Window_1, width, height);
    //                _infoWindow.AnchorAndAlignToObject(_bldgWindow, SideEnum.Right, SideEnum.Bottom, TileSize / 2);

    //                _gtName = new GUIText(_bldg.Name);
    //                _gtName.AnchorToInnerSide(_infoWindow, SideEnum.Top);
    //                _gtName.AlignToObject(_infoWindow, SideEnum.CenterX);

    //                _gtDesc = new GUIText();
    //                _gtDesc.ParseAndSetText(_bldg.Description, _infoWindow.Width, 4, true);
    //                _gtDesc.AnchorToInnerSide(_infoWindow, SideEnum.Left);
    //                _gtDesc.AnchorToObject(_gtName, SideEnum.Bottom);
    //            }
    //            else
    //            {
    //                _gtName = new GUIText(DiUpgrades[merch.MerchID].Name);
    //                int width = 100;
    //                int height = 100;
    //                int minWidth = width + margin * 2;
    //                int minHeight = height + margin * 2;
    //                _bldgWindow = new GUIWindow(GUIWindow.Window_1, minWidth, minHeight);
    //                _bldgWindow.CenterOnScreen();

    //                _infoWindow = new GUIWindow(GUIWindow.Window_1, width, height);
    //                _infoWindow.AnchorAndAlignToObject(_bldgWindow, SideEnum.Right, SideEnum.Bottom, TileSize / 2);

    //                //Placeholderimage
    //                _giBuilding = new GUISprite(new SpriteAnimations.AnimatedSprite(DataManager.FILE_WORLDOBJECTS));
    //                _giBuilding.CenterOnObject(_bldgWindow);
    //            }

    //            Width = _bldgWindow.Width + _infoWindow.Width + TileSize;
    //            Height = _bldgWindow.Height;
    //            Position(_bldgWindow.Position());
    //        }

    //        public void Load()
    //        {
    //            _gMoney = new GUIMoneyDisplay(_merch.MoneyCost);

    //            for (int i = 0; i < _merch.RequiredItems.Count; i++)
    //            {
    //                KeyValuePair<int, int> kvp = _merch.RequiredItems[i];
    //                GUIItem it = new GUIItem(DataManager.GetItem(kvp.Key, kvp.Value));
    //                _liReqs.Add(it);
    //            }

    //            CreateSpacedGrid(ref _liReqs, new Vector2(_infoWindow.InnerLeft(), _infoWindow.DrawRectangle.Center.Y), _bldgWindow.Width, 3);

    //            _gMoney.AnchorToInnerSide(_infoWindow, SideEnum.BottomRight, 10);

    //            _bldgWindow.AddControl(_gMoney);
    //        }

    //        public override void Draw(SpriteBatch spriteBatch)
    //        {
    //            _bldgWindow.Draw(spriteBatch);
    //            _infoWindow.Draw(spriteBatch);
    //            _giBuilding.Draw(spriteBatch);
    //            _gMoney.Draw(spriteBatch);
    //            foreach (GUIItem c in _liReqs)
    //            {
    //                c.Draw(spriteBatch);
    //            }
    //        }
    //    }
    //}

    public class HUDPurchaseWorkers : GUIMainObject
    {
        GUIMoneyDisplay _gMoney;
        GUIText _gName;
        GUIText _gDescription;
        GUIMoneyDisplay _gCost;
        GUIWindow _winWorkers;
        List<GUIObject> _liWorkers;
        WorkerBox _currentWorker;
        GUIButton _btnBuy;
        GUIItem _gDailyItem;

        public HUDPurchaseWorkers(List<Merchandise> merch)
        {
            Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

            int minWidth = 64;
            int minHeight = 64;
            _winWorkers = new GUIWindow(GUIWindow.Window_1, minWidth, minHeight);
            _liWorkers = new List<GUIObject>();

            int i = 0;
            foreach (Merchandise m in merch)
            {
                if (m.MerchType == Merchandise.ItemType.Adventurer)
                {
                    Adventurer w = DataManager.GetAdventurer(m.MerchID);
                    WorkerBox wb = new WorkerBox(w, m.MoneyCost);
                    _liWorkers.Add(wb);

                    if (i == 0) { wb.AnchorToInnerSide(_winWorkers, GUIObject.SideEnum.TopLeft); }
                    else {wb.AnchorAndAlignToObject(_liWorkers[i - 1], GUIObject.SideEnum.Right, GUIObject.SideEnum.Top, ScaleIt(1)); }

                    _winWorkers.AddControl(wb);
                    i++;
                }
            }

            _winWorkers.Resize();

            _winMain = new GUIWindow(GUIWindow.Window_1, _winWorkers.Width, GUIManager.MAIN_COMPONENT_HEIGHT/2);
            
            _gMoney = new GUIMoneyDisplay();
            _winWorkers.AnchorAndAlignToObject(_gMoney, SideEnum.Bottom, SideEnum.Left);
            _winMain.AnchorAndAlignToObject(_winWorkers, SideEnum.Bottom, SideEnum.Left, ScaleIt(1));

            AddControl(_winMain);
            AddControl(_winWorkers);
            AddControl(_gMoney);

            _gName = new GUIText();
            _gName.AnchorToInnerSide(_winMain, SideEnum.TopLeft, ScaleIt(1));
            _gDescription = new GUIText();
            _gDescription.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left, ScaleIt(1));
            _btnBuy = new GUIButton("Buy", PurchaseWorker);
            _btnBuy.AnchorToInnerSide(_winMain, SideEnum.BottomRight, ScaleIt(1));

            Width = _winWorkers.Width;
            Height = _winMain.Bottom - _gMoney.Top;

            CenterOnScreen();
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (WorkerBox wB in _liWorkers)
            {
                bool hovering = wB.Contains(mouse);
                if (wB != _currentWorker)
                {
                    wB.Enable(hovering);

                    if (hovering)
                    {
                        string value = string.Empty;
                        DataManager.GetTextData("Class", wB.ID, ref value, "Name");
                        _gName.SetText(value);

                        DataManager.GetTextData("Class", wB.ID, ref value, "Description");
                        _gDescription.ParseAndSetText(value, _winMain.MidWidth(), 3, true, false);

                        _winMain.RemoveControl(_gCost);
                        _gCost = new GUIMoneyDisplay(wB.Cost);
                        _gCost.AnchorAndAlignToObject(_btnBuy, SideEnum.Left, SideEnum.Bottom, ScaleIt(1));

                        _winMain.RemoveControl(_gDailyItem);
                        _gDailyItem = new GUIItem(DataManager.GetItem(DataManager.GetAdventurer(wB.ID).DailyItemID));
                        _gDailyItem.AnchorToInnerSide(_winMain, SideEnum.BottomLeft, ScaleIt(1));

                        _currentWorker = wB;
                    }
                }
            }

            return rv;
        }

        private void PurchaseWorker()
        {
            //If all items are found, then remove them.
            //if (PlayerManager._diBuildings.Count > 0 && PlayerManager.Money >= _currentWorker.Cost)
            //{
            //    HUDManagement m = new HUDManagement();
            //    m.PurchaseWorker(DataManager.GetAdventurer(_currentWorker.ID), _currentWorker.Cost);
            //    GUIManager.OpenMainObject(m);
            //}
        }
    }

    public class WorkerBox : GUIObject
    {
        CharacterDisplayBox _workerWindow;
        public int Cost;
        public int ID;

        public WorkerBox(Adventurer w, int cost)
        {
            Cost = cost;
            ID = w.WorkerID;
            _workerWindow = new CharacterDisplayBox(w, null);
            AddControl(_workerWindow);

            Width = _workerWindow.Width;
            Height = _workerWindow.Height;
        }
    }

    public class BuyItemBox : GUIWindow
    {
        private BitmapFont _font;
        GUIImage _giItem;
        GUIText _gTextName;
        GUIMoneyDisplay _gMoney;
        public Item itemForSale;
        public int Cost;

        public BuyItemBox(Item i, int cost, int mainWidth) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
            _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
            Cost = cost;
            itemForSale = i;
            _giItem = new GUIImage(itemForSale.SourceRectangle, ScaledTileSize, ScaledTileSize, itemForSale.Texture);
            _giItem.SetColor(i.ItemColor);
            _gTextName = new GUIText(itemForSale.Name);

            _giItem.AnchorToInnerSide(this, SideEnum.Left);
            _gTextName.AnchorAndAlignToObject(_giItem, SideEnum.Right, SideEnum.CenterY);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(this, SideEnum.Right, ScaleIt(2));
        }

        public override void Update(GameTime gTime)
        {
            if (!InventoryManager.HasSpaceInInventory(itemForSale.ItemID, itemForSale.Number) || PlayerManager.Money < Cost)
            {
                _gMoney.SetColor(Color.Red);
            }
            else
            {
                _gMoney.SetColor(Color.White);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (PlayerManager.Money >= Cost)
            {
                PlayerManager.TakeMoney(Cost);
                InventoryManager.AddToInventory(DataManager.GetItem(itemForSale.ItemID));

                rv = true;
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = Contains(mouse);

            Enable(rv);

            return rv;
        }
    }
}