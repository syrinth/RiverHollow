using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
{
    class NewGameScreen : GUIScreen
    {
        static int _iHatIndex = 0;
        static List<int> _liHats = new List<int> { -1, 402};
        static int _iShirtIndex = 0;
        static List<int> _liShirts = new List<int> { -1, 400, 401 };
        bool _bCloseColorSelection;
        static int _iHairTypeIndex;
        int _iHairTypeMax = GameContentManager.GetTexture(@"Textures\texPlayerHair").Height / 32;
        enum SelectionEnum { None, Name, Manor };
        SelectionEnum _selection;
        const int BTN_HEIGHT = 32;
        const int BTN_WIDTH= 128;
        GUIWindow _window;
        GUIButton _btnOK;
        GUIButton _btnCancel;
        GUITextInputWindow _nameWindow;
        GUITextInputWindow _manorWindow;

        GUICheck _gCheck;

        List<GUIObject> _liClasses;
        ClassSelectionBox _selectedClass;
        PlayerDisplayBox _playerDisplayBox;

        GUISwatch _btnHairColor;
        GUIButton _btnNextHairType, _btnNextHat, _btnNextShirt;
        GUIImage _gHair, _gHat, _gShirt;

        ColorSelectionBox _colorSelection;

        public NewGameScreen()
        {
            _selection = SelectionEnum.Name;

            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIWindow.BrownWin.Edge;

            _window = new GUIWindow(new Vector2(startX, 0), GUIWindow.BrownWin, RiverHollow.ScreenHeight, RiverHollow.ScreenHeight);
            Controls.Add(_window);

            _btnCancel = new GUIButton("Cancel", BTN_WIDTH, BTN_HEIGHT, BtnCancel);
            _btnCancel.AnchorToInnerSide(_window, SideEnum.BottomRight, 0);
            
            _btnOK = new GUIButton("OK", BTN_WIDTH, BTN_HEIGHT, BtnNewGame);
            _window.Controls.Add(_btnOK);
            _btnOK.AnchorAndAlignToObject(_btnCancel, SideEnum.Left, SideEnum.Top, 0);
            
            _manorWindow = new GUITextInputWindow("Manor Name:", SideEnum.Left);
            _manorWindow.AnchorToInnerSide(_window, SideEnum.TopRight);
            
            _nameWindow = new GUITextInputWindow("Character Name:", SideEnum.Left);
            _nameWindow.AnchorAndAlignToObject(_manorWindow, SideEnum.Bottom, SideEnum.Right );

            _liClasses = new List<GUIObject>();
            for (int i = 1; i <= ObjectManager.GetWorkerNum(); i++) {
                ClassSelectionBox w = new ClassSelectionBox(Vector2.Zero, ObjectManager.GetWorker(i), BtnAssignClass);
                _liClasses.Add(w);
                _window.Controls.Add(w);
                Controls.Add(w);
            }
            _selectedClass = (ClassSelectionBox)_liClasses[0];
            _selectedClass.PlayAnimation("WalkDown");

            _playerDisplayBox = new PlayerDisplayBox();
            _playerDisplayBox.AnchorToInnerSide(_window, SideEnum.TopLeft);

            _btnHairColor = new GUISwatch(PlayerManager.World.HairColor, 16, 32, BtnChooseHairColor);
            _btnHairColor.AnchorAndAlignToObject(_playerDisplayBox, SideEnum.Bottom, SideEnum.Left);
            _gHair = new GUIImage(Vector2.Zero, new Rectangle(192, 16, 16, 16), 32, 32, @"Textures\Dialog");
            _gHair.AnchorAndAlignToObject(_btnHairColor, SideEnum.Right, SideEnum.Bottom, 10);
            
            _btnNextHairType = new GUIButton(new Rectangle(288, 96, 32, 32), 32, 32, @"Textures\Dialog", BtnNextHairType);
            _btnNextHairType.AnchorAndAlignToObject(_gHair, SideEnum.Right, SideEnum.Bottom, 10);

            _gHat = new GUIImage(Vector2.Zero, new Rectangle(160, 16, 16, 16), 32, 32, @"Textures\Dialog");
            _gHat.AnchorAndAlignToObject(_gHair, SideEnum.Bottom, SideEnum.Left, 10);
            _btnNextHat = new GUIButton(new Rectangle(288, 96, 32, 32), 32, 32, @"Textures\Dialog", BtnNextHat);
            _btnNextHat.AnchorAndAlignToObject(_gHat, SideEnum.Right, SideEnum.Bottom, 10);

            _gShirt = new GUIImage(Vector2.Zero, new Rectangle(176, 16, 16, 16), 32, 32, @"Textures\Dialog");
            _gShirt.AnchorAndAlignToObject(_gHat, SideEnum.Bottom, SideEnum.Left, 10);
            _btnNextShirt = new GUIButton(new Rectangle(288, 96, 32, 32), 32, 32, @"Textures\Dialog", BtnNextShirt);
            _btnNextShirt.AnchorAndAlignToObject(_gShirt, SideEnum.Right, SideEnum.Bottom, 10);

            GUIObject.CreateSpacedRow(ref _liClasses, _window.Height / 2, _window.Position().X, _window.Width, 20);

            _gCheck = new GUICheck("Skip Intro");
            _gCheck.AnchorToInnerSide(_window, SideEnum.BottomLeft);
        }

        public override void Update(GameTime gameTime)
        {
            if (_bCloseColorSelection)
            {
                _colorSelection.ParentWindow.Controls.Remove(_colorSelection);
                _colorSelection = null;
                _bCloseColorSelection = false;
            }
            if(_selection == SelectionEnum.Name) { _nameWindow.Update(gameTime); }
            else if (_selection == SelectionEnum.Manor) { _manorWindow.Update(gameTime); }

            _btnOK.Enable(_nameWindow.GetText().Length > 0);

            foreach (GUIObject o in _liClasses)
            {
                ((ClassSelectionBox)o).Update(gameTime);
            }

            _playerDisplayBox.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_colorSelection != null) {
                rv = _colorSelection.ProcessLeftButtonClick(mouse);
                if (rv) { return rv; }
            }

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if(rv) { break; }
            }

            if (_nameWindow.Contains(mouse)) {
                _selection = SelectionEnum.Name;
                _manorWindow.HideCursor();
            }
            else if (_manorWindow.Contains(mouse)) {
                _selection = SelectionEnum.Manor;
                _nameWindow.HideCursor();
            }
            else {
                _selection = SelectionEnum.None;
                _nameWindow.HideCursor();
                _manorWindow.HideCursor();
            }

            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            GUIManager.SetScreen(new IntroMenuScreen());
            GameManager.DontReadInput();

            return true;
        }
        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnOK.IsMouseHovering = _btnOK.Contains(mouse);
            _btnCancel.IsMouseHovering = _btnCancel.Contains(mouse);
            if (_colorSelection != null) { _colorSelection.ProcessHover(mouse); }
            return rv;
        }

        #region Button Logic
        public void BtnNewGame()
        {
            PlayerManager.World.SetScale();
            RiverHollow.NewGame();
            PlayerManager.SetClass(_selectedClass.ClassID);
            PlayerManager.SetName(_nameWindow.GetText());
            GameManager.DontReadInput();
        }
        public void BtnCancel()
        {
            GUIManager.SetScreen(new IntroMenuScreen());
            GameManager.DontReadInput();
        }
        public void BtnChooseHairColor()
        {
            if (_colorSelection == null)
            {
                _colorSelection = new ColorSelectionBox(PlayerColorEnum.Hair, _btnHairColor, CloseColorSelection);
                _colorSelection.AnchorAndAlignToObject(_btnHairColor, SideEnum.Right, SideEnum.Top);
            }
        }
        public void BtnNextHairType()
        {
            if (_iHairTypeIndex < _iHairTypeMax - 1) { _iHairTypeIndex++; }
            else { _iHairTypeIndex = 0; }

            _playerDisplayBox.Sync();
        }
        public void BtnNextHat()
        {
            if (_iHatIndex < _liHats.Count - 1) { _iHatIndex++; }
            else { _iHatIndex = 0; }
            
            _playerDisplayBox.Sync();
        }
        public void BtnNextShirt()
        {
            Color currHair = PlayerManager.World.HairColor;
            if (_iShirtIndex < _liShirts.Count - 1) { _iShirtIndex++; }
            else { _iShirtIndex = 0; }

            _playerDisplayBox.Sync();
        }
        public void BtnAssignClass(ClassSelectionBox o)
        {
                ClassSelectionBox csb = ((ClassSelectionBox)o);
                if (_selectedClass != csb)
                {
                    csb.PlayAnimation("WalkDown");
                    _selectedClass.PlayAnimation("Idle");
                    _selectedClass = csb;
                }
        }
        public void CloseColorSelection()
        {
            _bCloseColorSelection = true;
        }
        #endregion

        public class PlayerDisplayBox : GUIWindow
        {
            GUISprite _bodySprite;
            public GUISprite BodySprite => _bodySprite;
            GUISprite _eyeSprite;
            public GUISprite EyeSprite => _eyeSprite;
            GUISprite _hairSprite;
            public GUISprite HairSprite => _hairSprite;
            GUISprite _armSprite;
            public GUISprite ArmSprite => _armSprite;
            GUISprite _hatSprite;
            public GUISprite HatSprite => _hatSprite;
            GUISprite _shirtSprite;
            public GUISprite ShirtSprite => _shirtSprite;

            public PlayerDisplayBox()
            {
                Configure();

                _winData = GUIWindow.RedWin;
                
                PositionSprites();
            }

            public override void Update(GameTime gameTime)
            {
                _bodySprite.Update(gameTime);
                _eyeSprite.Update(gameTime);
                _hairSprite.Update(gameTime);
                _armSprite.Update(gameTime);
                if (_hatSprite != null) { _hatSprite.Update(gameTime); }
                if (_shirtSprite != null) { _shirtSprite.Update(gameTime); }
            }

            private void Configure()
            {
                _bodySprite = new GUISprite(PlayerManager.World.BodySprite);
                _eyeSprite = new GUISprite(PlayerManager.World.EyeSprite);
                _hairSprite = new GUISprite(PlayerManager.World.HairSprite);
                _armSprite = new GUISprite(PlayerManager.World.ArmSprite);
                if (PlayerManager.World.Hat != null) { _hatSprite = new GUISprite(PlayerManager.World.Hat.Sprite); }
                if (PlayerManager.World.Chest != null) { _shirtSprite = new GUISprite(PlayerManager.World.Chest.Sprite); }

                _bodySprite.SetScale((int)GameManager.Scale);
                _eyeSprite.SetScale((int)GameManager.Scale);
                _hairSprite.SetScale((int)GameManager.Scale);
                _armSprite.SetScale((int)GameManager.Scale);
                if (_hatSprite != null) { _hatSprite.SetScale((int)GameManager.Scale); }
                if (_shirtSprite != null) { _shirtSprite.SetScale((int)GameManager.Scale); }
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                PositionSprites();
            }

            public void PlayAnimation(string animation)
            {
                _bodySprite.PlayAnimation(animation);
                _eyeSprite.PlayAnimation(animation);
                _hairSprite.PlayAnimation(animation);
                _armSprite.PlayAnimation(animation);
            }

            public void PositionSprites()
            {
                if (_bodySprite != null)
                {
                    _bodySprite.CenterOnWindow(this);
                    _bodySprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    Width = _bodySprite.Width + _bodySprite.Width / 3;
                    Height = _bodySprite.Height + 2 * _bodySprite.Height / 4;

                }
                PositionSprite(_eyeSprite);
                PositionSprite(_hairSprite);
                PositionSprite(_armSprite);
                PositionSprite(_eyeSprite);
                PositionSprite(_hatSprite);
                PositionSprite(_shirtSprite);

            }
            private void PositionSprite(GUISprite sprite)
            {
                if (sprite != null)
                {
                    sprite.CenterOnWindow(this);
                    sprite.AnchorToInnerSide(this, SideEnum.Bottom);
                }
            }

            public void Sync()
            {
                PlayerManager.World.SetHairType(_iHairTypeIndex);
                PlayerManager.World.SetClothes((Clothes)ObjectManager.GetItem(_liHats[_iHatIndex]));
                PlayerManager.World.SetClothes((Clothes)ObjectManager.GetItem(_liShirts[_iShirtIndex]));

                Configure();
                PositionSprites();
            }
        }

        public class ClassSelectionBox : GUIWindow
        {
            GUISprite _sprite;
            public GUISprite Sprite => _sprite;

            int _iClassID;
            public int ClassID => _iClassID;

            public delegate void ClickDelegate(ClassSelectionBox o);
            private ClickDelegate _delAction;

            public ClassSelectionBox(Vector2 p, WorldAdventurer w, ClickDelegate del)
            {
                _sprite = new GUISprite(w.BodySprite);
                _sprite.SetScale((int)GameManager.Scale);
                _iClassID = w.AdventurerID;
                Position(p);
                _winData = GUIWindow.GreyWin;
                Width = _sprite.Width + _sprite.Width / 4;
                Height = _sprite.Height + (_winData.Edge * 2);
                _sprite.CenterOnWindow(this);
                _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                _delAction = del;
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
                if (_sprite != null) {
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);
                }
            }

            public void PlayAnimation(string animation)
            {
                _sprite.PlayAnimation(animation);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse) && _delAction != null)
                {
                    _delAction(this);
                    rv = true;
                }
                return rv;
            }
        }

        public class ColorSelectionBox : GUIWindow
        {
            const int MAX_COLUMN = 100;
            PlayerColorEnum _target;
            List<Color> colorList = new List<Color> { Color.Black, Color.Blue, Color.Yellow, Color.White, Color.Purple, Color.Orange, Color.Orchid, Color.Pink, Color.PeachPuff, Color.Green, Color.Gray };
            List<GUISwatch> _liSwatches;

            public delegate void CloseDelegate();
            private CloseDelegate _closeAction;

            GUISwatch _main;

            public ColorSelectionBox(PlayerColorEnum target, GUISwatch mainSwatch, CloseDelegate closeIt)
            {
                _main = mainSwatch;
                _closeAction = closeIt;
                _liSwatches = new List<GUISwatch>();
                _winData = GUIWindow.RedWin;

                _target = target;
                Width = 10;
                Height = 10;
                int id = 0;

                for (int i = 0; i< colorList.Count; i++)
                {
                    Color temp = colorList[i];
                
                //for(int r = 0; r < 25; r++) {
                //    for (int g = 0; g < 25; g++)
                //    {
                //        for (int b = 0; b < 25; b++)
                //        {
                            GUISwatch newSwatch = new GUISwatch(temp);
                            _liSwatches.Add(newSwatch);

                            if (id == 0) { newSwatch.AnchorToInnerSide(this, SideEnum.TopLeft); }
                            else if (id % MAX_COLUMN == 0) { newSwatch.AnchorAndAlignToObject(_liSwatches[id - MAX_COLUMN], SideEnum.Bottom, SideEnum.Left); }
                            else { newSwatch.AnchorAndAlignToObject(_liSwatches[id - 1], SideEnum.Right, SideEnum.Top); }
                            id++;
                    //    }
                    //}
                }

                Resize();
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach(GUISwatch g in _liSwatches)
                {
                    if (g.Contains(mouse))
                    {
                        SetPlayerColor(g.SwatchColor);
                        _closeAction();
                        rv = true;
                        
                        break;
                    }
                }

                _closeAction();
                return rv;
            }
            public bool ProcessHover(Point mouse)
            {
                bool rv = false;

                foreach (GUISwatch g in _liSwatches)
                {
                    if (g.Contains(mouse))
                    {
                        SetPlayerColor(g.SwatchColor);
                        rv = true;
                        break;
                    }
                }

                return rv;
            }

            private void SetPlayerColor(Color c)
            {
                _main.SetColor(c);
                switch (_target)
                {
                    case PlayerColorEnum.Hair:
                        PlayerManager.World.SetHairColor(c);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
