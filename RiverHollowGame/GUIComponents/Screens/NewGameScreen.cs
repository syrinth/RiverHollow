using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox;
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox.CharacterDisplayBox;
using static RiverHollow.Items.Clothes;

namespace RiverHollow.GUIComponents.Screens
{
    class NewGameScreen : GUIScreen
    {
        static int _iHatIndex = 0;
        static List<int> _liHats;
        static int _iShirtIndex = 0;
        static List<int> _liShirts;
        bool _bCloseColorSelection;
        static int _iHairTypeIndex;
        int _iHairTypeCount = 3;
        static int _iCurrBodyType = 1;
        GUIWindow _window;
        GUIButton _btnOK;
        GUIButton _btnCancel;
        GUITextInputWindow _nameWindow;
        GUITextInputWindow _townWindow;

        GUICheck _gCheckSkipCutscene;
        GUIButton _gMuteButton;

        List<GUIObject> _liClassBoxes;
        ClassSelector _csbSelected;
        PlayerDisplayBox _playerDisplayBox;

        GUISwatch _btnHairColor;
        GUIButton _btnBodyType, _btnNextHairType, _btnNextHat, _btnNextShirt;
        GUIImage _gHair, _gHat, _gShirt;

        ColorSelectionBox _colorSelection;

