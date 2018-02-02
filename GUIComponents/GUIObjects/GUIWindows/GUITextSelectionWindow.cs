using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Items;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        private Point _mousePos = Point.Zero;
        private Food _food;
        private Dictionary<int,string> _options;
        private GUIImage _imgSelection;
        private int _keySelection;

        private int _optionsOffsetY;

        public GUITextSelectionWindow(string selectionText) : base()
        {
            Position = new Vector2(RiverHollow.ScreenWidth / 2 - _width / 2, RiverHollow.ScreenHeight / 2 - _height / 2);
            Setup(selectionText);
            _width = (int)_font.MeasureString(_text).X + _innerBorder * 2 + 6; //6 is adding a bit of arbitrary extra space for the parsing. Exactsies are bad
            PostParse();
        }

        public GUITextSelectionWindow(NPC talker, string selectionText) : base()
        {
            _talker = talker;
            _position.Y = RiverHollow.ScreenHeight - _height - SpaceFromBottom;
            Position = _position;   //don to set _drawRect

            Setup(selectionText);
            PostParse();
        }

        public void Setup(string selectionText)
        {
            GameManager.Pause();
            _keySelection = 0;
            SeparateText(selectionText);
        }
        public void PostParse()
        {
            ParseText(_text);
            _height = (((_numReturns + 1) + _options.Count) * (int)_characterHeight + _innerBorder * 2);
            _optionsOffsetY = Math.Max((int)_characterHeight, (int)((_numReturns + 1) * _characterHeight));
            _imgSelection = new GUIImage(new Vector2((int)_position.X + _innerBorder, (int)_position.Y + _innerBorder + _optionsOffsetY), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
        }

        public GUITextSelectionWindow(Food f, string selectionText) : this(selectionText)
        {
            GameManager.Pause();
            _food = f;
        }

        private void SeparateText(string selectionText)
        {
            _options = new Dictionary<int, string>();
            string[] firstPass = selectionText.Split(new[] { '[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            if (firstPass.Length > 0)
            {
                _text = firstPass[0];
                string[] secondPass = firstPass[1].Split('|');
                int key = 0;
                foreach(string s in secondPass)
                {
                    if (s.Contains("%"))
                    {
                        string[] friendshipPass = s.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        if (int.TryParse(friendshipPass[0], out int val))
                        {
                            if(_talker.Friendship >= val)
                            {
                                _options.Add(key++, friendshipPass[1]);
                            }
                        }
                    }
                    else
                    {
                        _options.Add(key++, s);
                    }
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (InputManager.CheckKey(Keys.W) || InputManager.CheckKey(Keys.Up))
            {
                if (_keySelection - 1 >= 0)
                {
                    _imgSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                    _keySelection--;
                }
            }
            else if (InputManager.CheckKey(Keys.S) || InputManager.CheckKey(Keys.Down))
            {
                if (_keySelection + 1 < _options.Count)
                {
                    _imgSelection.MoveImageBy(new Vector2(0, _characterHeight));
                    _keySelection++;
                }
            }
            else
            {
                //Until fixed for specific motion
                if (_mousePos != GraphicCursor.Position.ToPoint() && Contains(GraphicCursor.Position.ToPoint()))
                {
                    _mousePos = GraphicCursor.Position.ToPoint();
                    if (_keySelection - 1 >= 0 && GraphicCursor.Position.Y < _imgSelection.Position.Y)
                    {
                        _imgSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                        _keySelection--;
                    }
                    else if (_keySelection + 1 < _options.Count && GraphicCursor.Position.Y > _imgSelection.Position.Y + _imgSelection.Height)
                    {
                        _imgSelection.MoveImageBy(new Vector2(0, _characterHeight));
                        _keySelection++;
                    }
                }
            }

            if (InputManager.CheckKey(Keys.Enter))
            {
                SelectAction();
            }
        }

        public void SelectAction()
        {
            string action = _options[_keySelection].Split(':')[1];
            if (_talker == null)
            {
                ProcessGameTextSelection(action);
            }
            else
            {
                ProcessNPCDialogSelection(action);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            int xindex = (int)_position.X + _innerBorder;
            int yIndex = (int)_position.Y + _innerBorder;
            foreach (string s in _parsedStrings)
            {
                spriteBatch.DrawString(_font, s, new Vector2(xindex, yIndex), Color.White);
            }
            _imgSelection.Draw(spriteBatch);

            xindex += 32;
            yIndex += _optionsOffsetY;
            foreach (KeyValuePair<int, string> kvp in _options)
            {
                spriteBatch.DrawString(_font, kvp.Value.Split(':')[0], new Vector2(xindex, yIndex), Color.White);
                yIndex += (int)_characterHeight;
            }
        }

        private void ProcessGameTextSelection(string action)
        {
            if (action.Equals("SleepNow"))
            {
                GUIManager.SetScreen(GUIManager.Screens.DayEnd);
            }
            else if (action.Contains("Eat") && _food != null)
            {
                if (_food.Number > 0)
                {
                    _food.Remove(1);
                    PlayerManager.IncreaseStamina(_food.Stamina);
                    PlayerManager.Combat.IncreaseHealth(_food.Health);
                }
                GameManager.BackToMain();
            }
            else
            {
                GameManager.BackToMain();
            }
        }

        private void ProcessNPCDialogSelection(string action)
        {
            string nextText = _talker.GetDialogEntry(action);

            if (!string.IsNullOrEmpty(nextText))
            {
                GUIManager.LoadTextScreen(_talker, nextText);
            }
            else if(GUIManager.CurrentGUIScreen == GUIManager.Screens.Text || GUIManager.CurrentGUIScreen == GUIManager.Screens.TextInput)
            {
                GameManager.BackToMain();
            }
        }
    }
}
