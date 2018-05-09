using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Characters.NPCs;
using RiverHollow.Characters;
using RiverHollow.Misc;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        string _sStatement;
        protected Point _poiMouse = Point.Zero;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected int _iOptionsOffsetY;
        List<GUIText> _liOptions;

        public GUITextSelectionWindow(string selectionText) : base()
        {
            Setup(selectionText);
            Width = (int)_liText[0].TextSize.X + 6; //6 is adding a bit of arbitrary extra space for the parsing. Exactsies are bad
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            PostParse();
        }

        public GUITextSelectionWindow(NPC talker, string selectionText) : base()
        {
            GameManager.gmNPC = talker;
            Position(new Vector2(Position().X, RiverHollow.ScreenHeight - Height - SpaceFromBottom));

            Setup(selectionText);
            PostParse();
        }

        public GUITextSelectionWindow(string selectionText, int door = -1) : base()
        {
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
            ParseText(_sStatement);
            Height = (((_numReturns + 1) + _liOptions.Count) * _iCharHeight);
            _iOptionsOffsetY = Math.Max(_iCharHeight, (int)((_numReturns + 1) * _iCharHeight));
            _giSelection = new GUIImage(new Vector2((int)Position().X, (int)Position().Y + _iOptionsOffsetY), new Rectangle(288, 96, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");
            _giSelection.AnchorAndAlignToObject(_liText[0], SideEnum.Bottom, SideEnum.Left);

            foreach (GUIText t in _liOptions)
            {
                if (_liOptions[0] == t) { t.AnchorAndAlignToObject(_giSelection, SideEnum.Right, SideEnum.Bottom, 16); }
                else { t.AnchorAndAlignToObject(_liOptions[_liOptions.Count - 1], SideEnum.Bottom, SideEnum.Left, 16); }
            }
            
        }

        private void SeparateText(string selectionText)
        {
            _liOptions = new List<GUIText>();
            string[] firstPass = selectionText.Split(new[] { '[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            if (firstPass.Length > 0)
            {
                _sStatement = firstPass[0];

                string[] secondPass = firstPass[1].Split('|');
                //int key = 0;
                foreach (string s in secondPass)
                {
                    GUIText t = new GUIText(s);
                    _liOptions.Add(t);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.CheckPressedKey(Keys.W) || InputManager.CheckPressedKey(Keys.Up))
            {
                if (_iKeySelection - 1 >= 0)
                {
                    _giSelection.MoveImageBy(new Vector2(0, -_iCharHeight));
                    _iKeySelection--;
                }
            }
            else if (InputManager.CheckPressedKey(Keys.S) || InputManager.CheckPressedKey(Keys.Down))
            {
                if (_iKeySelection + 1 < _liOptions.Count)
                {
                    _giSelection.MoveImageBy(new Vector2(0, _iCharHeight));
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
                        _giSelection.MoveImageBy(new Vector2(0, -_iCharHeight));
                        _iKeySelection--;
                    }
                    else if (_iKeySelection + 1 < _liOptions.Count && GraphicCursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, _iCharHeight));
                        _iKeySelection++;
                    }
                }
            }

            if (InputManager.CheckPressedKey(Keys.Enter))
            {
                SelectAction();
            }
        }

        protected virtual void SelectAction()
        {
            string action = _liOptions[_iKeySelection].GetText().Split(':')[1];
            if (GameManager.gmNPC == null || action.Equals("SellContract"))
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
            int xindex = (int)Position().X;
            int yIndex = (int)Position().Y;

            _giSelection.Draw(spriteBatch);

            foreach(GUIText t in _liOptions)
            {
                t.Draw(spriteBatch);
            }

            ////xindex += 32;
            //yIndex += _iOptionsOffsetY;
            //foreach (KeyValuePair<int, string> kvp in _diOptions)
            //{
            //    spriteBatch.DrawString(_font, kvp.Value.Split(':')[0], new Vector2(xindex, yIndex), Color.White);
            //    yIndex += _iCharHeight;
            //}
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
            else if (action.Equals("OpenDoor"))
            {
                GUIManager.SetScreen(new InventoryScreen(GameManager.gmDoor));
            }
            else if (action.Contains("UseItem"))
            {
                GameManager.UseItem();
            }
            else if (action.Contains("SellContract") && GameManager.gmNPC != null)
            {
                if (GameManager.gmNPC.IsWorldAdventurer())
                {
                    ((WorldAdventurer)GameManager.gmNPC).Building.RemoveWorker((WorldAdventurer)GameManager.gmNPC);
                    PlayerManager.AddMoney(1000);
                    GameManager.BackToMain();
                }
            }
            else
            {
                GameManager.BackToMain();
            }
        }

        private void ProcessNPCDialogSelection(string action)
        {
            if (GameManager.gmNPC != null)
            {
                string nextText = GameManager.gmNPC.GetDialogEntry(action);

                if (action.StartsWith("Quest"))
                {
                    Quest q = GameManager.DIQuests[int.Parse(action.Remove(0, "Quest".Length))];
                    PlayerManager.AddToQuestLog(q);
                    GUIManager.SetScreen(new TextScreen(GameManager.gmNPC, q.Description));
                }
                else if (!string.IsNullOrEmpty(nextText))
                {
                    GUIManager.SetScreen(new TextScreen(GameManager.gmNPC, nextText));
                }
                else if (GUIManager.IsTextScreen())
                {
                    GameManager.BackToMain();
                }
            }
        }

        internal void Clear()
        {
            _iKeySelection = 0;
            _liOptions.Clear();
            _giSelection = new GUIImage(new Vector2((int)Position().X, (int)Position().Y), new Rectangle(288, 96, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");
        }
    }
}
