using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.CombatStuff;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextCombatMenuWindow : GUITextSelectionWindow
    {
        const int _iMaxMenuActions = 4;
        List<Ability> _liAbilities;
        Ability _chosenAbility;
        public Ability ChosenAbility { get => _chosenAbility; }
        Vector2 _vecMenuSize;
        SpriteFont _fFont;

        public GUITextCombatMenuWindow(int startX, int width)
        {
            _iOptionsOffsetY = 0;
            _diOptions = new Dictionary<int, string>();
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _vecMenuSize = _fFont.MeasureString("XXXXXXXX");

            _width = width;
            _height = (int)(_vecMenuSize.Y * _iMaxMenuActions);
            _edgeSize = GreyDialogEdge;
            _sourcePoint = GreyDialog;

            Position = new Vector2(startX, RiverHollow.ScreenHeight - GreyDialogEdge - (_vecMenuSize.Y * _iMaxMenuActions) - RiverHollow.ScreenHeight/100);

            _giSelection = new GUIImage(new Vector2((int)_position.X + _innerBorder, (int)_position.Y + _innerBorder), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
        }

        public void Assign(List<Ability> abilities)
        {
            int key = 0;
            if (_diOptions.Count == 0)
            {
                _liAbilities = abilities;
                _iKeySelection = 0;
                foreach (Ability s in abilities)
                {
                    _diOptions.Add(key++, s.Name);
                }
            }
        }

        public void Clear()
        {
            _diOptions.Clear();
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

        protected override void SelectAction()
        {
            _chosenAbility = _liAbilities[_iKeySelection];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.DrawWindow(spriteBatch);
            int xindex = (int)_position.X + _innerBorder;
            int yIndex = (int)_position.Y + _innerBorder;

            if (_diOptions.Count > 0) { _giSelection.Draw(spriteBatch); }

            xindex += 32;
            yIndex += _iOptionsOffsetY;
            int i = Math.Max(0, _iKeySelection - _iMaxMenuActions);
            foreach (KeyValuePair<int, string> kvp in _diOptions)
            {
                if (kvp.Key >= i) {
                    Color c = (_chosenAbility != null && kvp.Value == _chosenAbility.Name) ? Color.Green : Color.White;
                    spriteBatch.DrawString(_fFont, kvp.Value, new Vector2(xindex, yIndex), c);
                    yIndex += (int)_characterHeight;
                }
            }
        }

        public void ClearChosenAbility()
        {
            _chosenAbility = null;
        }
    }
}
