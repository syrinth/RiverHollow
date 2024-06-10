using Microsoft.Xna.Framework;

namespace RiverHollow.GUIComponents.GUIObjects
{
    internal class GUIToggle : GUIObject
    {
        public enum ToggleTypeEnum { Image, Tab, Fade };
        private readonly ToggleTypeEnum _toggleType;

        public bool Selected { get; private set; }
        readonly GUIImage _gUnselected;
        readonly GUIImage _gSelected;
        readonly GUIImage _gIcon;
        GUIToggle[] toggleGroup;

        readonly EmptyDelegate _delAction;
        public GUIToggle(Rectangle icon, ToggleTypeEnum toggleType, string texture, EmptyDelegate del)
        {
            _delAction = del;
            _toggleType = toggleType;

            _gIcon = new GUIImage(icon, texture);
            if (toggleType == ToggleTypeEnum.Tab)
            {
                _gUnselected = new GUIImage(GUIUtils.TAB_UNSELECTED, texture);
                _gSelected = new GUIImage(GUIUtils.TAB_SELECTED, texture);

                _gSelected.MoveBy(0, _gUnselected.Height - _gSelected.Height);

                Width = _gUnselected.Width;
                Height = _gUnselected.Height;
                AddControls(_gSelected, _gUnselected);
            }
            else
            {
                Width = _gIcon.Width;
                Height = _gIcon.Height;
            }

            AddControl(_gIcon);
            Select(false);
        }
        public GUIToggle(Rectangle unselected, Rectangle selected, Rectangle icon, string texture, EmptyDelegate del)
        {
            _delAction = del;
            _toggleType = ToggleTypeEnum.Image;

            _gUnselected = new GUIImage(unselected, texture);
            _gSelected = new GUIImage(selected, texture);
            _gIcon = new GUIImage(icon, texture);

            Width = _gUnselected.Width;
            Height = _gUnselected.Height;

            AddControls(_gSelected, _gUnselected, _gIcon);

            Select(false);
        }

        internal override void Show(bool val)
        {
            Visible = val;
        }

        public override bool Contains(Point mouse)
        {
            bool unselectedHit = _gUnselected != null && _gUnselected.Contains(mouse);
            return unselectedHit || base.Contains(mouse);
        }
        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Active && !Selected && Contains(mouse))
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
            _gSelected?.Show(Selected);
            _gUnselected?.Show(!Selected);

            if (_toggleType == ToggleTypeEnum.Tab)
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
            else if(_toggleType == ToggleTypeEnum.Fade)
            {
                _gIcon.Alpha(Selected ? 1f : 0.5f);
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
