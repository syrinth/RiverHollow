using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GameMenuScreen : GUIScreen
    {
        const int BTN_PADDING = 120;
        const int BTN_WIDTH = 128;
        const int BTN_HEIGHT = 64;
        private GUIButton _btnExitGame;
        private GUIButton _btnQuestLog;
        private GUIButton _btnInventory;
        private GUIButton _btnParty;
        private GUIScreen _infoScreen;
        List<GUIObject> _liButtons;

        bool _open = false;
        bool _close = false;

        public GameMenuScreen()
        {
            _btnInventory = new GUIButton("Inventory");
            _btnParty = new GUIButton("Party");
            _btnQuestLog = new GUIButton("Quest Log");
            _btnExitGame = new GUIButton("Exit Game");

            _liButtons = new List<GUIObject>() { _btnInventory, _btnParty, _btnQuestLog, _btnExitGame };
            GUIObject.CreateSpacedColumn(ref _liButtons, -BTN_WIDTH, RiverHollow.ScreenHeight, BTN_PADDING, BTN_WIDTH, BTN_HEIGHT);
            foreach(GUIObject o in _liButtons) { Controls.Add(o); }

            GameManager.Pause();
            _open = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            int _openingFinished = 0;
            foreach(GUIObject o in Controls)
            {
                int val = 0;
                if (_open)
                {
                    if (o.Position.X < 0) { val = 16; }
                }
                if (_close)
                {
                    if (o.Position.X > -BTN_WIDTH) { val = -16; }
                }

                Vector2 temp = o.Position;
                temp.X += val;
                o.Position = temp;
                if (_open && o.Position.X == 0) { _openingFinished++; }
                if (_close && o.Position.X == -BTN_WIDTH) { GameManager.BackToMain(); }
            }
            if(_openingFinished == _liButtons.Count) { _open = false; }
            if(_infoScreen != null)
            {
                _infoScreen.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_infoScreen != null)
            {
                _infoScreen.Draw(spriteBatch);
            }
        }        

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnExitGame.Contains(mouse))
            {
                RiverHollow.PrepExit();
                rv = true;
            }
            if (_btnInventory.Contains(mouse))
            {
                _infoScreen = new InventoryScreen();
                rv = true;
            }
            if (_btnQuestLog.Contains(mouse))
            {
                _infoScreen = new QuestScreen();
                rv = true;
            }
            if (_btnParty.Contains(mouse))
            {
                _infoScreen = new PartyScreen();
                rv = true;
            }

            if (_infoScreen != null && _infoScreen.Contains(mouse))
            {
                _infoScreen.ProcessLeftButtonClick(mouse);
                rv = true;
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if(_infoScreen == null)
            {
                _close = true;
            }
            else if (!_infoScreen.Contains(mouse))
            {
                _close = true;
            }
            else if (_infoScreen.Contains(mouse))
            {
                _infoScreen.ProcessRightButtonClick(mouse);
            }
            return rv;
        }

        

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnExitGame.IsMouseHovering = _btnExitGame.Contains(mouse);
            _btnInventory.IsMouseHovering = _btnInventory.Contains(mouse);
            _btnParty.IsMouseHovering = _btnParty.Contains(mouse);
            if (_infoScreen != null)
            {
                _infoScreen.ProcessHover(mouse);
            }
            return rv;
        }

        public override bool IsGameMenuScreen() { return true; }
    }
}
