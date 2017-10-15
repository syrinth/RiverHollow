using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Tile_Engine;
using RiverHollow.Items;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        private Food _food;
        private Dictionary<int,string> _options;
        private GUIImage _imgSelection;
        private int _keySelection;

        private int _optionsOffsetY;

        public GUITextSelectionWindow(string selectionText) : base()
        {
            _position = new Vector2(RiverHollow.ScreenWidth / 2 - _width / 2, RiverHollow.ScreenHeight / 2 - _height / 2);
            Setup(selectionText);
            _width = (int)_font.MeasureString(_text).X + _innerBorder * 2 + 6; //6 is adding a bit of arbitrary extra space for the parsing. Exactsies are bad
            PostParse();
        }

        public GUITextSelectionWindow(NPC talker, string selectionText) : base()
        {
            _talker = talker;
            _position.Y = RiverHollow.ScreenHeight - _height - SpaceFromBottom;

            Setup(selectionText);
            PostParse();
        }

        public void Setup(string selectionText)
        {
            RiverHollow.ChangeGameState(RiverHollow.GameState.Paused);
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
            RiverHollow.ChangeGameState(RiverHollow.GameState.Paused);
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
                    _options.Add(key++, s);
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (InputManager.CheckKey(Keys.W))
            {
                if (_keySelection - 1 >= 0)
                {
                    _imgSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                    _keySelection--;
                }
            }
            if (InputManager.CheckKey(Keys.S))
            {
                if (_keySelection + 1 < _options.Count)
                {
                    _imgSelection.MoveImageBy(new Vector2(0, _characterHeight));
                    _keySelection++;
                }
            }
            if (InputManager.CheckKey(Keys.Enter))
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
                GUIManager.RemoveComponent(this);
                RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
            }
            else{
                GUIManager.RemoveComponent(this);
                RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
            }
        }

        private void ProcessNPCDialogSelection(string action)
        {
            string nextText = _talker.GetDialogEntry(action);

            if (!string.IsNullOrEmpty(nextText))
            {
                GUIManager.LoadScreen(GUIManager.Screens.Text, _talker, nextText);
            }
        }
    }
}
