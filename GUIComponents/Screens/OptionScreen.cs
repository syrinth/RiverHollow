using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class OptionScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;
        GUIWindow _partyWindow;
        GUICheck _gAutoDisband;
        GUIButton _btnSave;

        public OptionScreen()
        {
            _partyWindow = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedWin, WIDTH, HEIGHT);
            Controls.Add(_partyWindow);

            _gAutoDisband = new GUICheck("Auto-Disband", GameManager.AutoDisband);
            _gAutoDisband.AnchorToInnerSide(_partyWindow, SideEnum.TopLeft);

            _btnSave = new GUIButton("Save", BtnSave);
            _btnSave.AnchorToInnerSide(_partyWindow, SideEnum.BottomRight);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void BtnSave()
        {
            GameManager.AutoDisband = _gAutoDisband.Checked();
            GameManager.BackToMain();
        }
    }
}
