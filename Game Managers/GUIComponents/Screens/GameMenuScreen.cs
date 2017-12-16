using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GameMenuScreen : GUIScreen
    {
        private GUIButton _btnExitGame;
        private GUIButton _btnInventory;
        private GUIButton _btnParty;
        //private GUITextSelectionWindow _btnReallyExit;
        private GUIScreen _infoScreen;

        public GameMenuScreen()
        {
            _btnInventory = new GUIButton(new Vector2(0, 100), new Rectangle(0, 128, 64, 32), 128, 64, "Inventory", @"Textures\Dialog", true);
            _btnParty = new GUIButton(new Vector2(0, 400), new Rectangle(0, 128, 64, 32), 128, 64, "Party", @"Textures\Dialog", true);
            _btnExitGame = new GUIButton(new Vector2(0, 700), new Rectangle(0, 128, 64, 32), 128, 64, "Exit", @"Textures\Dialog", true);
            Controls.Add(_btnExitGame);
            Controls.Add(_btnInventory);
            Controls.Add(_btnParty);
            RiverHollow.ChangeGameState(RiverHollow.GameState.Information);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if(_infoScreen != null)
            {
                _infoScreen.Update(gameTime);
            }
            //_btnNewGame.Update(gameTime);
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
                BackToMain();
            }
            else if (!_infoScreen.Contains(mouse))
            {
                BackToMain();
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
