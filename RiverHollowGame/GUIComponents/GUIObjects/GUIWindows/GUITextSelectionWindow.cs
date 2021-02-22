using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        string _sStatement;
        protected Point _poiMouse = Point.Zero;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected Dictionary<int, SelectionData> _diOptions;

        public GUITextSelectionWindow(string selectionText, bool open = true)
        {
            _diOptions = new Dictionary<int, SelectionData>();
            ConfigureHeight();
            _iKeySelection = 0;
            SeparateText(selectionText);
            PostParse();

            Setup(open);
        }

        public void PostParse()
        {
            SyncText(_sStatement, true);
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            _giSelection = new GUIImage(new Rectangle(288, 96, 8, 9), GameManager.ScaleIt(8), GameManager.ScaleIt(9), DataManager.DIALOGUE_TEXTURE);
            _giSelection.AnchorAndAlignToObject(_giText, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_giSelection);

            AssignToColumn();
        }

        private void SeparateText(string selectionText)
        {
            string[] firstPass = selectionText.Split(new[] { '{', '}'}, StringSplitOptions.RemoveEmptyEntries);
            if (firstPass.Length > 0)
            {
                _sStatement = firstPass[0];

                string[] secondPass = Util.FindParams(firstPass[1]);
                int key = 0;
                foreach (string s in secondPass)
                {
                    string[] actionType = s.Split(':');
                    SelectionData t = new SelectionData(actionType[0], actionType[1]);
                    _diOptions.Add(key++, t);
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            if (_bOpening)
            {
                HandleOpening(gTime);
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
                    if (_poiMouse != GUICursor.Position.ToPoint() && Contains(GUICursor.Position.ToPoint()))
                    {
                        _poiMouse = GUICursor.Position.ToPoint();
                        if (_iKeySelection - 1 >= 0 && GUICursor.Position.Y < _giSelection.Position().Y)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                            _iKeySelection--;
                        }
                        else if (_iKeySelection + 1 < _diOptions.Count && GUICursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
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

        /// <summary>
        /// Triggered when the user selects an option in the GUITextSelectionWindow
        /// 
        /// This method retrieves the appropriate selectedAction and passes it to the relevant
        /// object for processing.
        /// </summary>
        protected void SelectAction()
        {
            string selectedAction = _diOptions[_iKeySelection].Action;

            if (GameManager.CurrentNPC != null)
            {
                string nextText = string.Empty;
                bool rv = GameManager.CurrentNPC.HandleTextSelection(selectedAction, ref nextText);

                if (!rv) { GUIManager.CloseTextWindow(); }
                else { GUIManager.SetWindowText(nextText, GameManager.CurrentNPC, true); }
            }
            else if (GameManager.CurrentItem != null)
            {
                if (!selectedAction.Equals("Cancel")) { GameManager.CurrentItem.UseItem(selectedAction); }
                GUIManager.CloseTextWindow();
            }
            else
            {
                GameManager.ProcessTextInteraction(selectedAction);
                GUIManager.CloseTextWindow();
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
                    RemoveControl(g.GText);
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

            public SelectionData(string text, string action = "", string fontName = DataManager.FONT_MAIN)
            {
                _gText = new GUIText(text, true, fontName);
                _sAction = action;
            }
        }

        public override bool IsSelectionBox() { return true; }
    }
}
