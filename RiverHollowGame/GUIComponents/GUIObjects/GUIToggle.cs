using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects
{
    internal class GUIToggle : GUIObject
    {
        public bool Selected { get; private set; }
        GUIImage _gUnselected;
        GUIImage _gSelected;

        EmptyDelegate _delAction;

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
            if (Contains(mouse) && !Selected && Active && _delAction != null)
            {
                rv = true;
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
    }
}
