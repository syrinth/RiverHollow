using Microsoft.Xna.Framework;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class MainMenuScreen : GUIScreen
    {
        const int BTN_NUM = 3;
        const int BTN_HEIGHT = 64;
        const int BTN_WIDTH = 128;
        private GUIButton _btnNewGame;
        private GUIButton _btnLoadGame;
        private GUIButton _btnExit;

        public MainMenuScreen()
        {
            int btnPadding = 4;
            int btnStart = ((RiverHollow.ScreenHeight - (BTN_NUM * BTN_HEIGHT) - (btnPadding * BTN_NUM-1))/2) + BTN_HEIGHT/2;
            int yPos = btnStart;

            _btnNewGame = new GUIButton(new Vector2(RiverHollow.ScreenWidth/2, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "New Game", @"Textures\Dialog");
            yPos += BTN_HEIGHT + btnPadding;
            _btnLoadGame = new GUIButton(new Vector2(RiverHollow.ScreenWidth / 2, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Load Game", @"Textures\Dialog");
            yPos += BTN_HEIGHT + btnPadding;
            _btnExit = new GUIButton(new Vector2(RiverHollow.ScreenWidth / 2, yPos), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Exit Game", @"Textures\Dialog");
            Controls.Add(_btnNewGame);
            Controls.Add(_btnLoadGame);
            Controls.Add(_btnExit);

            GameManager.GoToInformation();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnNewGame.Contains(mouse))
            {
                RiverHollow.NewGame();
                rv = true;
            }
            if (_btnLoadGame.Contains(mouse))
            {
                RiverHollow.LoadGame();
                rv = true;
            }
            if (_btnExit.Contains(mouse))
            {
                RiverHollow.PrepExit();
                rv = true;
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnNewGame.IsMouseHovering = _btnNewGame.Contains(mouse);
            _btnLoadGame.IsMouseHovering = _btnLoadGame.Contains(mouse);
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            //_btnNewGame.Update(gameTime);
        }
    }
}
