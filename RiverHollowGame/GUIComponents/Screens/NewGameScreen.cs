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

        PlayerDisplayBox _displayBox;

        ColorPicker _gColorPicker;

        public NewGameScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.Info;

            //AssignClothes(ref _liHats, Util.FindParams(DataManager.Config[4]["ItemID"]));
            AssignClothes(ref _liShirts, Util.FindParams(DataManager.Config[5]["ItemID"]));

            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIUtils.WINDOW_DARKBLUE.WidthEdges();

            new GUIImage(DataManager.GetTexture(DataManager.GUI_COMPONENTS + @"\Newgame_Background"));

            //Create the main window
            _window = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL, 208, 208);
            _window.SetScale(ScaledPixel);
            _window.CenterOnScreen();

            //Create Player Display Box
            _displayBox = new PlayerDisplayBox(GUIUtils.NEW_DISPLAY, new Point(17, 14));
            _displayBox.PositionAndMove(_window, 23, 17);

            //Create Turn Buttons
            GUIButton btnTurnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            btnTurnLeft.AnchorAndAlignThenMove(_displayBox, SideEnum.Left, SideEnum.Bottom, -1, -1);

            GUIButton btnTurnRight= new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            btnTurnRight.AnchorAndAlignThenMove(_displayBox, SideEnum.Right, SideEnum.Bottom, 1, -1);

            //Create the Character Name Window
            _nameWindow = new GUITextInputWindow();
            _nameWindow.AnchorAndAlignWithSpacing(_displayBox, SideEnum.Bottom, SideEnum.CenterX, 3);

#if DEBUG
            _nameWindow.SetText("Syrinth");

            //Create the Body Picker
            OptionLabel bodyLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Body").GetFormattedText(), ChangeHairType, ChangeHairColor, AssignColorPicker, Color.White);
            bodyLabel.AnchorAndAlignThenMove(_nameWindow, SideEnum.Bottom, SideEnum.Left, 5, 4);
            bodyLabel.Show(false);

            OptionLabel hairLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Hair").GetFormattedText(), ChangeHairType, ChangeHairColor, AssignColorPicker, PlayerManager.PlayerActor.HairColor);
            hairLabel.AnchorAndAlignWithSpacing(bodyLabel, SideEnum.Bottom, SideEnum.Left, 3);

            OptionLabel eyeLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Eyes").GetFormattedText(), null, ChangeEyeColor, AssignColorPicker, PlayerManager.PlayerActor.EyeColor);
            eyeLabel.AnchorAndAlignWithSpacing(hairLabel, SideEnum.Bottom, SideEnum.Left, 3);

            OptionLabel shirtLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Shirt").GetFormattedText(), ChangeHairType, null, null, Color.White);
            shirtLabel.AnchorAndAlignWithSpacing(hairLabel, SideEnum.Bottom, SideEnum.Right, 27);
            shirtLabel.Show(false);

            OptionLabel pantsLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Pants").GetFormattedText(), ChangeHairType, null, null, Color.White);
            pantsLabel.AnchorAndAlignWithSpacing(shirtLabel, SideEnum.Bottom, SideEnum.Right, 3);
            pantsLabel.Show(false);

            OptionLabel shoesLabel = new OptionLabel(DataManager.GetGameTextEntry("Label_Shoes").GetFormattedText(), ChangeHairType, null, null, Color.White);
            shoesLabel.AnchorAndAlignWithSpacing(pantsLabel, SideEnum.Bottom, SideEnum.Right, 3);
            shoesLabel.Show(false);
#endif
            //Create the Character Name Window
            GUIText townLabel = new GUIText(DataManager.GetGameTextEntry("Label_Town").GetFormattedText());
            townLabel.PositionAndMove(_window, 127, 5);

            _townWindow = new GUITextInputWindow(12);
            _townWindow.AnchorAndAlignWithSpacing(townLabel, SideEnum.Bottom, SideEnum.CenterX, 3);
            _townWindow.SetText("River Hollow");

            _gCheckSkipCutscene = new GUICheck("Skip Intro");
            _gCheckSkipCutscene.AnchorAndAlign(_window, SideEnum.Bottom, SideEnum.Left);
            _gCheckSkipCutscene.ScaledMoveBy(7, 0);
