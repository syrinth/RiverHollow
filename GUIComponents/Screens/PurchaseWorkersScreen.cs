using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

namespace RiverHollow.Game_Managers.GUIObjects.Screens
{
    public class PurchaseWorkersScreen : GUIScreen
    {
        private GUIWindow _mainWindow;
        private List<GUIObject> _liWorkers;

        public PurchaseWorkersScreen(List<Merchandise> merch)
        {
            try
            {
                Vector2 center = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

                int minWidth = 64 * merch.Count + 64;
                int minHeight = 128 + 64;
                _mainWindow = new GUIWindow(GUIObject.PosFromCenter(center, minWidth, minHeight), GUIWindow.RedWin, minWidth, minHeight);

                int numDivions = merch.Count + 2;
                float xPos = _mainWindow.Position().X + _mainWindow.Width;
                float incrementVal = _mainWindow.Position().Y / numDivions; //If we only display one box, it needs to be centered at the halfway point, so divided by 2
                float yPos = _mainWindow.Position().Y + incrementVal;

                _mainWindow.PositionAdd(new Vector2(32, 32));
                Vector2 position = _mainWindow.Position();
                _liWorkers = new List<GUIObject>();


                foreach (Merchandise m in merch)
                {
                    WorldAdventurer w = ObjectManager.GetWorker(m.MerchID);

                    _liWorkers.Add(new WorkerBox(position, w, m.MoneyCost));
                }
                

                GUIObject.CreateSpacedRow(ref _liWorkers, RiverHollow.ScreenHeight / 2, _mainWindow.Position().X, RiverHollow.ScreenWidth/10, 20);
                Controls.Add(_mainWindow);
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
                        m.PurchaseWorker(ObjectManager.GetWorker(wB._w.AdventurerID), wB.Cost);
                        GUIManager.SetScreen(m);

                        rv = true;
                    }
                }
            }

            return rv;
        }
    }

    public class WorkerBox : GUIObject
    {
        private SpriteFont _font;
        private GUIWindow _workerWindow;
        private GUIWindow _costWindow;
        private GUISprite _sprite;
        public WorldAdventurer _w;
        public int Cost;

        public WorkerBox(Vector2 p, WorldAdventurer w, int cost)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            Cost = cost;
            _w = w;
            _sprite = new GUISprite(w.Sprite);
            _sprite.SetScale((int)GameManager.Scale);
            _workerWindow = new GUIWindow(p, GUIWindow.RedWin, _sprite.Width + _sprite.Width / 3, _sprite.Height + 2 * _sprite.Height / 4);
            _costWindow = new GUIWindow(new Vector2(p.X, p.Y + 96), GUIWindow.RedWin, _sprite.Width + _sprite.Width / 3, 32);

            Position(_workerWindow.Position());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _workerWindow.Draw(spriteBatch);
            _costWindow.Draw(spriteBatch);
            _sprite.Draw(spriteBatch);
            spriteBatch.DrawString(_font, Cost.ToString(), _costWindow.Position() + new Vector2(_costWindow.EdgeSize/2, _costWindow.EdgeSize/2), Color.White);
        }

        public override bool Contains(Point mouse)
        {
            return _workerWindow.Contains(mouse);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _workerWindow.Position(value);
            _costWindow.AnchorToObject(_workerWindow, SideEnum.Bottom);
            _sprite.CenterOnWindow(_workerWindow);
            _sprite.AnchorToInnerSide(_workerWindow, SideEnum.Bottom);

            Width = _workerWindow.Width;
            Height = _workerWindow.Height;
        }
    }
}
