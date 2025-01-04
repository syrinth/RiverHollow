using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Utilities.Enums;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Input;

namespace RiverHollow.GUIComponents.MainObjects
{
    internal class HUDCosmetics : GUIMainObject
    {
        private delegate void ChangeColorDelegate(Color c);
        private delegate void OpenColorPickerDelegate(ChangeColorDelegate colorDel);

        DirectionEnum _eCurrentDirection = DirectionEnum.Down;
        CosmeticSlotEnum _eCurrentCosmeticSlot = CosmeticSlotEnum.Hair;


        readonly List<GUICosmeticBox> _liAppliedCosmetics;
        readonly List<GUICosmeticBox> _liAllCosmetics;

        readonly PlayerDisplayBox _displayBox;

        readonly GUIButton _gColorSwatch;
        readonly ColorPicker _gColorPicker;

        readonly GUIWindow _window;

        readonly GUIImage _gSelected;
        readonly GUITextInputWindow _nameWindow;
        readonly GUITextInputWindow _townWindow;


        public HUDCosmetics()
        {
            _liAllCosmetics = new List<GUICosmeticBox>();

            //Create the main window
            _window = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL, 208, 208);
            _window.SetScale(GameManager.ScaledPixel);
            AddControl(_window);

            //Create Player Display Box
            _displayBox = new PlayerDisplayBox(GUIUtils.NEW_DISPLAY, new Point(1, 6));
            _displayBox.AnchorToInnerSide(_window, SideEnum.Top, 5);

            //Create Turn Buttons
            GUIButton btnTurnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            btnTurnLeft.AnchorAndAlignWithSpacing(_displayBox, SideEnum.Left, SideEnum.CenterY, 2);

            GUIButton btnTurnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            btnTurnRight.AnchorAndAlignWithSpacing(_displayBox, SideEnum.Right, SideEnum.CenterY, 2);

            //Create the Character Name Window
            _nameWindow = new GUITextInputWindow();
            _nameWindow.AnchorAndAlignWithSpacing(_displayBox, SideEnum.Bottom, SideEnum.CenterX, 2);

            //Create the Randomize Button
            GUIButton btnRandomize = new GUIButton(GUIUtils.BTN_RANDOMIZE, BtnRandomize);
            btnRandomize.AnchorAndAlignWithSpacing(_nameWindow, SideEnum.Top, SideEnum.Right, 1);

            _gSelected = new GUIImage(GUIUtils.SELECT_CORNER);
            _gSelected.Show(false);

            //Create Player Applied Cosmetic Boxes
            int index = 0;
            _liAppliedCosmetics = new List<GUICosmeticBox>();
            foreach (CosmeticSlotEnum e in Enum.GetValues(typeof(CosmeticSlotEnum)))
            {
                var box = new GUICosmeticBox(e, PlayerManager.PlayerActor.GetCosmetic(e), OpenCosmetics, true);
                AddControl(box);

                if (_liAppliedCosmetics.Count == 0) { box.PositionAndMove(_window, 16, 10); }
                else if (_liAppliedCosmetics.Count % 2 != 0) { box.AnchorAndAlignWithSpacing(_liAppliedCosmetics[index - 1], SideEnum.Right, SideEnum.Top, 4); }
                else { box.AnchorAndAlignWithSpacing(_liAppliedCosmetics[index - 2], SideEnum.Bottom, SideEnum.Left, 3); }
                _liAppliedCosmetics.Add(box);

                index++;
            }

            string defaultNameValue;
#if DEBUG
            defaultNameValue = "Syrinth";
#endif
            string nameStr = string.IsNullOrEmpty(PlayerManager.Name) ? defaultNameValue : PlayerManager.PlayerActor.Name;
            _nameWindow.SetText(nameStr);

            _townWindow = new GUITextInputWindow(12);
            _townWindow.PositionAndMove(_window, new Point(5, 181));
            _townWindow.SetText("River Hollow");

            _gColorSwatch = new GUIButton(GUIUtils.HUD_COLOR_PICK, AssignColorPicker);
            _gColorSwatch.Show(false);
            _gColorPicker = new ColorPicker(_gColorSwatch);
            _gColorPicker.Show(false);

            _window.AddControls(_gColorSwatch, _gColorPicker);

            Width = _window.Width;
            Height = _window.Height;

            CenterOnScreen();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            if (_gColorPicker.Visible && !_gColorPicker.Contains(mouse))
            {
                _gColorPicker.Show(false);
            }

            bool rv = base.ProcessLeftButtonClick(mouse);

