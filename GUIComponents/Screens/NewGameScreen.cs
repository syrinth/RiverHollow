using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

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

        List<GUIObject> _liClasses;
        ClassSelectionBox _selectedClass;

        public NewGameScreen()
        {
            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIWindow.BrownWin.Edge;

            _window = new GUIWindow(new Vector2(startX, 0), GUIWindow.BrownWin, RiverHollow.ScreenHeight, RiverHollow.ScreenHeight);
            Controls.Add(_window);

            _btnCancel = new GUIButton("Cancel", BTN_WIDTH, BTN_HEIGHT);
            _btnCancel.AnchorToInnerSide(_window, SideEnum.BottomRight, 0);
            
            _btnOK = new GUIButton("OK", BTN_WIDTH, BTN_HEIGHT);
            _window.Controls.Add(_btnOK);
            _btnOK.AnchorAndAlignToObject(_btnCancel, SideEnum.Left, SideEnum.Top, 0);
            
            _manorWindow = new GUITextInputWindow("Manor Name:", SideEnum.Left);
            _manorWindow.AnchorToInnerSide(_window, SideEnum.TopRight);
            
            _nameWindow = new GUITextInputWindow("Character Name:", SideEnum.Left);
            _nameWindow.AnchorAndAlignToObject(_manorWindow, SideEnum.Bottom, SideEnum.Right );

            _liClasses = new List<GUIObject>();
            for (int i = 1; i <= 4; i++) {
                ClassSelectionBox w = new ClassSelectionBox(Vector2.Zero, ObjectManager.GetWorker(i));
                _liClasses.Add(w);
                _window.Controls.Add(w);
                Controls.Add(w);
            }
            _selectedClass = (ClassSelectionBox)_liClasses[0];
            _selectedClass.PlayAnimation("WalkDown");

            GUIObject.CreateSpacedRow(ref _liClasses, _window.Height / 2, _window.Position().X, _window.Width, 20);

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

            foreach (GUIObject o in _liClasses)
            {
                ((ClassSelectionBox)o).Update(gameTime);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                RiverHollow.NewGame();
                PlayerManager.SetClass(_selectedClass.ClassID);
                PlayerManager.SetName(_nameWindow.GetText());
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

            foreach(GUIObject o in _liClasses)
            {
                if (o.Contains(mouse))
                {
                    ClassSelectionBox csb = ((ClassSelectionBox)o);
                    if (_selectedClass != csb)
                    {
                        csb.PlayAnimation("WalkDown");
                        _selectedClass.PlayAnimation("Idle");
                        _selectedClass = csb;
                    }
                }
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

        public class ClassSelectionBox : GUIWindow
        {
            GUISprite _sprite;
            public GUISprite Sprite => _sprite;

            int _iClassID;
            public int ClassID => _iClassID;

            public ClassSelectionBox(Vector2 p, WorldAdventurer w)
            {
                _sprite = new GUISprite(w.Sprite);
                _iClassID = w.AdventurerID;
                Position(p);
                _winData = GUIWindow.RedWin;
                Width = 64;
                Height = 96;
            }

            public override void Update(GameTime gameTime)
            {
                _sprite.Update(gameTime);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                _sprite.Draw(spriteBatch);
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                if (_sprite != null) { _sprite.CenterOnWindow(this); }
            }

            public void PlayAnimation(string animation)
            {
                _sprite.PlayAnimation(animation);
            }
        }
    }
}
