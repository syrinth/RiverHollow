using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Actors;
using RiverHollow.Misc;
using RiverHollow.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        string _sStatement;
        protected Point _poiMouse = Point.Zero;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected int _iOptionsOffsetY;
        protected Dictionary<int, SelectionData> _diOptions;

        public GUITextSelectionWindow() : base()
        {
            _diOptions = new Dictionary<int, SelectionData>();
        }
        public GUITextSelectionWindow(string selectionText, bool open = true) : this()
        {
            Height = Math.Max(Height, (_iCharHeight * MAX_ROWS));
            Position(new Vector2(Position().X, RiverHollow.ScreenHeight - Height - SpaceFromBottom));
            _iKeySelection = 0;
            SeparateText(selectionText);
            PostParse();

            Setup(open);
        }

        public void PostParse()
        {
            ParseText(_sStatement);
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft);
            _iOptionsOffsetY = Math.Max(_iCharHeight, (int)((_numReturns + 1) * _iCharHeight));
            _giSelection = new GUIImage(new Rectangle(288, 96, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");
            _giSelection.AnchorAndAlignToObject(_giText, SideEnum.Bottom, SideEnum.Left);
            AddControl(_giSelection);

            AssignToColumn();
        }

        private void SeparateText(string selectionText)
        {
            string[] firstPass = selectionText.Split(new[] { '[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            if (firstPass.Length > 0)
            {
                _sStatement = firstPass[0];

                string[] secondPass = firstPass[1].Split('|');
                int key = 0;
                foreach (string s in secondPass)
                {
                    string[] actionType = s.Split(':');
                    SelectionData t = new SelectionData(actionType[0], actionType[1]);
                    _diOptions.Add(key++, t);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_bOpening)
            {
                HandleOpening(gameTime);
            }
            else
            {
                if (InputManager.CheckPressedKey(Keys.W) || InputManager.CheckPressedKey(Keys.Up))
                {
                    if (_iKeySelection - 1 >= 0)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                        _iKeySelection--;
                    }
                }
                else if (InputManager.CheckPressedKey(Keys.S) || InputManager.CheckPressedKey(Keys.Down))
                {
                    if (_iKeySelection + 1 < _diOptions.Count)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.Bottom);
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
                            _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                            _iKeySelection--;
                        }
                        else if (_iKeySelection + 1 < _diOptions.Count && GraphicCursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.Bottom);
                            _iKeySelection++;
                        }
                    }
                }

                if (InputManager.CheckPressedKey(Keys.Enter))
                {
                    SelectAction();
                }
            }
        }

        protected virtual void SelectAction()
        {
            string selectedAction = _diOptions[_iKeySelection].Action;

            if (GameManager.gmNPC != null)
            {
                if (!GameManager.gmNPC.HandleTextInteraction(selectedAction))
                {
                    GUIManager.CloseTextWindow(this);
                }
            }
            else if (GameManager.gmActiveItem != null)
            {
                if (!selectedAction.Equals("Cancel")) { GameManager.gmActiveItem.UseItem(selectedAction); }
                GUIManager.CloseTextWindow(this);
            }
            else
            {
                GameManager.ProcessTextInteraction(selectedAction);
                GUIManager.CloseTextWindow(this);
            }
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

        internal void Clear()
        {
            _iKeySelection = 0;
            
            foreach (SelectionData g in _diOptions.Values)
            {
                if (Controls.Contains(g.GText))
                {
                    Controls.Remove(g.GText);
                }
            }

            _diOptions.Clear();
        }

        protected void AssignToColumn()
        {
            for (int i = 0; i < _diOptions.Count; i++)
            {
                GUIText gText = _diOptions[i].GText;
                if (i == 0) { gText.AnchorAndAlignToObject(_giSelection, SideEnum.Right, SideEnum.Bottom); }
                else { gText.AnchorAndAlignToObject(_diOptions[i - 1].GText, SideEnum.Bottom, SideEnum.Left); }
                AddControl(gText);
            }
        }

        protected class SelectionData
        {
            GUIText _gText;
            public GUIText GText => _gText;
            string _sAction;
            public string Action => _sAction;

            public string Text => _gText.Text;

            public SelectionData(string text, string action = "", string fontName = @"Fonts\Font")
            {
                _gText = new GUIText(text, true, fontName);
                _sAction = action;
            }
        }

        public override bool IsSelectionBox() { return true; }
    }
}