#if DEBUG
            _gCheckSkipCutscene.SetChecked(true);
#endif

            _btnOK = new GUIButton("OK", BtnNewGame);
            _window.AddControl(_btnOK);
            _btnOK.AnchorToInnerSide(_window, SideEnum.BottomRight);

            _nameWindow.Activate();

            _gColorPicker = new ColorPicker();
            _window.AddControl(_gColorPicker);
            _gColorPicker.Show(false);

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

            _btnOK.Enable(!string.IsNullOrEmpty(_townWindow.GetText()) && !string.IsNullOrEmpty(_nameWindow.GetText()));
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = GUIUtils.ProcessLeftMouseButton(mouse, _gCheckSkipCutscene, _gColorPicker);

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

        public void ClickMuteButton()
        {
            if (SoundManager.IsMuted) {
                SoundManager.UnmuteAllSound();
                 //_gMuteButton.ChangeImage(new Rectangle(96, 80, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE);
            } else { 
                SoundManager.MuteAllSound();
                //_gMuteButton.ChangeImage(new Rectangle(96, 96, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE);
            }
        }
        public void AssignClothes(ref List<int> clothesList, string[] clothingIDs)
        {
            clothesList = new List<int>();
            foreach (string s in clothingIDs)
            {
                clothesList.Add(int.Parse(s));
            }

            PlayerManager.PlayerActor.PlayerGear[1, 0] = (Clothing)DataManager.GetItem(clothesList[0]);
            PlayerManager.PlayerActor.SetClothing((Clothing)DataManager.GetItem(clothesList[0]));
            PlayerManager.PlayerActor.PlayerGear[2, 0] = (Clothing)DataManager.GetItem(clothesList[1]);
            PlayerManager.PlayerActor.SetClothing((Clothing)DataManager.GetItem(clothesList[1]));
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
            TownManager.SetTownName(_townWindow.GetText());

            RiverHollow.Instance.NewGame(!_gCheckSkipCutscene.Checked());
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

            _displayBox.SyncSprites();
        }
        private void AssignColorPicker(OptionLabel obj, ChangeColorDelegate colorDel)
        {
            _gColorPicker.SetAction(colorDel, obj.OptionColor);
            _gColorPicker.AnchorAndAlignWithSpacing(obj, SideEnum.Left, SideEnum.CenterY, 5);
            _gColorPicker.SetOptionLabel(obj);
            _gColorPicker.Show(true);
        }
        private void ChangeHairColor(Color c)
        {
            PlayerManager.PlayerActor.SetHairColor(c);
            _displayBox.SyncSprites();
        }
        private void ChangeEyeColor(Color c)
        {
            PlayerManager.PlayerActor.SetEyeColor(c);
            _displayBox.SyncSprites();
        }
        private int ChangeHairType(bool increase)
        {
            if (_iHairTypeIndex < _iHairTypeCount - 1) { _iHairTypeIndex++; }
            else { _iHairTypeIndex = 0; }

            PlayerManager.PlayerActor.SetHairType(_iHairTypeIndex);

            _displayBox.SyncSprites();

            return _iHairTypeIndex;
        }
        private void BtnNextHat()
        {
           // if (_iHatIndex < _liHats.Count - 1) { _iHatIndex++; }
           // else { _iHatIndex = 0; }

            //SyncClothing((Clothing)DataManager.GetItem((_liHats[_iHatIndex])), ClothingEnum.Hat);
            _displayBox.SyncSprites();
        }
        private void BtnNextShirt()
        {
            if (_iShirtIndex < _liShirts.Count - 1) { _iShirtIndex++; }
            else { _iShirtIndex = 0; }

            //SyncClothing((Clothing)DataManager.GetItem((_liShirts[_iShirtIndex])), ClothingEnum.Body);
            _displayBox.SyncSprites();
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
#endregion

        private class OptionLabel : GUIObject
        {
            public Color OptionColor => _gColorPicker.ObjColor;

            readonly GUIImage _gColorPicker;
            readonly GUIText _gIndex;

            public delegate int ChangeIndexDelegate(bool increase);
            readonly ChangeIndexDelegate _delChangeIndex;
            readonly ChangeColorDelegate _delChangeColor;
            readonly OpenColorPickerDelegate _delOpenPicker;

            readonly GUIImage _gDecrease;
            readonly GUIImage _gIncrease;

            public OptionLabel(string label, ChangeIndexDelegate indexDelegate, ChangeColorDelegate colorDelegate, OpenColorPickerDelegate pickerDelegate, Color c)
            {
                _delChangeIndex = indexDelegate;
                _delChangeColor = colorDelegate;
                _delOpenPicker = pickerDelegate;

                if (_delChangeColor != null)
                {
                    _gColorPicker = new GUIImage(GUIUtils.HUD_COLOR_PICK);
                    _gColorPicker.SetColor(c);
                    _gColorPicker.ScaledMoveBy(0, 2);
                    AddControl(_gColorPicker);
                }

                GUIText bodyLabel = new GUIText(label);
                if (_gColorPicker != null)
                {
                    bodyLabel.AnchorAndAlignWithSpacing(_gColorPicker, SideEnum.Right, SideEnum.Top, 3);
                }
                else
                {
                    bodyLabel.ScaledMoveBy(0, 2);
                }
                AddControl(bodyLabel);

                if (_delChangeIndex != null)
                {
                    _gDecrease = new GUIImage(GUIUtils.BTN_LEFT_SMALL);
                    _gDecrease.PositionAndMove(bodyLabel, 26, -2);
                    AddControl(_gDecrease);

                    _gIndex = new GUIText("00");
                    _gIndex.AnchorAndAlignThenMove(_gDecrease, SideEnum.Right, SideEnum.Top, 2, 2);
                    AddControl(_gIndex);

                    _gIncrease = new GUIImage(GUIUtils.BTN_RIGHT_SMALL);
                    _gIncrease.AnchorAndAlignWithSpacing(_gIndex, SideEnum.Right, SideEnum.CenterY, 2);
                    AddControl(_gIncrease);

                    Width = _gIncrease.Right;
                    Height = _gIncrease.Height;
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
                    if (_gIncrease.Contains(mouse))
                    {
                        rv = true;
                        _gIndex.SetText(_delChangeIndex(true).ToString("00"));
                    }
                    if (_gDecrease.Contains(mouse))
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

        private class ColorPicker : GUIWindow
        {
            float _fHue;
            float _fSaturation;
            float _fValue;

            private ChangeColorDelegate _delChangeColor;

            Slider _gHue;
            Slider _gSaturation;
            Slider _gValue;

            OptionLabel _linkedLabel;
            public ColorPicker() : base(GUIUtils.WINDOW_BROWN, ScaleIt(48), ScaleIt(36))
            {
                _gHue = new Slider();
                _gHue.AnchorToInnerSide(this, SideEnum.Top);

                _gSaturation = new Slider();
                _gSaturation.AnchorAndAlignWithSpacing(_gHue, SideEnum.Bottom, SideEnum.Left, 3);

                _gValue = new Slider();
                _gValue.AnchorAndAlignWithSpacing(_gSaturation, SideEnum.Bottom, SideEnum.Left, 3);

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
                    rv = base.ProcessHover(mouse);
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
                        _arrPixelArray[i] = new GUIImage(GUIUtils.HUD_COLOR_PATCH);
                        if (i > 0) { _arrPixelArray[i].AnchorAndAlign(_arrPixelArray[i - 1], SideEnum.Right, SideEnum.Top); }
                        else { _arrPixelArray[i].ScaledMoveBy(1, 1); }
                        AddControl(_arrPixelArray[i]);
                    }

                    _gFrame = new GUIImage(GUIUtils.HUD_COLOR_FRAME);
                    AddControl(_gFrame);

                    _gToggle = new GUIImage(GUIUtils.HUD_COLOR_TOGGLE);
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
                    _gToggle.MoveBy((int)value, 0);
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
    }
};
