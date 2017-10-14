﻿using Adventure.Characters.NPCs;
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Adventure.GUIObjects;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIObjects.Screens
{
    public class PurchaseWorkersScreen : GUIScreen
    {
        private GUIWindow _mainWindow;
        private List<WorkerBox> _workers;

        public PurchaseWorkersScreen(List<Merchandise> merch)
        {
            try
            {
                Vector2 center = new Vector2(AdventureGame.ScreenWidth / 2, AdventureGame.ScreenHeight / 2);

                int minWidth = 64 * merch.Count + 64;
                int minHeight = 128 + 64;
                _mainWindow = new GUIWindow(GUIObject.PosFromCenter(center, minWidth, minHeight), new Vector2(0, 0), 32, minWidth, minHeight);

                int numDivions = merch.Count + 2;
                float xPos = _mainWindow.Position.X + _mainWindow.Width;
                float incrementVal = _mainWindow.Position.Y / numDivions; //If we only display one box, it needs to be centered at the halfway point, so divided by 2
                float yPos = _mainWindow.Position.Y + incrementVal;

                Vector2 position = _mainWindow.Position += new Vector2(32, 32);
                _workers = new List<WorkerBox>();
                foreach (Merchandise m in merch)
                {
                    Worker w = ObjectManager.GetWorker(m.MerchID);

                    _workers.Add(new WorkerBox(position, w, m.MoneyCost));
                    position.X += 64;
                }
                Controls.Add(_mainWindow);
            }
            catch (Exception e)
            {

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (WorkerBox wB in _workers)
            {
                wB.Draw(spriteBatch);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (WorkerBox wB in _workers)
            {
                if (wB.Contains(mouse))
                {
                    //If all items are found, then remove them.
                    if (PlayerManager.Buildings.Count > 0 && PlayerManager.Player.Money >= wB.Cost)
                    {
                        PlayerManager.Player.TakeMoney(wB.Cost);

                        GUIManager.SetScreen(GUIManager.Screens.None);
                        GraphicCursor.PickUpWorker(wB._w.ID);
                        AdventureGame.ChangeGameState(AdventureGame.GameState.Build);
                        Camera.UnsetObserver();
                        MapManager.ViewMap("Map1");
                        rv = true;
                    }
                }
            }

            return rv;
        }
    }

    public class WorkerBox
    {
        private SpriteFont _font;
        private GUIWindow _workerWindow;
        private GUIWindow _costWindow;
        public Worker _w;
        public int Cost;

        public WorkerBox(Vector2 p, Worker w, int cost)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            Cost = cost;
            _w = w;
            _workerWindow = new GUIWindow(p, new Vector2(0, 0), 32, 64, 96);
            _costWindow = new GUIWindow(new Vector2(p.X, p.Y + 96), new Vector2(0, 0), 32, 64, 32);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _workerWindow.Draw(spriteBatch);
            _costWindow.Draw(spriteBatch);
            spriteBatch.Draw(_w.Texture, new Rectangle((int)_workerWindow.Position.X + _workerWindow.EdgeSize, (int)_workerWindow.Position.Y + _workerWindow.EdgeSize, 32, 64), Color.White);
            spriteBatch.DrawString(_font, Cost.ToString(), _costWindow.Position + new Vector2(_costWindow.EdgeSize/2, _costWindow.EdgeSize/2), Color.White);
        }

        public bool Contains(Point mouse)
        {
            return _workerWindow.Contains(mouse);
        }
    }
}
