using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
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

                _btnBuy = new GUIButton("Buy");
                _btnBuy.AnchorAndAlignToObject(_bldgWindow, SideEnum.Bottom, SideEnum.CenterX, 50);
                _bldgWindow.Load();

                _btnLast = new GUIButton("Last");
                _btnLast.AnchorAndAlignToObject(_btnBuy, SideEnum.Left, SideEnum.Bottom, 100);
                _btnNext = new GUIButton("Next");
                _btnNext.AnchorAndAlignToObject(_btnBuy, SideEnum.Right, SideEnum.CenterY, 100);

                Controls.Add(_btnBuy);
                Controls.Add(_btnLast);
                Controls.Add(_btnNext);
                Controls.Add(_bldgWindow);
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
                _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
                Controls.Add(_bldgWindow);
                _bldgWindow.Load();
            }
            else if (_btnNext.ProcessLeftButtonClick(mouse))
            {
                _iCurrIndex++;
                Controls.Remove(_bldgWindow);
                _bldgWindow = new BuildingInfoDisplay(_liMerchandise[_iCurrIndex]);
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
            private const int margin = 64;

            WorkerBuilding _bldg;
            public WorkerBuilding Building => _bldg;
            GUIImage _giBuilding;
            SpriteFont _font;
            List<GUIObject> _liReqs;
            Merchandise _merch;
            GUIText _gText;
            GUIWindow _mainWindow;

            public BuildingInfoDisplay(Merchandise merch)
            {
                _liReqs = new List<GUIObject>();

                _merch = merch;
                _bldg = ObjectManager.GetBuilding(_merch.MerchID);
                _font = GameContentManager.GetFont(@"Fonts\Font");

                int minWidth = _bldg.Texture.Width + margin * 2;
                int minHeight = _bldg.Texture.Height + margin * 2;
                _mainWindow = new GUIWindow(Vector2.Zero, GUIWindow.RedWin, minWidth, minHeight);
                _mainWindow.CenterOnScreen();

                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
                _giBuilding = new GUIImage(PosFromCenter(center, _bldg.Texture.Width, _bldg.Texture.Height), _bldg.SourceRectangle, _bldg.Texture.Width, _bldg.Texture.Height, _bldg.Texture);

                Width = _mainWindow.Width;
                Height = _mainWindow.Height;
                Position(_mainWindow.Position());
            }

            public void Load()
            {
                _gText = new GUIText(_merch.MoneyCost.ToString());

                for(int i=0; i< _merch.RequiredItems.Count; i++)
                {
                    KeyValuePair<int, int> kvp = _merch.RequiredItems[i];
                    GUIItemReq it = new GUIItemReq(kvp.Key, kvp.Value);
                    _liReqs.Add(it);
                }

                CreateSpacedColumn(ref _liReqs, _mainWindow.DrawRectangle.Right, _mainWindow.DrawRectangle.Top, _mainWindow.Height, 10, true);
                _gText.AnchorAndAlignToObject(_liReqs[_liReqs.Count-1], SideEnum.Bottom, SideEnum.Left, 10);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                _mainWindow.Draw(spriteBatch);
                _giBuilding.Draw(spriteBatch);
                _gText.Draw(spriteBatch);
                foreach (GUIItemReq c in _liReqs)
                {
                    c.Draw(spriteBatch);
                }
            }
        }
    }
}
