using Microsoft.Xna.Framework;
using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    public class NamingScreen : GUIScreen
    {
        private GUITextInputWindow _window;
        WorldAdventurer _w;
        WorkerBuilding _b;

        private NamingScreen()
        {
            _window = new GUITextInputWindow();
            _window.SetupNaming();

            AddControl(_window);
        }

        public NamingScreen(WorldAdventurer w) : this()
        {
            _w = w;
        }

        public NamingScreen(WorkerBuilding b) : this()
        {
            _b = b;
            _window.AcceptSpace = true;
        }

        public override bool IsTextScreen() { return true; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_window != null && _window.Finished)
            {
                if (_w != null)
                {
                    _w.SetName(_window.EnteredText);
                }
                if (_b != null)
                {
                    _b.SetName(_window.EnteredText);
                }

                RiverHollow.ResetCamera();
                GameManager.Unpause();
                GameManager.Scry(false);
                GameManager.DontReadInput();
            }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            return false;
        }
    }
}
