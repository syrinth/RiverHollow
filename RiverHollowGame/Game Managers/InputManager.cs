using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class InputManager
    {
        public static Dictionary<Keys, bool> KeyDownDictionary { get; private set; }

        public static void Load()
        {
            KeyDownDictionary = new Dictionary<Keys, bool>();
            foreach (var k in Enum.GetValues(typeof(Keys)))
            {
                KeyDownDictionary.Add((Keys)k, false);
            }
        }

        //Note: This only grabs one button press due to trying to control up/down issues.
        public static bool CheckPressedKey(Keys key)
        {
            bool rv = false;
            KeyboardState keyboardState = Keyboard.GetState();
            bool keyDownThisFrame = (keyboardState.IsKeyDown(key));

            if (!KeyDownDictionary[key] && keyDownThisFrame)
            {
                rv = true;
            }
            KeyDownDictionary[key] = keyDownThisFrame;

            return rv;
        }

        public static bool IsKeyDown(Keys key)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            return keyboardState.IsKeyDown(key);
        }

        public static string GetCharFromKey(Keys key)
        {
            string rv = "";

            if (!IsShift(key))
            {
                if (key == Keys.Space) { rv = " "; }
                else if (key == Keys.Back) { rv = "--"; }
                else if (key == Keys.Delete) { rv = "-+"; }
                else if (key == Keys.OemMinus) {
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

        private static bool IsLetter(Keys k) { return k >= Keys.A && k <= Keys.Z; }
        private static bool IsNumber(Keys k) { return k >= Keys.D0 && k <= Keys.D9; }
        private static bool IsShift(Keys k) { return k == Keys.LeftShift || k == Keys.RightShift; }
        private static bool ShiftDown() { return Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift); }
    }
}
