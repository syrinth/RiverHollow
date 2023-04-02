using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects
{
    internal class GUIToggle : GUIObject
    {
        public bool Selected { get; private set; }
        GUIImage _gUnselected;
        GUIImage _gSelected;
        GUIToggle[] toggleGroup;

        EmptyDelegate _delAction;
        public GUIToggle(Rectangle unselected, Rectangle selected, string texture, EmptyDelegate del)
        {
            _delAction = del;

            _gUnselected = new GUIImage(unselected, texture);
            _gSelected = new GUIImage(selected, texture);
            _gSelected.MoveBy(0, _gUnselected.Height - _gSelected.Height);

            Width = _gUnselected.Width;
            Height = _gUnselected.Height;

            AddControl(_gSelected);
            AddControl(_gUnselected);

            Select(false);
        }
        public GUIToggle(Point unselected, Point selected, Point size, string texture, EmptyDelegate del)
        {
            _delAction = del;


            Width = GameManager.ScaleIt(size.X);
            Height = GameManager.ScaleIt(size.Y);
            _gUnselected = new GUIImage(new Rectangle(unselected, size), Width, Height, texture);
            _gSelected = new GUIImage(new Rectangle(selected, size), Width, Height, texture);

            AddControl(_gUnselected);
            AddControl(_gSelected);

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

        public void Select(bool value)
        {
            Selected = value;
            _gSelected.Show(Selected);
            _gUnselected.Show(!Selected);
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
        public void AssignToggleGroup(params GUIToggle[] toggles)
        {
            toggleGroup = new GUIToggle[toggles.Length];
            for (int i = 0; i < toggles.Length; i++)
            {
                toggleGroup[i] = toggles[i];
                toggles[i].AssignToggleGroup(this, toggles);
            }

            Select(true);
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