        public NewGameScreen()
        {
            AssignClothes(ref _liHats, Util.FindParams(DataManager.Config[4]["ItemID"]));
            AssignClothes(ref _liShirts, Util.FindParams(DataManager.Config[5]["ItemID"]));

            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIWindow.Window_2.Edge;

            _window = new GUIWindow(GUIWindow.Window_2, RiverHollow.ScreenHeight, RiverHollow.ScreenHeight);
            _window.CenterOnScreen();
            AddControl(_window);

            _btnCancel = new GUIButton("Cancel", BtnCancel);
            _btnCancel.AnchorToInnerSide(_window, SideEnum.BottomRight, 0);
            
            _btnOK = new GUIButton("OK", BtnNewGame);
            _window.AddControl(_btnOK);
            _btnOK.AnchorAndAlignToObject(_btnCancel, SideEnum.Left, SideEnum.Top, 0);

            _gMuteButton = new GUIButton(new Rectangle(96, 80, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, ClickMuteButton);
            _window.AddControl(_gMuteButton);
            _gMuteButton.AnchorAndAlignToObject(_btnOK, SideEnum.Left, SideEnum.Top, 10);
            
            _townWindow = new GUITextInputWindow("Town Name:", SideEnum.Left);
            _townWindow.AnchorToInnerSide(_window, SideEnum.TopRight);
            //_manorWindow.SetText("Panda's");

            _nameWindow = new GUITextInputWindow("Character Name:", SideEnum.Left);
            _nameWindow.AnchorAndAlignToObject(_townWindow, SideEnum.Bottom, SideEnum.Right );
            //_nameWindow.SetText("Harold");
            _nameWindow.Activate();

            _liClassBoxes = new List<GUIObject>();
            for (int i = 0; i < DataManager.GetWorkerNum(); i++) {
                ClassSelector w = new ClassSelector(i, BtnAssignClass);
                w.Enable(false);
                _liClassBoxes.Add(w);
                _window.AddControl(w);
            }
            _csbSelected = (ClassSelector)_liClassBoxes[0];
            _csbSelected.Enable(true);

            _playerDisplayBox = new PlayerDisplayBox(false);
            _playerDisplayBox.AnchorToInnerSide(_window, SideEnum.TopLeft);

            _btnBodyType = new GUIButton(new Rectangle(256, 112, 16, 16), 32, 32, DataManager.DIALOGUE_TEXTURE, BtnNextBodyType);
            _btnBodyType.AnchorAndAlignToObject(_playerDisplayBox, SideEnum.Right, SideEnum.Bottom, 10);

            _btnHairColor = new GUISwatch(PlayerManager.World.HairColor, 16, 32, BtnChooseHairColor);
            _btnHairColor.AnchorAndAlignToObject(_playerDisplayBox, SideEnum.Bottom, SideEnum.Left);
            _gHair = new GUIImage(new Rectangle(192, 16, 16, 16), 32, 32, DataManager.DIALOGUE_TEXTURE);
            _gHair.AnchorAndAlignToObject(_btnHairColor, SideEnum.Right, SideEnum.Bottom, 10);
            
            _btnNextHairType = new GUIButton(new Rectangle(288, 96, 32, 32), 32, 32, DataManager.DIALOGUE_TEXTURE, BtnNextHairType);
            _btnNextHairType.AnchorAndAlignToObject(_gHair, SideEnum.Right, SideEnum.Bottom, 10);

            _gHat = new GUIImage( new Rectangle(160, 16, 16, 16), 32, 32, DataManager.DIALOGUE_TEXTURE);
            _gHat.AnchorAndAlignToObject(_gHair, SideEnum.Bottom, SideEnum.Left, 10);
            _btnNextHat = new GUIButton(new Rectangle(288, 96, 32, 32), 32, 32, DataManager.DIALOGUE_TEXTURE, BtnNextHat);
            _btnNextHat.AnchorAndAlignToObject(_gHat, SideEnum.Right, SideEnum.Bottom, 10);

            _gShirt = new GUIImage(new Rectangle(176, 16, 16, 16), 32, 32, DataManager.DIALOGUE_TEXTURE);
            _gShirt.AnchorAndAlignToObject(_gHat, SideEnum.Bottom, SideEnum.Left, 10);
            _btnNextShirt = new GUIButton(new Rectangle(288, 96, 32, 32), 32, 32, DataManager.DIALOGUE_TEXTURE, BtnNextShirt);
            _btnNextShirt.AnchorAndAlignToObject(_gShirt, SideEnum.Right, SideEnum.Bottom, 10);

            GUIObject.CreateSpacedRow(ref _liClassBoxes, _window.Height / 2, _window.Position().X, _window.Width, 20);

            _gCheckSkipCutscene = new GUICheck("Skip Intro");
            _gCheckSkipCutscene.SetChecked(false);
            _gCheckSkipCutscene.AnchorToInnerSide(_window, SideEnum.BottomLeft);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (_bCloseColorSelection)
            {
                _colorSelection.ParentWindow.RemoveControl(_colorSelection);
                _colorSelection = null;
                _bCloseColorSelection = false;
            }

            _btnOK.Enable(_nameWindow.GetText().Length > 0);
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
                SetSelection(_nameWindow);
            }
            else if (_townWindow.Contains(mouse)) {
                SetSelection(_townWindow);
            }
            else {
                SetSelection(null);
            }

            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            GUIManager.SetScreen(new IntroMenuScreen());
            GameManager.StopTakingInput();

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

        public void ClickMuteButton()
        {
            if (SoundManager.IsMuted) {
                SoundManager.UnmuteAllSound();
                 _gMuteButton.ChangeImage(new Rectangle(96, 80, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE);
            } else { 
                SoundManager.MuteAllSound();
                _gMuteButton.ChangeImage(new Rectangle(96, 96, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE);
            }
        }
        public void AssignClothes(ref List<int> clothesList, string[] clothingIDs)
        {
            clothesList = new List<int>();
            foreach (string s in clothingIDs)
            {
                clothesList.Add(int.Parse(s));
            }
        }
        public void SetSelection(GUITextInputWindow g)
        {
            if(g == _nameWindow)
            {
                _nameWindow.Activate();
                _townWindow.Activate(false);
            }
            else if (g == _townWindow)
            {
                _townWindow.Activate();
                _nameWindow.Activate(false);
            }
            else
            {
                _nameWindow.Activate(false);
                _townWindow.Activate(false);
            }
        }
        #region Button Logic
        public void BtnNewGame()
        {
            PlayerManager.World.SetScale();
            PlayerManager.SetClass(_csbSelected.ClassID);
            PlayerManager.World.AssignStartingGear();
            PlayerManager.SetName(_nameWindow.GetText());
            PlayerManager.SetTownName(_townWindow.GetText());

            RiverHollow.NewGame(DataManager.GetAdventurer(1), DataManager.GetAdventurer(2), !_gCheckSkipCutscene.Checked());
            GameManager.StopTakingInput();

        }
        public void BtnCancel()
        {
            GUIManager.SetScreen(new IntroMenuScreen());
            GameManager.StopTakingInput();
        }

        public void BtnNextBodyType()
        {
            _iCurrBodyType++;
            if(!DataManager.HasTexture(string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, _iCurrBodyType.ToString("00"))))
            {
                _iCurrBodyType = 1;
            }
            PlayerManager.World.SetBodyType(_iCurrBodyType);

            _playerDisplayBox.Configure();
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
            if (_iHairTypeIndex < _iHairTypeCount - 1) { _iHairTypeIndex++; }
            else { _iHairTypeIndex = 0; }

            PlayerManager.World.SetHairType(_iHairTypeIndex);
            
            _playerDisplayBox.Configure();
        }
        public void BtnNextHat()
        {
            if (_iHatIndex < _liHats.Count - 1) { _iHatIndex++; }
            else { _iHatIndex = 0; }

            SyncClothing((Clothes)DataManager.GetItem((_liHats[_iHatIndex])), ClothesEnum.Hat);    
            _playerDisplayBox.Configure();
        }
        public void BtnNextShirt()
        {
            if (_iShirtIndex < _liShirts.Count - 1) { _iShirtIndex++; }
            else { _iShirtIndex = 0; }

            SyncClothing((Clothes)DataManager.GetItem((_liShirts[_iShirtIndex])), ClothesEnum.Body);
            _playerDisplayBox.Configure();
        }

        private void SyncClothing(Clothes c, ClothesEnum e)
        {
            if (c != null) { PlayerManager.World.SetClothes(c); }
            else { PlayerManager.World.RemoveClothes(e); }
        }

        public void BtnAssignClass(ClassSelector obj)
        {
            if (obj != null && _csbSelected != obj)
            {
                _csbSelected.Enable(false);
                _csbSelected = obj;
                _csbSelected.Enable(true);
            }
        }
        public void CloseColorSelection()
        {
            _bCloseColorSelection = true;
        }
        #endregion

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
                _winData = GUIWindow.Window_1;

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
            public override bool ProcessHover(Point mouse)
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

        /// <summary>
        /// This class is a GUIImage that will diplay a class Icon and maintain an associated
        /// ClassID number to return to a query.
        /// </summary>
        public class ClassSelector : GUIImage
        {
            public int ClassID { get; } = -1;
            private ClickDelegate _delClassAction;
            public delegate void ClickDelegate(ClassSelector obj);

            public ClassSelector(int classID, ClickDelegate del) : base(new Rectangle(0 + (classID * TILE_SIZE), 112, TILE_SIZE, TILE_SIZE), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE)
            {
                ClassID = classID;
                _delClassAction = del;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                this.Alpha(Enabled ? 1.0f : 0.5f);
                base.Draw(spriteBatch);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse) && _delClassAction != null)
                {
                    _delClassAction(this);
                    rv = true;
                }
                return rv;
            }
        }
    }
}
