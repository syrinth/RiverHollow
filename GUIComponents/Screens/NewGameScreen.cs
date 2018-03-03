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
        GUITextInputWindow _manorWindow;

        public NewGameScreen()
        {
            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIWindow.BrownWin.Edge;

            _window = new GUIWindow(new Vector2(startX, 0), GUIWindow.BrownWin, RiverHollow.ScreenHeight, RiverHollow.ScreenHeight);
            Controls.Add(_window);

            _btnCancel = new GUIButton("Cancel", BTN_WIDTH, BTN_HEIGHT);
            _btnCancel.AnchorToInnerSide(_window, SideEnum.BottomRight, 0);
            
            _btnOK = new GUIButton("OK", BTN_WIDTH, BTN_HEIGHT);
            _btnOK.AnchorAndAlignToObject(_btnCancel, SideEnum.Left, SideEnum.Top, 0);
            
            _manorWindow = new GUITextInputWindow("Manor Name:", SideEnum.Left);
            _manorWindow.AnchorToInnerSide(_window, SideEnum.TopRight);
            
            _nameWindow = new GUITextInputWindow("Character Name:", SideEnum.Left);
            _nameWindow.AnchorAndAlignToObject(_manorWindow, SideEnum.Bottom, SideEnum.Right );

            Controls.Add(_window);
            Controls.Add(_btnCancel);
            Controls.Add(_btnOK);
            Controls.Add(_manorWindow);
            Controls.Add(_nameWindow);

            _selection = SelectionEnum.None;
        }

        public override void Update(GameTime gameTime)
        {
            if(_selection == SelectionEnum.Name) { _nameWindow.Update(gameTime); }
            else if (_selection == SelectionEnum.Manor) { _manorWindow.Update(gameTime); }
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
                GUIManager.SetScreen(new IntroMenuScreen());
                rv = true;
            }

            if (_nameWindow.Contains(mouse)) { _selection = SelectionEnum.Name; }
            else if (_manorWindow.Contains(mouse)) { _selection = SelectionEnum.Manor; }
            else { _selection = SelectionEnum.None;}

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
