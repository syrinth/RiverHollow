﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.WorldObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        Food _food;

        protected Point _poiMouse = Point.Zero;
        protected Dictionary<int,string> _diOptions;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected int _iOptionsOffsetY;

        protected GUITextSelectionWindow() : base() { }
        public GUITextSelectionWindow(string selectionText) : base()
        {
            Setup(selectionText);
            Width = (int)_font.MeasureString(_text).X + _iInnerBorder * 2 + 6; //6 is adding a bit of arbitrary extra space for the parsing. Exactsies are bad
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            PostParse();
        }

        public GUITextSelectionWindow(NPC talker, string selectionText) : base()
        {
            _talker = talker;
            Position(new Vector2(Position().X, RiverHollow.ScreenHeight - Height - SpaceFromBottom));

            Setup(selectionText);
            PostParse();
        }

        public void Setup(string selectionText)
        {
            GameManager.Pause();
            _iKeySelection = 0;
            SeparateText(selectionText);
        }
        public void PostParse()
        {
            ParseText(_text);
            Height = (((_numReturns + 1) + _diOptions.Count) * (int)_characterHeight + _iInnerBorder * 2);
            _iOptionsOffsetY = Math.Max((int)_characterHeight, (int)((_numReturns + 1) * _characterHeight));
            _giSelection = new GUIImage(new Vector2((int)Position().X + _iInnerBorder, (int)Position().Y + _iInnerBorder + _iOptionsOffsetY), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
        }

        public GUITextSelectionWindow(Food f, string selectionText) : this(selectionText)
        {
            GameManager.Pause();
            _food = f;
        }

        private void SeparateText(string selectionText)
        {
            _diOptions = new Dictionary<int, string>();
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
                                _diOptions.Add(key++, friendshipPass[1]);
                            }
                        }
                    }
                    else
                    {
                        _diOptions.Add(key++, s);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.CheckKey(Keys.W) || InputManager.CheckKey(Keys.Up))
            {
                if (_iKeySelection - 1 >= 0)
                {
                    _giSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                    _iKeySelection--;
                }
            }
            else if (InputManager.CheckKey(Keys.S) || InputManager.CheckKey(Keys.Down))
            {
                if (_iKeySelection + 1 < _diOptions.Count)
                {
                    _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                    _iKeySelection++;
                }
            }
            else
            {
                //Until fixed for specific motion
                if (_poiMouse != GraphicCursor.Position.ToPoint() && Contains(GraphicCursor.Position.ToPoint()))
                {
                    _poiMouse = GraphicCursor.Position.ToPoint();
                    if (_iKeySelection - 1 >= 0 && GraphicCursor.Position.Y < _giSelection.Position().Y)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                        _iKeySelection--;
                    }
                    else if (_iKeySelection + 1 < _diOptions.Count && GraphicCursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                        _iKeySelection++;
                    }
                }
            }

            if (InputManager.CheckKey(Keys.Enter))
            {
                SelectAction();
            }
        }

        protected virtual void SelectAction()
        {
            string action = _diOptions[_iKeySelection].Split(':')[1];
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
            int xindex = (int)Position().X + _iInnerBorder;
            int yIndex = (int)Position().Y + _iInnerBorder;
            foreach (string s in _parsedStrings)
            {
                spriteBatch.DrawString(_font, s, new Vector2(xindex, yIndex), Color.White);
            }
            _giSelection.Draw(spriteBatch);

            xindex += 32;
            yIndex += _iOptionsOffsetY;
            foreach (KeyValuePair<int, string> kvp in _diOptions)
            {
                spriteBatch.DrawString(_font, kvp.Value.Split(':')[0], new Vector2(xindex, yIndex), Color.White);
                yIndex += (int)_characterHeight;
            }
        }

        protected void DrawWindow(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                SelectAction();
                rv = true;
            }
            return rv;
        }

        protected virtual void ProcessGameTextSelection(string action)
        {
            if (action.Equals("SleepNow"))
            {
                GUIManager.SetScreen(new DayEndScreen());
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
                GUIManager.SetScreen(new TextScreen(_talker, nextText));
            }
            else if(GUIManager.IsTextScreen())
            {
                GameManager.BackToMain();
            }
        }

        internal void Clear()
        {
            _iKeySelection = 0;
            _diOptions.Clear();
            _giSelection = new GUIImage(new Vector2((int)Position().X + _iInnerBorder, (int)Position().Y + _iInnerBorder), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
        }
    }
}
