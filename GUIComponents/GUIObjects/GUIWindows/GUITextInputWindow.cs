using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.NPCs;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    class GUITextInputWindow : GUITextWindow
    {
        int _strLen;
        int _maxLength = 10;
        WorldAdventurer _w;
        WorkerBuilding _b;
        SideEnum _textLoc;
        GUIText _gStatement;
        GUIText _gText;

        public GUITextInputWindow() : base()
        {
            GameManager.ReadInput();
            GameManager.Pause();
        }

        public GUITextInputWindow(string statement, SideEnum textLoc) : this()
        {
            StatementSetup(statement, textLoc);
            _textLoc = textLoc;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _strLen = 0;
            _text = string.Empty;
        }

        public GUITextInputWindow(WorldAdventurer w) : this()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Enter name:");
            Width = Math.Max(_gStatement.Width, (int)_characterWidth * 10);
            Height = (int)_characterHeight * 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _strLen = 0;
            _w = w;
            _text = string.Empty;
        }

        public GUITextInputWindow(WorkerBuilding b) : this()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Name Building:");
            Width = Math.Max(_gStatement.Width, (int)_characterWidth * 10);
            Height = (int)_characterHeight * 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _strLen = 0;
            _b = b;
            _text = string.Empty;
        }

        public GUITextInputWindow(ref string text): this()
        {
            _strLen = 0;
            _text = text;
        }

        private void StatementSetup(string text, SideEnum textLoc = SideEnum.Top)
        {
            _gStatement = new GUIText(text);

            if (textLoc == SideEnum.Top)
            {
                Width = Math.Max(_gStatement.Width, (int)_characterWidth * 10);
                Height = _gStatement.Height*2;
            }
            else if (textLoc == SideEnum.Left)
            {
                Width = _gStatement.Width + (int)_characterWidth * 10;
                Height = _gStatement.Height;
            }

            _gStatement.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gStatement.SetColor(Color.White);

            _gText = new GUIText();
            _gStatement.SetColor(Color.White);

            if (textLoc == SideEnum.Top)
            {
                _gText.AnchorAndAlignToObject(_gStatement, SideEnum.Bottom, SideEnum.Left);
            }
            else if (textLoc == SideEnum.Left)
            {
                _gText.AnchorAndAlignToObject(_gStatement, SideEnum.Right, SideEnum.Bottom, 10);
            }
            Controls.Add(_gText);
            
            Resize();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Keys k in InputManager.KeyDownDictionary.Keys.ToList())
            {
                if (InputManager.CheckPressedKey(k))
                {
                    if (k == Keys.Enter)
                    {
                        bool finished = false;
                        if (_w != null)
                        {
                            finished = true;
                            _w.SetName(_text);
                        }
                        if (_b != null)
                        {
                            finished = true;
                            _b.SetName(_text);  
                        }

                        if(finished){
                            RiverHollow.ResetCamera();
                            GameManager.Unpause();
                            GameManager.Scry(false);
                            GameManager.DontReadInput();
                        }
                    }
                    else
                    {
                        string input = InputManager.GetCharFromKey(k);
                        if (input != "--" && _gText.Length < _maxLength)
                        {
                            _gText.Insert(input);
                        }
                        else if (input == "--")
                        {
                            if (_gText.Length > 0)
                            {
                                _gText.RemoveLast();
                            }
                        }
                    }
                }
            }
        }

        public string GetText()
        {
            return _text;
        }
    }
}
