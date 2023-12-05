using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects
{
    internal class GUIToggle : GUIObject
    {
        public bool Selected { get; private set; }
        private bool _bTab = false;
        GUIImage _gUnselected;
        GUIImage _gSelected;
        GUIImage _gIcon;
        GUIToggle[] toggleGroup;

        EmptyDelegate _delAction;
        public GUIToggle(Rectangle icon, string texture, EmptyDelegate del)
        {
            _delAction = del;
            _bTab = true;

            _gUnselected = new GUIImage(GUIUtils.TAB_UNSELECTED, texture);
            _gSelected = new GUIImage(GUIUtils.TAB_SELECTED, texture);
            _gIcon = new GUIImage(icon, texture);
            _gSelected.MoveBy(0, _gUnselected.Height - _gSelected.Height);

            Width = _gUnselected.Width;
            Height = _gUnselected.Height;

            AddControl(_gSelected);
            AddControl(_gUnselected);
            AddControl(_gIcon);

            Select(false);
        }
        public GUIToggle(Rectangle unselected, Rectangle selected, Rectangle icon, string texture, EmptyDelegate del)
        {
            _delAction = del;
            _bTab = false;

            _gUnselected = new GUIImage(unselected, texture);
            _gSelected = new GUIImage(selected, texture);
            _gIcon = new GUIImage(icon, texture);

            Width = _gUnselected.Width;
            Height = _gUnselected.Height;

            AddControl(_gSelected);
            AddControl(_gUnselected);
            AddControl(_gIcon);

            Select(false);
        }

        internal override void Show(bool val)
        {
            Visible = val;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Active && !Selected && _gUnselected.Contains(mouse))
            {
                rv = true;
                SyncToggles();
                _delAction();
            }

            return rv;
        }

        private void Select(bool value)
        {
            Selected = value;
            _gSelected.Show(Selected);
            _gUnselected.Show(!Selected);

            if (_bTab)
            {
                if (Selected)
                {
                    _gIcon.PositionAndMove(_gSelected, 4, 4);
                }
                else
                {
                    _gIcon.PositionAndMove(_gUnselected, 4, 4);
                }
            }
        }

        private void SyncToggles()
        {
            Select(true);
            if (toggleGroup != null)
            {
                for (int i = 0; i < toggleGroup.Length; i++)
                {
                    toggleGroup[i].Select(false);
                }
            }
        }
        public void AssignToggleGroup(bool fireDelegate, params GUIToggle[] toggles)
        {
            toggleGroup = new GUIToggle[toggles.Length];
            for (int i = 0; i < toggles.Length; i++)
            {
                toggleGroup[i] = toggles[i];
                toggles[i].AssignToggleGroup(this, toggles);
            }

            Select(true);

            if (fireDelegate)
            {
                _delAction();
            }
        }
        public void AssignToggleGroup(GUIToggle primeToggle, GUIToggle[] toggles)
        {
            toggleGroup = new GUIToggle[toggles.Length];
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i].Equals(this)) { toggleGroup[i] = primeToggle; }
                else { toggleGroup[i] = toggles[i]; }
            }
        }
    }
}
