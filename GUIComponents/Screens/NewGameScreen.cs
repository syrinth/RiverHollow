using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
{
    class NewGameScreen : GUIScreen
    {
        enum SelectionEnum { None, Name, Manor };
        SelectionEnum _selection;
        const int BTN_HEIGHT = 32;
        const int BTN_WIDTH= 128;
        GUIWindow _window;
        GUIButton _btnOK;
        GUIButton _btnCancel;
        GUITextInputWindow _nameWindow;


        public NewGameScreen()
        {
            int height = RiverHollow.ScreenHeight - (2 * GUIWindow.BrownDialogEdge);
            int width = height;
            int startX = ((RiverHollow.ScreenWidth - width) / 2) - GUIWindow.BrownDialogEdge;

            _window = new GUIWindow(new Vector2(startX, GUIWindow.BrownDialogEdge), GUIWindow.BrownDialog, GUIWindow.BrownDialogEdge, width, height);
            Controls.Add(_window);

            _btnCancel = new GUIButton(new Vector2(_window.GetUsableRectangleVec().X+_window.Width-128, _window.GetUsableRectangleVec().Y + _window.Height- BTN_HEIGHT), new Rectangle(0, 128, 64, 32), BTN_WIDTH, BTN_HEIGHT, "Cancel", @"Textures\Dialog", true);
            Controls.Add(_btnCancel);

            _btnOK = new GUIButton("OK", BTN_WIDTH, BTN_HEIGHT);
            _btnOK.PlaceAndAlignObject(_btnCancel, SideEnum.Left, SideEnum.Top, 0);
            Controls.Add(_btnOK);

            _nameWindow = new GUITextInputWindow("Name");
            Controls.Add(_nameWindow);

            _selection = SelectionEnum.None;
        }

        public override void Update(GameTime gameTime)
        {
            if(_selection == SelectionEnum.Name)
            {
                _nameWindow.Update(gameTime);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                PlayerManager.Name = _nameWindow.GetText();
                RiverHollow.NewGame();
                rv = true;
            }
            if (_btnCancel.Contains(mouse))
            {
                GUIManager.SetScreen(new MainMenuScreen());
                rv = true;
            }

            if (_nameWindow.Contains(mouse))
            {
                _selection = SelectionEnum.Name;
            }
            else
            {
                _selection = SelectionEnum.None;
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnOK.IsMouseHovering = _btnOK.Contains(mouse);
            _btnCancel.IsMouseHovering = _btnCancel.Contains(mouse);
            return rv;
        }
    }
}
