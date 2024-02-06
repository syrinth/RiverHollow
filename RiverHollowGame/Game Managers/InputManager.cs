using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class InputManager
    {
        public static List<Keys> Numbers = new List<Keys> { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
        private static Dictionary<Keys, bool> _diKeysDown;
        private static MouseState _lastMouseState = new MouseState();
        private static RHTimer _mouseTimer;
        public static bool ButtonHeld = _mouseTimer != null;

        public static void Load()
        {
            _diKeysDown = new Dictionary<Keys, bool>();
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                _diKeysDown.Add(k, false);
            }
        }

        public static void Update(GameTime gTime)
        {
            _lastMouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            var keys = new List<Keys>(_diKeysDown.Keys);
            foreach (var k in keys)
            {
                _diKeysDown[k] = keyboardState.IsKeyDown(k);
            }

            _mouseTimer?.TickDown(gTime);
        }

        public static bool ButtonPressed(ButtonEnum e, out bool interval)
        {
            interval = false;

            MouseState ms = Mouse.GetState();
            switch (e)
            {
                case ButtonEnum.Left:
                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        if (_mouseTimer == null)
                        {
                            _mouseTimer = new RHTimer(Constants.MOUSE_PRESS_INTERVAL);
                            return true;
                        }
                        else if (_mouseTimer.Finished())
                        {
                            interval = true;
                            return true;
                        }
                    }
                    else
                    {
                        _mouseTimer = null;
                        return false;
                    }
                    return ms.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released;
                case ButtonEnum.Right:
                    return ms.RightButton == ButtonState.Pressed && _lastMouseState.RightButton == ButtonState.Released;
                case ButtonEnum.Middle:
                    return ms.MiddleButton == ButtonState.Pressed && _lastMouseState.MiddleButton == ButtonState.Released;
            }

            return false;
        }

        public static int ScrollWheelChanged()
        {
            int rv = 0;

            var currValue = Mouse.GetState().ScrollWheelValue;
            var lastValue = _lastMouseState.ScrollWheelValue;

            if (currValue > lastValue)
            {
                return 1;
            }
            else if (currValue < lastValue)
            {
                return -1;
            }

            return rv;
        }

        //Note: This only grabs one button press due to trying to control up/down issues.
        public static bool CheckForInitialKeyDown(Keys key)
        {
            return _diKeysDown[key] == false && IsKeyDown(key);
        }

        public static bool IsKeyDown(Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key);
        }

        public static string GetCharFromKey(Keys key)
        {
            string rv = "";

            if (!IsShift(key))
            {
                if (key == Keys.Space) { rv = " "; }
                else if (key == Keys.Back) { rv = "--"; }
                else if (key == Keys.Delete) { rv = "-+"; }
                else if (key == Keys.OemMinus)
                {
                    if (ShiftDown()) { rv = "_"; }
                    else { rv = "-"; }
                }
                else if (IsNumber(key)) { rv = key.ToString().Remove(0, 1); }
                else if (IsLetter(key))
                {
                    if (ShiftDown()) { rv = key.ToString(); }
                    else { rv = key.ToString().ToLower(); }
                }
            }

            return rv;
        }

        public static void ConsumeHeldButton()
        {
            _mouseTimer.Reset();
        }

        private static bool IsLetter(Keys k) { return k >= Keys.A && k <= Keys.Z; }
        public static bool IsNumber(Keys k) { return k >= Keys.D0 && k <= Keys.D9; }
        private static bool IsShift(Keys k) { return k == Keys.LeftShift || k == Keys.RightShift; }
        private static bool ShiftDown() { return Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift); }
    }
}
