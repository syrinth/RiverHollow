﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace RiverHollow.Game_Managers
{
    public static class InputManager
    {
        private static Dictionary<Keys, bool> _keyDownDictionary;
        public static Dictionary<Keys, bool> KeyDownDictionary { get => _keyDownDictionary; }

        public static void Load()
        {
            _keyDownDictionary = new Dictionary<Keys, bool>();
            foreach (var k in Enum.GetValues(typeof(Keys)))
            {
                Keys key = (Keys)k;
                if ((key >= Keys.A && key <= Keys.Z) ||
                {
                    _keyDownDictionary.Add((Keys)k, false);
                }
            }
        }

        //Note: This only grabs one button press due to trying to control up/down issues.
        public static bool CheckPressedKey(Keys key)
        {
            bool rv = false;
            KeyboardState keyboardState = Keyboard.GetState();
            bool keyDownThisFrame = (keyboardState.IsKeyDown(key));

            if (!_keyDownDictionary[key] && keyDownThisFrame)
            {
                rv = true;
            }
            _keyDownDictionary[key] = keyDownThisFrame;

            return rv;
        }

        public static bool IsKeyHeld(Keys key)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            return keyboardState.IsKeyDown(key);
        }

        public static string GetCharFromKey(Keys key)
        {
            string rv = "";

            if (key == Keys.Space) { rv = " "; }
            else if (key == Keys.Back) { rv = "--"; }
            else if (key == Keys.Delete) { rv = "-+"; }
            else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
            { rv = key.ToString(); }
            else { rv = key.ToString().ToLower(); }

            return rv;
        }
    }
}