            if (_nameWindow.Contains(mouse))
            {
                SetSelection(_nameWindow);
            }
            else if (_townWindow.Contains(mouse))
            {
                SetSelection(_townWindow);
            }
            else
            {
                SetSelection(null);
            }

            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            if (_gColorPicker.Visible)
            {
                _gColorPicker.Show(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetSelection(GUITextInputWindow g)
        {
            if (g == _nameWindow)
            {
                GameManager.TakeInput();
                _nameWindow.Activate();
                _townWindow.Activate(false);
            }
            else if (g == _townWindow)
            {
                GameManager.TakeInput();
                _townWindow.Activate();
                _nameWindow.Activate(false);
            }
            else
            {
                GameManager.StopTakingInput();
                _nameWindow.Activate(false);
                _townWindow.Activate(false);
            }
        }

        public bool TextBoxesFilled()
        {
            return !string.IsNullOrEmpty(_townWindow.GetText()) && !string.IsNullOrEmpty(_nameWindow.GetText());
        }

        public string GetPlayerName()
        {
            return _nameWindow.GetText();
        }
        public string GetTownName()
        {
            return _townWindow.GetText();
        }

        #region Button Logic
        public void OpenCosmetics(GUICosmeticBox obj)
        {
            _eCurrentCosmeticSlot = obj.CosmeticSlot;

            _gColorPicker.Show(false);

            _liAllCosmetics.ForEach(x => x.RemoveSelfFromControl());
            _liAllCosmetics.Clear();

            _gSelected.CenterOnObject(obj);
            _gSelected.Show(true);

            var allCosmetics = PlayerManager.GetCosmetics(_eCurrentCosmeticSlot);
            if (obj.CosmeticSlot != CosmeticSlotEnum.Eyes)
            {
                allCosmetics.Insert(0, new KeyValuePair<int, bool>(-1, true));
            }

            for (int i = 0; i < allCosmetics.Count; i++)
            {
                var kvp = allCosmetics[i];
                var box = new GUICosmeticBox(_eCurrentCosmeticSlot, DataManager.GetCosmetic(kvp.Key), EquipCosmetic, false, kvp.Value);
                AddControl(box);

                if (_liAllCosmetics.Count == 0) { box.PositionAndMove(_window, 8, 108); }
                else if (_liAllCosmetics.Count % 9 != 0) { box.AnchorAndAlignWithSpacing(_liAllCosmetics[i - 1], SideEnum.Right, SideEnum.Top, 2); }
                else { box.AnchorAndAlignWithSpacing(_liAllCosmetics[i - 9], SideEnum.Bottom, SideEnum.Left, 2); }

                _liAllCosmetics.Add(box);
            }

            _gColorSwatch.Show(true);
            _gColorSwatch.AnchorAndAlignWithSpacing(_liAllCosmetics[0], SideEnum.Top, SideEnum.Left, 3);
            _gColorSwatch.SetColor(PlayerManager.PlayerActor.GetCosmeticColor(obj.CosmeticSlot));
        }

        public void EquipCosmetic(GUICosmeticBox obj)
        {
            _gColorPicker.Show(false);

            PlayerManager.PlayerActor.SetCosmetic(obj.CosmeticSlot, obj.Cosmetic);
            _displayBox.SyncSprites();

            foreach (var item in _liAppliedCosmetics)
            {
                if (item.CosmeticSlot == obj.CosmeticSlot)
                {
                    item.SetCosmetic(obj.Cosmetic);
                }
            }
        }

        public void SetCosmeticColor(Color c)
        {
            PlayerManager.PlayerActor.SetCosmeticColor(_eCurrentCosmeticSlot, c);
            _liAllCosmetics.FindAll(x => x.Unlocked).ForEach(x => x.SetCosmeticColor(c));
            _liAppliedCosmetics.Find(x => x.CosmeticSlot == _eCurrentCosmeticSlot).SetCosmeticColor(c);
            _displayBox.SyncSprites();
        }

        private void AssignColorPicker()
        {
            _gColorPicker.Show(true);
            _gColorPicker.SetAction(SetCosmeticColor, PlayerManager.PlayerActor.GetCosmeticColor(_eCurrentCosmeticSlot));
            _gColorPicker.AnchorAndAlignWithSpacing(_gColorSwatch, SideEnum.Left, SideEnum.CenterY, 5);
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

        public void BtnRandomize()
        {
            PlayerManager.PlayerActor.RandomizeCosmetics();
            _displayBox.SyncSprites();

            _liAppliedCosmetics.ForEach(x => x.SetCosmetic(PlayerManager.PlayerActor.GetCosmetic(x.CosmeticSlot)));
        }
        #endregion

        public override void CloseMainWindow()
        {
            PlayerManager.SetName(GetPlayerName());
            TownManager.SetTownName(GetTownName());
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

            GUIImage _gSwatch;

            public ColorPicker(GUIImage swatch) : base(GUIUtils.WINDOW_BROWN, GameManager.ScaleIt(48), GameManager.ScaleIt(36))
            {
                _gSwatch = swatch;
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
                    _gSwatch.SetColor(c);

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
                bool rv = base.ProcessLeftButtonClick(mouse);

                if (!rv) { Show(false); }

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

                _gHue.SetValue(_fHue / 3);
                _gSaturation.SetValue(_fSaturation * 4 / 3.3f * 100);
                _gValue.SetValue(_fValue * 4 / 3.3f * 100);

                Colorize();
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
                    for (int i = 0; i < PIXEL_NUM; i++)
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
                    ChosenValue = value;

                    _gToggle.Position(_gFrame.Position());
                    _gToggle.MoveBy((int)value, 0);
                }

                public void Colorize(float imagePixelValue, float hue, float saturation, float value)
                {
                    float newHue = hue;
                    float newSaturation = saturation;
                    float newValue = value;

                    for (int i = 0; i < PIXEL_NUM; i++)
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
}
