using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects.Screens
{
    public class PurchaseBuildingsScreen : GUIScreen
    {
        private int margin = 64;
        private List<Merchandise> _merchandise;
        private GUIWindow _mainWindow;
        //private GUIButton _btnNext;
        //private GUIButton _btnLast;
        private GUIImage _btnBuy;
        private SpriteFont _font;
        private List<KeyValuePair<Rectangle, Item>> _requirements;
        private int _currentItemIndex;
        private GUIImage _imgCurrentBuilding;
        private Vector2 moneyStrPos;

        public PurchaseBuildingsScreen(List<Merchandise> merch)
        {
            try
            {
                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
                _merchandise = merch;
                _currentItemIndex = 0;
                WorkerBuilding b = ObjectManager.GetBuilding(_merchandise[_currentItemIndex].MerchID);

                int minWidth = b.Texture.Width + margin * 2;
                int minHeight = b.Texture.Height + margin * 2;
                _font = GameContentManager.GetFont(@"Fonts\Font");
                _mainWindow = new GUIWindow(GUIObject.PosFromCenter(center, minWidth, minHeight), GUIWindow.RedDialog, GUIWindow.RedDialogEdge, minWidth, minHeight);

                _imgCurrentBuilding = new GUIImage(GUIObject.PosFromCenter(center, b.Texture.Width, b.Texture.Height), b.SourceRectangle, b.Texture.Width, b.Texture.Height, b.Texture);
                _btnBuy = new GUIImage(new Vector2(center.X - 32, center.Y + (_mainWindow.UsableRectangle().Height / 2)), new Rectangle(0, 96, 64, 32), 64, 32, @"Textures\Dialog");

                int numDivions = _merchandise.Count+2;
                float xPos = _mainWindow.Position.X + _mainWindow.Width;
                float incrementVal = _mainWindow.Position.Y / numDivions; //If we only display one box, it needs to be centered at the halfway point, so divided by 2
                float yPos = _mainWindow.Position.Y + incrementVal;

                moneyStrPos = new Vector2(xPos-16, yPos-16);
                yPos += incrementVal;
                _requirements = new List<KeyValuePair<Rectangle, Item>>();
                foreach (KeyValuePair<int, int> kvp in _merchandise[_currentItemIndex].RequiredItems)
                {
                    Item i = ObjectManager.GetItem(kvp.Key, kvp.Value);

                    Rectangle r = new Rectangle((int)xPos - 16, (int)yPos - 16, 32, 32);
                    _requirements.Add(new KeyValuePair<Rectangle, Item>(r, i));
                    yPos += incrementVal;
                }

                Controls.Add(_mainWindow);
                Controls.Add(_imgCurrentBuilding);
                Controls.Add(_btnBuy);
            }
            catch (Exception e)
            {

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(_font, _merchandise[_currentItemIndex].MoneyCost.ToString(), moneyStrPos, Color.White);
            foreach (KeyValuePair<Rectangle, Item> kvp in _requirements)
            {
                spriteBatch.Draw(kvp.Value.Texture, kvp.Key, kvp.Value.SourceRectangle, Color.White);
                spriteBatch.DrawString(_font, kvp.Value.Number.ToString(), new Vector2(kvp.Key.Location.X + 32, kvp.Key.Location.Y), Color.White);
                //string text = kvp.Value.Name.ToString();
                //Vector2 textSize = _font.MeasureString(text);
                //spriteBatch.DrawString(_font, _merchandise[_currentItemIndex].ToString(), GUIObject.PosFromCenter(_mainWindow.Width/2, (int)_mainWindow.Position.Y - (int)textSize.Y, (int)textSize.X, (int)textSize.Y), Color.White);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (InputManager.CheckKey(Keys.Escape))
            {
                GameManager.GoToWorldMap();
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_btnBuy.Contains(mouse)){
                bool create = true;
                create = PlayerManager.Money >= _merchandise[_currentItemIndex].MoneyCost;
                if (create)
                {
                    foreach (KeyValuePair<int, int> kvp in _merchandise[_currentItemIndex].RequiredItems)
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
                    PlayerManager.TakeMoney(_merchandise[_currentItemIndex].MoneyCost);
                    foreach (KeyValuePair<int, int> kvp in _merchandise[_currentItemIndex].RequiredItems)
                    {
                        InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                    }

                    GUIManager.SetScreen(null);
                    WorkerBuilding b = ObjectManager.GetBuilding(_merchandise[_currentItemIndex].MerchID);
                    GraphicCursor.PickUpBuilding(b);
                    GameManager.Scry(true);
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
    }
}
