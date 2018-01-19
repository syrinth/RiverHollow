using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GameMenuScreen : GUIScreen
    {
        const int BTN_NUM = 4;
        const int BTN_HEIGHT = 64;
        const int BTN_WIDTH = 128;
        private GUIButton _btnExitGame;
        private GUIButton _btnQuestLog;
        private GUIButton _btnInventory;
        private GUIButton _btnParty;
        //private GUITextSelectionWindow _btnReallyExit;
        private GUIScreen _infoScreen;

        bool _open = false;
        bool _close = false;

        public GameMenuScreen()
        {
            int btnPadding = (RiverHollow.ScreenHeight - (BTN_NUM * BTN_HEIGHT))/(BTN_NUM+1);
            int yPos = btnPadding;
            int xPos = -BTN_WIDTH;
            _btnInventory = new GUIButton(new Vector2(xPos, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Inventory", @"Textures\Dialog", true);
            yPos += BTN_HEIGHT + btnPadding;
            _btnParty = new GUIButton(new Vector2(xPos, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Party", @"Textures\Dialog", true);
            yPos += BTN_HEIGHT + btnPadding;
            _btnQuestLog = new GUIButton(new Vector2(xPos, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Quest Log", @"Textures\Dialog", true);
            yPos += BTN_HEIGHT + btnPadding;
            _btnExitGame = new GUIButton(new Vector2(xPos, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Exit", @"Textures\Dialog", true);
            Controls.Add(_btnExitGame);
            Controls.Add(_btnInventory);
            Controls.Add(_btnQuestLog);
            Controls.Add(_btnParty);

            RiverHollow.ChangeGameState(RiverHollow.GameState.Paused);
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
                if (_close && o.Position.X == -BTN_WIDTH) { BackToMain(); }
            }
            if(_openingFinished == BTN_NUM) { _open = false; }
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
                //GUIManager.SetScreen(GUIManager.Screens.Inventory);

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

        public void BackToMain()
        {
            GUIManager.SetScreen(GUIManager.Screens.HUD);
            RiverHollow.ChangeGameState(RiverHollow.GameState.Running);
            RiverHollow.ChangeMapState(RiverHollow.MapState.WorldMap);
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
    }
}
