using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects.Screens
{
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
                _mainWindow = new GUIWindow(GUIObject.PosFromCenter(center, minWidth, minHeight), GUIWindow.RedWin, minWidth, minHeight);

                _mainWindow.PositionAdd(new Vector2(32, 32));
                Vector2 position = _mainWindow.Position();
                _liWorkers = new List<GUIObject>();

                int i = 0;
                foreach (Merchandise m in merch)
                {
                    if (m.MerchType == Merchandise.ItemType.Worker)
                    {
                        WorldAdventurer w = ObjectManager.GetWorker(m.MerchID);
                        WorkerBox wb = new WorkerBox(position, w, m.MoneyCost);
                        _liWorkers.Add(wb);

                        if (i == 0) { wb.AnchorToInnerSide(_mainWindow, GUIObject.SideEnum.TopLeft); }
                        else {
                            if (i == merch.Count / 2)
                            {
                                wb.AnchorAndAlignToObject(_liWorkers[0], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left, 20);
                                _mainWindow.AddControl(wb);
                            }
                            else{
                                wb.AnchorAndAlignToObject(_liWorkers[i - 1], GUIObject.SideEnum.Right, GUIObject.SideEnum.Top, 20);
                                _mainWindow.AddControl(wb);
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
                        m.PurchaseWorker(ObjectManager.GetWorker(wB._w.AdventurerID), wB.Cost);
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
        GUIWindow _workerWindow;
        GUIWindow _costWindow;
        GUISprite _sprite;
        GUIMoneyDisplay _gMoney;
        public WorldAdventurer _w;
        public int Cost;

        public WorkerBox(Vector2 p, WorldAdventurer w, int cost)
        {
            Cost = cost;
            _w = w;
            _sprite = new GUISprite(w.BodySprite);
            _sprite.SetScale((int)GameManager.Scale);
            _workerWindow = new GUIWindow(p, GUIWindow.RedWin, _sprite.Width + _sprite.Width / 3, _sprite.Height + 2 * _sprite.Height / 4);
            _costWindow = new GUIWindow(Vector2.Zero, GUIWindow.RedWin, _sprite.Width + _sprite.Width / 3, 16);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(_costWindow, SideEnum.TopRight);

            _costWindow.AddControl(_gMoney);
            _costWindow.Resize();

            Position(_workerWindow.Position());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _workerWindow.Draw(spriteBatch);
            _costWindow.Draw(spriteBatch);
            _sprite.Draw(spriteBatch);
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
            _sprite.CenterOnWindow(_workerWindow);
            _sprite.AnchorToInnerSide(_workerWindow, SideEnum.Bottom);

            Width = _workerWindow.Width;
            Height = _workerWindow.Height + _costWindow.Height;
        }

        public override void Enable(bool val)
        {
            _workerWindow.Enable(val);
            _costWindow.Enable(val);
        }
    }
}
