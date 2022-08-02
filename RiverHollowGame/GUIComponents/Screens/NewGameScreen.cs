using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    class NewGameScreen : GUIScreen
    {
        private delegate void ChangeColorDelegate(Color c);
        private delegate void OpenColorPickerDelegate(OptionLabel obj, ChangeColorDelegate colorDel);

        DirectionEnum _eCurrentDirection = DirectionEnum.Down;

        static int _iHatIndex = 0;
        static int _iShirtIndex = 0;
        static List<int> _liShirts;
        static int _iHairTypeIndex;
        int _iHairTypeCount = 3;
        static int _iCurrBodyType = 1;
        GUIWindow _window;
        GUIButton _btnOK;
        GUITextInputWindow _nameWindow;
        GUITextInputWindow _townWindow;

        GUICheck _gCheckSkipCutscene;
        GUIButton _gMuteButton;

        List<GUIObject> _liClassBoxes;
        ClassSelector _csbSelected;
        ActorDisplayBox _displayBox;

        ColorPicker _gColorPicker;

        public NewGameScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.Info;

            //AssignClothes(ref _liHats, Util.FindParams(DataManager.Config[4]["ItemID"]));
            AssignClothes(ref _liShirts, Util.FindParams(DataManager.Config[5]["ItemID"]));

            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIWindow.Window_2.WidthEdges();

            GUIImage background = new GUIImage(new Rectangle(0, 0, 480, 270), DataManager.GUI_COMPONENTS + @"\Newgame_Background");
            AddControl(background);

            //Create the main window
            _window = new GUIWindow(GUIWindow.WoodenPanel, 208, 208);
            _window.SetScale(ScaledPixel);
            _window.CenterOnScreen();
            AddControl(_window);

            //Create Player Display Box
            _displayBox = new ActorDisplayBox(PlayerManager.PlayerActor, new GUIImage(new Rectangle(0, 144, 50, 49), DataManager.DIALOGUE_TEXTURE));
            _displayBox.Position(_window.Position());
            _displayBox.ScaledMoveBy(23, 17);
            _window.AddControl(_displayBox);

            //Create Turn Buttons
            GUIButton btnTurnLeft = new GUIButton(new Rectangle(102, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnLeft);
            btnTurnLeft.AnchorAndAlignToObject(_displayBox, SideEnum.Left, SideEnum.Bottom);
            btnTurnLeft.ScaledMoveBy(-1, -1);
            _window.AddControl(btnTurnLeft);

            GUIButton btnTurnRight= new GUIButton(new Rectangle(112, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnRight);
            btnTurnRight.AnchorAndAlignToObject(_displayBox, SideEnum.Right, SideEnum.Bottom);
            btnTurnRight.ScaledMoveBy(1, -1);
            _window.AddControl(btnTurnRight);

            //Create the Character Name Window
            _nameWindow = new GUITextInputWindow();
            _nameWindow.AnchorAndAlignToObject(_displayBox, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(3));
            _nameWindow.SetText("Syrinth");

            //Create the Body Picker
            OptionLabel bodyLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Body").GetFormattedText(), ChangeHairType, ChangeHairColor, AssignColorPicker, Color.White);
            bodyLabel.AnchorAndAlignToObject(_nameWindow, SideEnum.Bottom, SideEnum.Left);
            bodyLabel.ScaledMoveBy(5, 4);
            _window.AddControl(bodyLabel);

            OptionLabel hairLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Hair").GetFormattedText(), ChangeHairType, ChangeHairColor, AssignColorPicker, PlayerManager.PlayerActor.HairColor);
            hairLabel.AnchorAndAlignToObject(bodyLabel, SideEnum.Bottom, SideEnum.Left, ScaleIt(3));
            _window.AddControl(hairLabel);

            OptionLabel eyeLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Eyes").GetFormattedText(), null, ChangeEyeColor, AssignColorPicker, PlayerManager.PlayerActor.EyeColor);
            eyeLabel.AnchorAndAlignToObject(hairLabel, SideEnum.Bottom, SideEnum.Left, ScaleIt(3));
            _window.AddControl(eyeLabel);

            OptionLabel shirtLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Shirt").GetFormattedText(), ChangeHairType, null, null, Color.White);
            shirtLabel.AnchorAndAlignToObject(hairLabel, SideEnum.Bottom, SideEnum.Right, ScaleIt(27));
            _window.AddControl(shirtLabel);

            OptionLabel pantsLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Pants").GetFormattedText(), ChangeHairType, null, null, Color.White);
            pantsLabel.AnchorAndAlignToObject(shirtLabel, SideEnum.Bottom, SideEnum.Right, ScaleIt(3));
            _window.AddControl(pantsLabel);

            OptionLabel shoesLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Shoes").GetFormattedText(), ChangeHairType, null, null, Color.White);
            shoesLabel.AnchorAndAlignToObject(pantsLabel, SideEnum.Bottom, SideEnum.Right, ScaleIt(3));
            _window.AddControl(shoesLabel);

            //Create the Character Name Window
            GUIText townLabel = new GUIText(DataManager.GetGameTextEntry("Label_Town").GetFormattedText());
            townLabel.Position(_window.Position());
            townLabel.ScaledMoveBy(127, 5);
            _window.AddControl(townLabel);

            _townWindow = new GUITextInputWindow(12);
            _townWindow.AnchorAndAlignToObject(townLabel, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(3));
            _townWindow.SetText("River Hollow");

            GUIText classLabel = new GUIText(DataManager.GetGameTextEntry("Label_Class").GetFormattedText());
            classLabel.AnchorAndAlignToObject(_townWindow, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(4));
            _window.AddControl(classLabel);

            _liClassBoxes = new List<GUIObject>();
            for (int i = 0; i < DataManager.NumberOfClasses(); i++)
            {
                ClassSelector w = new ClassSelector(i, BtnAssignClass);
                w.Enable(false);

                if (i == 0) {
                    w.Position(classLabel.Position());
                    w.ScaledMoveBy(-24, 11);
                }
                else if (i % 4 == 0) { w.AnchorAndAlignToObject(_liClassBoxes[i - 4], SideEnum.Bottom, SideEnum.Left, ScaleIt(2)); }
                else { w.AnchorAndAlignToObject(_liClassBoxes[i - 1], SideEnum.Right, SideEnum.Top, ScaleIt(2)); }

                _liClassBoxes.Add(w);
                _window.AddControl(w);
            }
            _csbSelected = (ClassSelector)_liClassBoxes[0];
            _csbSelected.Enable(true);

            _gCheckSkipCutscene = new GUICheck("Skip Intro");
            _gCheckSkipCutscene.SetChecked(true);
            _gCheckSkipCutscene.AnchorAndAlignToObject(_window, SideEnum.Bottom, SideEnum.Left);
            _gCheckSkipCutscene.ScaledMoveBy(7, 0);
            AddControl(_gCheckSkipCutscene);

            _btnOK = new GUIButton("OK", BtnNewGame);
            _window.AddControl(_btnOK);
            _btnOK.AnchorToInnerSide(_window, SideEnum.BottomRight);

            _nameWindow.Activate();

            _gColorPicker = new ColorPicker();
            _gColorPicker.Show(false);
            AddControl(_gColorPicker);

            //_btnCancel = new GUIButton("Cancel", BtnCancel);
            //_btnCancel.AnchorToInnerSide(_window, SideEnum.BottomRight, 0);

            //_gMuteButton = new GUIButton(new Rectangle(96, 80, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, ClickMuteButton);
            //_window.AddControl(_gMuteButton);
            //_gMuteButton.AnchorAndAlignToObject(_btnOK, SideEnum.Left, SideEnum.Top, 10);

            //_gCheckPregnancy = new GUICheck("Pregnancy", false, BtnPregnancy);
            //_gCheckPregnancy.AnchorAndAlignToObject(_gShirt, SideEnum.Bottom, SideEnum.Left, 10);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //_btnOK.Enable(_nameWindow.GetText().Length > 0);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_gColorPicker.Show())
            {
                rv = _gColorPicker.ProcessLeftButtonClick(mouse);
            }

            if (!rv)
            {
                rv = _window.ProcessLeftButtonClick(mouse);
            }

            if (_nameWindow.Contains(mouse)) {
                SetSelection(_nameWindow);
            }
            else if (_townWindow.Contains(mouse))
            {
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

            if (_gColorPicker.Show())
            {
                _gColorPicker.ProcessHover(mouse);
            }
            //_btnOK.IsMouseHovering = _btnOK.Contains(mouse);
            //_btnCancel.IsMouseHovering = _btnCancel.Contains(mouse);
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

            PlayerManager.PlayerActor.SetClothes((Clothing)DataManager.GetItem(clothesList[0]));
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
            PlayerManager.PlayerActor.SetScale();
            PlayerManager.SetName(_nameWindow.GetText());
            PlayerManager.SetClass(_csbSelected.ClassID);
            PlayerManager.PlayerCombatant.AssignStartingGear();
            PlayerManager.SetTownName(_townWindow.GetText());

            RiverHollow.NewGame(!_gCheckSkipCutscene.Checked());
            GameManager.StopTakingInput();

        }
        public void BtnCancel()
        {
            GUIManager.SetScreen(new IntroMenuScreen());
            GameManager.StopTakingInput();
        }

        private void BtnNextBodyType()
        {
            _iCurrBodyType++;
            if(!DataManager.HasTexture(string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, _iCurrBodyType.ToString("00"))))
            {
                _iCurrBodyType = 1;
            }
            PlayerManager.PlayerActor.SetBodyType(_iCurrBodyType);

            _displayBox.AssignActor(PlayerManager.PlayerActor);
        }
        private void AssignColorPicker(OptionLabel obj, ChangeColorDelegate colorDel)
        {
            _gColorPicker.SetAction(colorDel, obj.OptionColor);
            _gColorPicker.AnchorAndAlignToObject(obj, SideEnum.Left, SideEnum.CenterY, ScaleIt(5), false);
            _gColorPicker.SetOptionLabel(obj);
            _gColorPicker.Show(true);
        }
        private void ChangeHairColor(Color c)
        {
            PlayerManager.PlayerActor.SetHairColor(c);
            _displayBox.AssignActor(PlayerManager.PlayerActor);
        }
        private void ChangeEyeColor(Color c)
        {
            PlayerManager.PlayerActor.SetEyeColor(c);
            _displayBox.AssignActor(PlayerManager.PlayerActor);
        }
        private int ChangeHairType(bool increase)
        {
            if (_iHairTypeIndex < _iHairTypeCount - 1) { _iHairTypeIndex++; }
            else { _iHairTypeIndex = 0; }

            PlayerManager.PlayerActor.SetHairType(_iHairTypeIndex);

            _displayBox.AssignActor(PlayerManager.PlayerActor);

            return _iHairTypeIndex;
        }
        private void BtnNextHat()
        {
           // if (_iHatIndex < _liHats.Count - 1) { _iHatIndex++; }
           // else { _iHatIndex = 0; }

            //SyncClothing((Clothing)DataManager.GetItem((_liHats[_iHatIndex])), ClothingEnum.Hat);
            _displayBox.AssignActor(PlayerManager.PlayerActor);
        }
        private void BtnNextShirt()
        {
            if (_iShirtIndex < _liShirts.Count - 1) { _iShirtIndex++; }
            else { _iShirtIndex = 0; }

            //SyncClothing((Clothing)DataManager.GetItem((_liShirts[_iShirtIndex])), ClothingEnum.Body);
            _displayBox.AssignActor(PlayerManager.PlayerActor);
        }
        private void BtnPregnancy()
        {
            //PlayerManager.PlayerActor.CanBecomePregnant = _gCheckPregnancy.Checked();
        }

        private DirectionEnum ChangeDirection(bool goLeft)
        {
            DirectionEnum rv = DirectionEnum.Down;

            switch (_eCurrentDirection)
            {
                case DirectionEnum.Down:
                    if (goLeft) { rv = DirectionEnum.Left; }
                    else { rv = DirectionEnum.Right; }
                    break;
                case DirectionEnum.Left:
                    if (goLeft) { rv = DirectionEnum.Up; }
                    else { rv = DirectionEnum.Down; }
                    break;
                case DirectionEnum.Up:
                    if (goLeft) { rv = DirectionEnum.Right; }
                    else { rv = DirectionEnum.Left; }
                    break;
                case DirectionEnum.Right:
                    if (goLeft) { rv = DirectionEnum.Down; }
                    else { rv = DirectionEnum.Up; }
                    break;
            }
            return rv;
        }
        public void BtnLeft()
        {
            _eCurrentDirection = ChangeDirection(true);
            _displayBox.PlayAnimation(VerbEnum.Idle, _eCurrentDirection);
        }

        public void BtnRight()
        {
            _eCurrentDirection = ChangeDirection(false);
            _displayBox.PlayAnimation(VerbEnum.Idle, _eCurrentDirection);
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
        #endregion

        private class OptionLabel : GUIObject
        {
            public Color OptionColor => _gColorPicker.ObjColor;
            GUIImage _gColorPicker;
            GUIText _gIndex;

            public delegate int ChangeIndexDelegate(bool increase);
            private ChangeIndexDelegate _delChangeIndex;
            private ChangeColorDelegate _delChangeColor;
            private OpenColorPickerDelegate _delOpenPicker;

            private GUIButton _btnDecrease;
            private GUIButton _btnIncrease;

            public OptionLabel(string label, ChangeIndexDelegate indexDelegate, ChangeColorDelegate colorDelegate, OpenColorPickerDelegate pickerDelegate, Color c)
            {
                _delChangeIndex = indexDelegate;
                _delChangeColor = colorDelegate;
                _delOpenPicker = pickerDelegate;

                if (_delChangeColor != null)
                {
                    _gColorPicker = new GUIImage(new Rectangle(145, 32, 7, 9), DataManager.DIALOGUE_TEXTURE);
                    _gColorPicker.SetColor(c);
                    _gColorPicker.ScaledMoveBy(0, 2);
                    AddControl(_gColorPicker);
                }

                GUIText bodyLabel = new GUIText(label);
                if (_gColorPicker != null)
                {
                    bodyLabel.AnchorAndAlignToObject(_gColorPicker, SideEnum.Right, SideEnum.Top, ScaleIt(3));
                }
                else
                {
                    bodyLabel.ScaledMoveBy(0, 2);
                }
                AddControl(bodyLabel);

                if (_delChangeIndex != null)
                {
                    _btnDecrease = new GUIButton(new Rectangle(102, 34, 10, 13), DataManager.DIALOGUE_TEXTURE);
                    _btnDecrease.Position(bodyLabel.Position());
                    _btnDecrease.ScaledMoveBy(26, -2);
                    AddControl(_btnDecrease);

                    _gIndex = new GUIText("00");
                    _gIndex.AnchorAndAlignToObject(_btnDecrease, SideEnum.Right, SideEnum.Top, ScaleIt(2));
                    _gIndex.ScaledMoveBy(0, 2);
                    AddControl(_gIndex);

                    _btnIncrease = new GUIButton(new Rectangle(112, 34, 10, 13), DataManager.DIALOGUE_TEXTURE);
                    _btnIncrease.AnchorAndAlignToObject(_gIndex, SideEnum.Right, SideEnum.CenterY, ScaleIt(2));
                    AddControl(_btnIncrease);

                    Width = _btnIncrease.Right;
                    Height = _btnIncrease.Height;
                }
                else
                {
                    Width = bodyLabel.Right;
                    Height = bodyLabel.Height;
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (_delChangeIndex != null) {
                    if (_btnIncrease.Contains(mouse))
                    {
                        rv = true;
                        _gIndex.SetText(_delChangeIndex(true).ToString("00"));
                    }
                    if (_btnDecrease.Contains(mouse))
                    {
                        rv = true;
                        _gIndex.SetText(_delChangeIndex(false).ToString("00"));
                    }
                }

                if(_delChangeColor != null)
                {
                    if (_gColorPicker.Contains(mouse))
                    {
                        _delOpenPicker(this, _delChangeColor);
                    }
                }

                return rv;
            }

            public void SetColorSwatch(Color c)
            {
                _gColorPicker.SetColor(c);
            }
        }

        private class ColorPicker : GUIObject
        {
            float _fHue;
            float _fSaturation;
            float _fValue;

            private ChangeColorDelegate _delChangeColor;

            GUIWindow _gWindow;
            Slider _gHue;
            Slider _gSaturation;
            Slider _gValue;

            OptionLabel _linkedLabel;
            public ColorPicker()
            {
                _gWindow = new GUIWindow(GUIWindow.Window_1, ScaleIt(48), ScaleIt(36));
                AddControl(_gWindow);

                _gHue = new Slider();
                _gHue.AnchorToInnerSide(_gWindow, SideEnum.Top);
                _gWindow.AddControl(_gHue);

                _gSaturation = new Slider();
                _gSaturation.AnchorAndAlignToObject(_gHue, SideEnum.Bottom, SideEnum.Left, ScaleIt(3));
                _gWindow.AddControl(_gSaturation);

                _gValue = new Slider();
                _gValue.AnchorAndAlignToObject(_gSaturation, SideEnum.Bottom, SideEnum.Left, ScaleIt(3));
                _gWindow.AddControl(_gValue);

                Width = _gWindow.Width;
                Height = _gWindow.Height;

                //ToDo:
                //ColorPicker maintains the individual HSV values.
                //ColorPicker passes the wholeHSV value to each slider, with -1 in place of that slider's color
                //Slider needs to know how valuable each ScaledPixel is, and then colorize itself along the -1 axis
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = _gHue.ProcessHover(mouse) || _gSaturation.ProcessHover(mouse) || _gValue.ProcessHover(mouse);

                if (rv)
                {
                    _fHue = _gHue.ChosenValue * 3;
                    _fSaturation = _gSaturation.ChosenValue / 4 * 3.3f / 100;
                    _fValue = _gValue.ChosenValue / 4 * 3.3f / 100;

                    Color c = ColorFromHSV(_fHue, _fSaturation, _fValue);
                    _delChangeColor(c);
                    _linkedLabel.SetColorSwatch(c);

                    Colorize();
                }
                else
                {
                    rv = _gWindow.ProcessHover(mouse);
                }

                return rv;
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv =  base.ProcessLeftButtonClick(mouse);

                if(!rv) { Show(false); }

                return rv;
            }

            private void Colorize()
            {
                _gHue.Colorize(12, -1, _fSaturation, _fValue);
                _gSaturation.Colorize(0.033f, _fHue, -1, _fValue);
                _gValue.Colorize(0.033f, _fHue, _fSaturation, -1);
            }

            public void SetAction(ChangeColorDelegate action, Color initialColor)
            {
                _delChangeColor = action;
                ColorToHsv(initialColor, out _fHue, out _fSaturation, out _fValue);

                _gHue.SetValue(_fHue/3);
                _gSaturation.SetValue(_fSaturation * 4 / 3.3f * 100);
                _gValue.SetValue(_fValue * 4 / 3.3f * 100);

                Colorize();
            }

            public void SetOptionLabel(OptionLabel linkedLabel)
            {
                _linkedLabel = linkedLabel;
            }

            private void ColorToHsv(Color xnaColor, out float hue, out float saturation, out float value)
            {
                float min, max, delta;
                min = Math.Min(Math.Min(xnaColor.R, xnaColor.G), xnaColor.B);
                max = Math.Max(Math.Max(xnaColor.R, xnaColor.G), xnaColor.B);

                value = max / 255;
                delta = max - min;
                if (max != 0)
                {
                    saturation = delta / max;

                    System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(xnaColor.A, xnaColor.R, xnaColor.G, xnaColor.B);
                    hue = drawingColor.GetHue();
                }
                else
                {
                    // r = g = b = 0       // s = 0, v is undefined
                    saturation = 0;
                    hue = -1;
                }

            }

            static Color ColorFromHSV(double hue, double saturation, double value)
            {
                int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
                double f = hue / 60 - Math.Floor(hue / 60);

                value = value * 255;
                int v = Convert.ToInt32(value);
                int p = Convert.ToInt32(value * (1 - saturation));
                int q = Convert.ToInt32(value * (1 - f * saturation));
                int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

                if (hi == 0)
                    return new Color(v, t, p, 255);
                else if (hi == 1)
                    return new Color(q, v, p, 255);
                else if (hi == 2)
                    return new Color(p, v, t, 255);
                else if (hi == 3)
                    return new Color(p, q, v, 255);
                else if (hi == 4)
                    return new Color(t, p, v, 255);
                else
                    return new Color(v, p, q, 255);
            }

            private class Slider : GUIObject
            {
                const int PIXEL_NUM = 30;
                GUIImage _gFrame;
                GUIImage _gToggle;
                
                readonly GUIImage[] _arrPixelArray;

                public float ChosenValue { get; private set; }

                public Slider()
                {
                    _arrPixelArray = new GUIImage[PIXEL_NUM];
                    for (int i = 0; i < PIXEL_NUM; i++)
                    {
                        _arrPixelArray[i] = new GUIImage(new Rectangle(146, 33, 1, 3), DataManager.DIALOGUE_TEXTURE);
                        if (i > 0) { _arrPixelArray[i].AnchorAndAlignToObject(_arrPixelArray[i - 1], SideEnum.Right, SideEnum.Top); }
                        else { _arrPixelArray[i].ScaledMoveBy(1, 1); }
                        AddControl(_arrPixelArray[i]);
                    }

                    _gFrame = new GUIImage(new Rectangle(144, 43, 32, 5), DataManager.DIALOGUE_TEXTURE);
                    AddControl(_gFrame);

                    _gToggle = new GUIImage(new Rectangle(155, 32, 3, 5), DataManager.DIALOGUE_TEXTURE);
                    AddControl(_gToggle);

                    Width = _gFrame.Width;
                    Height = _gFrame.Height;
                }

                public override bool ProcessHover(Point mouse)
                {
                    bool rv = false;
                    for(int i = 0; i < PIXEL_NUM; i++)
                    {
                        if (_arrPixelArray[i].Contains(mouse) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                        {
                            SetValue(mouse.X - _arrPixelArray[0].Left);
                            rv = true;
                        }
                    }

                    return rv;
                }

                public void SetValue(float value)
                {
                    ChosenValue =  value;

                    _gToggle.Position(_gFrame.Position());
                    _gToggle.MoveBy(value, 0);
                }

                public void Colorize(float imagePixelValue, float hue, float saturation, float value)
                {
                    float newHue = hue;
                    float newSaturation = saturation;
                    float newValue = value;

                    for(int i =0; i < PIXEL_NUM; i++)
                    {
                        if (hue == -1) { newHue = i * imagePixelValue; }
                        if (saturation == -1) { newSaturation = i * imagePixelValue; }
                        if (value == -1) { newValue = i * imagePixelValue; }
                        _arrPixelArray[i].SetColor(ColorPicker.ColorFromHSV(newHue, newSaturation, newValue));
                    }
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

            public ClassSelector(int classID, ClickDelegate del) : base(new Rectangle(0 + (classID * Constants.TILE_SIZE), 112, Constants.TILE_SIZE, Constants.TILE_SIZE), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE)
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
};
