using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using static RiverHollow.GUIObjects.GUIObject;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects.Screens
{
    public class PurchaseBuildingsScreen : GUIScreen
    {
        private int margin = 64;
        private List<Merchandise> _liMerchandise;
        private GUIButton _btnNext;
        private GUIButton _btnLast;
        private GUIButton _btnBuy;
        private int _iCurrIndex;

        private GUIWindow _mainWindow;
        private BuildingInfoDisplay _bldgWindow;

        public PurchaseBuildingsScreen(List<Merchandise> merch)
        {
            try
            {
                _liMerchandise = merch;
                _iCurrIndex = 0;

                _bldgWindow = new BuildingInfoDisplay(this, _liMerchandise[_iCurrIndex]);

                int minWidth = _bldgWindow.Building.Texture.Width + margin * 2;
                int minHeight = _bldgWindow.Building.Texture.Height + margin * 2;

                _mainWindow = new GUIWindow(Vector2.Zero, GUIWindow.RedWin, minWidth, minHeight);
                _mainWindow.CenterOnScreen();

                _btnBuy = new GUIButton("Buy");
                _btnBuy.AnchorAndAlignToObject(_mainWindow, SideEnum.Bottom, SideEnum.Right, 5);
                _bldgWindow.Load();

                _btnLast = new GUIButton("Last");
                _btnLast.CenterOnWindow(_mainWindow);
                _btnLast.AnchorAndAlignToObject(_mainWindow, SideEnum.Left, SideEnum.Top);
                _btnNext = new GUIButton("Next");
                _btnNext.CenterOnWindow(_mainWindow);
                _btnNext.AnchorAndAlignToObject(_mainWindow, SideEnum.Right, SideEnum.Top);

                Controls.Add(_mainWindow);
                Controls.Add(_btnBuy);
                Controls.Add(_bldgWindow);
                Controls.Add(_btnLast);
                Controls.Add(_btnNext);
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
            _btnNext.Enabled = _iCurrIndex != _liMerchandise.Count-1;

            if (InputManager.CheckPressedKey(Keys.Escape))
            {
                GameManager.GoToWorldMap();
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_btnLast.ProcessLeftButtonClick(mouse))
            {
                _iCurrIndex--;
                Controls.Remove(_bldgWindow);
                _bldgWindow = new BuildingInfoDisplay(this, _liMerchandise[_iCurrIndex]);
                Controls.Add(_bldgWindow);
                _bldgWindow.Load();
            }
            else if (_btnNext.ProcessLeftButtonClick(mouse))
            {
                _iCurrIndex++;
                Controls.Remove(_bldgWindow);
                _bldgWindow = new BuildingInfoDisplay(this, _liMerchandise[_iCurrIndex]);
                Controls.Add(_bldgWindow);
                _bldgWindow.Load();
            }

            if (_btnBuy.Contains(mouse))
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
                //If all items are found, then remove them.
                if (create)
                {
                    PlayerManager.TakeMoney(_liMerchandise[_iCurrIndex].MoneyCost);
                    foreach (KeyValuePair<int, int> kvp in _liMerchandise[_iCurrIndex].RequiredItems)
                    {
                        InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                    }

                    GUIManager.SetScreen(null);
                    WorkerBuilding b = ObjectManager.GetBuilding(_liMerchandise[_iCurrIndex].MerchID);
                    GraphicCursor.PickUpBuilding(b);
                    GameManager.Scry(true);
                    GameManager.ConstructBuilding();
                    Camera.UnsetObserver();
                    MapManager.ViewMap(MapManager.HomeMap);
                    rv = true;
                }
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

        public class BuildingInfoDisplay : GUIObject
        {
            WorkerBuilding _bldg;
            public WorkerBuilding Building => _bldg;
            GUIImage _giBuilding;
            SpriteFont _font;
            List<GUIObject> _liReqs;
            Merchandise _merch;
            PurchaseBuildingsScreen _sParent;
            GUIText _gText;

            public BuildingInfoDisplay(PurchaseBuildingsScreen parent, Merchandise merch)
            {
                _liReqs = new List<GUIObject>();

                _sParent = parent;
                _merch = merch;
                _bldg = ObjectManager.GetBuilding(_merch.MerchID);
                _font = GameContentManager.GetFont(@"Fonts\Font");

                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
                _giBuilding = new GUIImage(GUIObject.PosFromCenter(center, _bldg.Texture.Width, _bldg.Texture.Height), _bldg.SourceRectangle, _bldg.Texture.Width, _bldg.Texture.Height, _bldg.Texture);
            }

            public void Load()
            {
                GUIWindow win = _sParent._mainWindow;
                _gText = new GUIText(_merch.MoneyCost.ToString());

                for(int i=0; i< _merch.RequiredItems.Count; i++)
                {
                    KeyValuePair<int, int> kvp = _merch.RequiredItems[i];
                    ItemCost it = new ItemCost(kvp.Key, kvp.Value);
                    _liReqs.Add(it);
                }

                CreateSpacedColumn(ref _liReqs, win.DrawRectangle.Right, win.DrawRectangle.Top, win.Height, 10, true);
                _gText.AnchorAndAlignToObject(_liReqs[_liReqs.Count-1], SideEnum.Bottom, SideEnum.Left, 10);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                _giBuilding.Draw(spriteBatch);
                _gText.Draw(spriteBatch);
                foreach (ItemCost c in _liReqs)
                {
                    c.Draw(spriteBatch);
                }
            }

            public class ItemCost : GUIObject
            {
                GUIImage _gImg;
                GUIText _gText;

                public ItemCost(int id, int number)
                {
                    Item it = ObjectManager.GetItem(id);
                    _gImg = new GUIImage(Vector2.Zero, it.SourceRectangle, it.SourceRectangle.Width, it.SourceRectangle.Height, it.Texture);
                    _gImg.SetScale(GameManager.Scale);
                    _gText = new GUIText(number.ToString());
                    Width = _gImg.Width + _gText.Width;
                    Height = _gImg.Height;
                    Position(Vector2.Zero);
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    _gImg.Draw(spriteBatch);
                    _gText.Draw(spriteBatch);
                }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    _gImg.Position(value);
                    _gText.AnchorAndAlignToObject(_gImg, SideEnum.Right, SideEnum.Bottom);
                }
            }
        }
    }
}
