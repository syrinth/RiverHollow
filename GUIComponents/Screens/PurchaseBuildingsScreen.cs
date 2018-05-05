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
            GUIImage _imgBuilding;
            SpriteFont _font;
            List<KeyValuePair<Rectangle, Item>> _liReqs;
            Vector2 _vCostPos;
            Merchandise _merch;
            PurchaseBuildingsScreen _parent;

            public BuildingInfoDisplay(PurchaseBuildingsScreen parent, Merchandise merch)
            {
                _parent = parent;
                _merch = merch;
                _bldg = ObjectManager.GetBuilding(_merch.MerchID);
                _font = GameContentManager.GetFont(@"Fonts\Font");

                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
                _imgBuilding = new GUIImage(GUIObject.PosFromCenter(center, _bldg.Texture.Width, _bldg.Texture.Height), _bldg.SourceRectangle, _bldg.Texture.Width, _bldg.Texture.Height, _bldg.Texture);
            }

            public void Load()
            {
                GUIWindow win = _parent._mainWindow;
                int numDivions = _parent._liMerchandise.Count + 2;
                float xPos = win.Position().X + win.Width;
                float incrementVal = win.Position().Y / numDivions; //If we only display one box, it needs to be centered at the halfway point, so divided by 2
                float yPos = win.Position().Y + incrementVal;

                _vCostPos = new Vector2(xPos - 16, yPos - 16);
                yPos += incrementVal;
                _liReqs = new List<KeyValuePair<Rectangle, Item>>();
                foreach (KeyValuePair<int, int> kvp in _parent._liMerchandise[_parent._iCurrIndex].RequiredItems)
                {
                    Item i = ObjectManager.GetItem(kvp.Key, kvp.Value);

                    Rectangle r = new Rectangle((int)xPos - 16, (int)yPos - 16, 32, 32);
                    _liReqs.Add(new KeyValuePair<Rectangle, Item>(r, i));
                    yPos += incrementVal;
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                _imgBuilding.Draw(spriteBatch);
                spriteBatch.DrawString(_font, _merch.MoneyCost.ToString(), _vCostPos, Color.White);
                foreach (KeyValuePair<Rectangle, Item> kvp in _liReqs)
                {
                    spriteBatch.Draw(kvp.Value.Texture, kvp.Key, kvp.Value.SourceRectangle, Color.White);
                    spriteBatch.DrawString(_font, kvp.Value.Number.ToString(), new Vector2(kvp.Key.Location.X + 32, kvp.Key.Location.Y), Color.White);
                }
            }
        }
    }
}
