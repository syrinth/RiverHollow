using Microsoft.Xna.Framework.Input;
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
                    key == Keys.Escape || key == Keys.Enter || key == Keys.Space || key == Keys.Back || key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right)
                {
                    _keyDownDictionary.Add((Keys)k, false);
                }
            }
        }

        public static bool CheckKey(Keys key)
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


        public static string GetCharFromKey(Keys key)
        {
            string rv = "";

            if (key == Keys.Space) { rv = " "; }
            else if (key == Keys.Back) { rv = "--"; }
            else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
            { rv = key.ToString(); }
            else { rv = key.ToString().ToLower(); }

            return rv;
        }
    }
}
