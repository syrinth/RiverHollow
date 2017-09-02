using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Adventure.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Input;
using Adventure.Characters;
using Adventure.Tile_Engine;
using Adventure.Items;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects.GUIWindows
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
            AdventureGame.ChangeGameState(AdventureGame.GameState.Paused);
            _keySelection = 0;
            SeparateText(selectionText);
            _width = Math.Max(100, (int)_font.MeasureString(_text).X)+64;
            ParseText(_text);
            _height = (((_numReturns+1) + _options.Count) * (int)_characterSize) + _edgeSize;
            _optionsOffsetY = Math.Max((int)_characterSize, (int)((_numReturns+1) * _characterSize));

            _position = new Vector2(AdventureGame.ScreenWidth / 2 - _width / 2, AdventureGame.ScreenHeight / 2 - _height / 2);
            _imgSelection = new GUIImage(new Vector2((int)_position.X + 16, (int)_position.Y + 16 + _optionsOffsetY), new Rectangle(288, 96, 32, 32), (int)_characterSize, (int)_characterSize, @"Textures\Dialog");

            Load(new Vector2(0, 0), 32);
        }

        public GUITextSelectionWindow(Vector2 pos, string selectionText) : this(selectionText)
        {
            _position = new Vector2(AdventureGame.ScreenWidth / 4, AdventureGame.ScreenHeight - 180);
            _width = AdventureGame.ScreenWidth / 4;
            _height = 148;
        }

        public GUITextSelectionWindow(NPC talker, string selectionText) : base()
        {
            AdventureGame.ChangeGameState(AdventureGame.GameState.Paused);
            _talker = talker;

            _keySelection = 0;
            SeparateText(selectionText);
            ParseText(_text);
            _height = (((_numReturns + 1) + _options.Count) * (int)_characterSize) + _edgeSize;
            _optionsOffsetY = Math.Max((int)_characterSize, (int)((_numReturns+1) * _characterSize));

            _position.Y = AdventureGame.ScreenHeight - _height - BottomMargin;
            _imgSelection = new GUIImage(new Vector2((int)_position.X + 16, (int)_position.Y + 16 + _optionsOffsetY), new Rectangle(288, 96, 32, 32), (int)_characterSize, (int)_characterSize, @"Textures\Dialog");

            Load(new Vector2(0, 0), 32);
        }

        public GUITextSelectionWindow(Food f, string selectionText) : this(selectionText)
        {
            AdventureGame.ChangeGameState(AdventureGame.GameState.Paused);
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
                    _imgSelection.MoveImageBy(new Vector2(0, -_characterSize));
                    _keySelection--;
                }
            }
            if (InputManager.CheckKey(Keys.S))
            {
                if (_keySelection + 1 < _options.Count)
                {
                    _imgSelection.MoveImageBy(new Vector2(0, _characterSize));
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
            int xindex = (int)_position.X + 16;
            int yIndex = (int)_position.Y + 16;
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
                yIndex += (int)_characterSize;
            }
        }

        private void ProcessGameTextSelection(string action)
        {
            if (action.Equals("SleepNow"))
            {
                GUIManager.SetScreen(GUIManager.Screens.DayEnd);
                AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
            }
            else if (action.Contains("Eat") && _food != null)
            {
                if (_food.Number > 0)
                {
                    _food.Remove(1);
                    PlayerManager.Player.IncreaseStamina(_food.Stamina);
                    PlayerManager.Player.IncreaseHealth(_food.Health);
                    if (_food.Number == 0)
                    {
                        PlayerManager.Player.RemoveItemFromInventory(PlayerManager.Player.CurrentItemNumber);
                    }
                }
                GUIManager.RemoveComponent(this);
                AdventureGame.ChangeGameState(AdventureGame.GameState.Running, false);
            }
            else
            {
                GUIManager.RemoveComponent(this);
                AdventureGame.ChangeGameState(AdventureGame.GameState.Running, false);
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
