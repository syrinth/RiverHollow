using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters.NPCs;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    class GUITextInputWindow : GUITextWindow
    {
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

        }

        public GUITextInputWindow(WorldAdventurer w) : this()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Enter name:");
            Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
            Height = _gStatement.CharHeight * 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _w = w;
        }

        public GUITextInputWindow(WorkerBuilding b) : this()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Name Building:");
            Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
            Height = _gStatement.CharHeight * 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _b = b;
        }

        private void StatementSetup(string text, SideEnum textLoc = SideEnum.Top)
        {
            _gStatement = new GUIText(text);

            if (textLoc == SideEnum.Top)
            {
                Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
                Height = _gStatement.Height*2;
            }
            else if (textLoc == SideEnum.Left)
            {
                Width = _gStatement.Width + _gStatement.CharWidth * 10;
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
                            _w.SetName(_gText.GetText());
                        }
                        if (_b != null)
                        {
                            finished = true;
                            _b.SetName(_gText.GetText());  
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
            return _gText.GetText();
        }
    }
}
