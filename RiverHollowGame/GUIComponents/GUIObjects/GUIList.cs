using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIList : GUIObject
    {
        //Passes all action handlers to it's sub controls.
        public int _iMaxShownItems;

        int _iListPos = 0;
        int _iSpacing;

        GUIButton _btnUp;
        GUIButton _btnDown;
        public List<GUIObject> Objects { get; }

        /// <summary>
        /// The constructor for GUIList. GUILists will display a subset of an array of GUIObjects and allow for scrolling if necessary.
        /// </summary>
        /// <param name="objects">The array of GUIObjects to be displayed by the GUIList</param>
        /// <param name="maxItems">The maximum number of objects that should be displayed in the List at any given time.</param>
        /// <param name="spacing">The amount of space (in px) to set inbetween items in the List.</param>
        /// <param name="maxHeight">The maximum height the GUIList is allowed to take up on the screen. If set, it will override maxItems if necessary.</param>
        public GUIList(List<GUIObject> objects, int maxItems, int spacing, int maxHeight = 0)
        {
            _iMaxShownItems = maxItems;
            Objects = objects;
            _iSpacing = spacing;
            int ScaledSpacing = GameManager.ScaleIt(spacing);

            int mostWidth = 0;
            int mostHeight = 0;
            foreach (GUIObject o in Objects)
            {
                if (o.Height > mostHeight)
                {
                    mostHeight = o.Height;
                }
                if (o.Width > mostWidth)
                {
                    mostWidth = o.Width;
                }
                AddControl(o);
            }

            int calcHeight = (mostHeight * maxItems) + (ScaledSpacing * (maxItems - 1));

            //the following set of if statmenets handles if the maxHeight parameter is used.
            if (maxHeight > 0)
            {
                if (calcHeight > maxHeight)
                {
                    int calcItems = maxHeight / (mostHeight + ScaledSpacing);
                    calcHeight = (mostHeight * calcItems) + (ScaledSpacing * (calcItems - 1));
                    _iMaxShownItems = calcItems;
                    Debug.Assert(calcHeight <= maxHeight);
                }
            }

            Width = mostWidth;
            Height = calcHeight;

            PopulateList();

            if (_iMaxShownItems < objects.Count)
            {
                Width = mostWidth + ScaledTileSize;

                _btnUp = new GUIButton(new Rectangle(272, 96, 16, 16), DataManager.DIALOGUE_TEXTURE, BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 16, 16), DataManager.DIALOGUE_TEXTURE, BtnDownClick);

                _btnUp.AnchorToInnerSide(this, SideEnum.TopRight);
                _btnDown.AnchorToInnerSide(this, SideEnum.BottomRight);

                _btnUp.Show(false);
            }
        }

        public GUIObject GetMousedOverEntry(Point mouse)
        {
            GUIObject rv = null;

            foreach(GUIObject obj in Objects)
            {
                if (obj.Contains(mouse))
                {
                    rv = obj;
                    break;
                }
            }

            return rv;
        }

        public List<GUIObject> GetEntries()
        {
            return Objects;
        }

        public void BtnUpClick()
        {
            if (_iListPos > 0) {
                _iListPos--;
                _btnDown.Show(true);
            }

            if (_iListPos == 0) { _btnUp.Show(false); }

            PopulateList();
        }
        public void BtnDownClick()
        {
            if (_iListPos < Objects.Count - _iMaxShownItems) {
                _iListPos++;
                _btnUp.Show(true);
            }

            if (_iListPos == Objects.Count - _iMaxShownItems) { _btnDown.Show(false); }

            PopulateList();
        }

        /// <summary>
        /// Handles the population of the to be displayed objects to the screen canvas.
        /// Should be called whenever the list of what should be displayed in the object changes.
        /// </summary>
        private void PopulateList()
        {
            foreach (GUIObject o in Objects)
            {
                o.Show(false);
            }

            for (int i = _iListPos; i < _iListPos + _iMaxShownItems && i < Objects.Count; i++)
            {
                GUIObject o = Objects[i];

                if (i == _iListPos) { o.AnchorToInnerSide(this, GUIObject.SideEnum.Top, _iSpacing); }
                else { o.AnchorAndAlignWithSpacing(Objects[i - 1], SideEnum.Bottom, SideEnum.Left, _iSpacing); }

                o.Show(true);
            }
        }
    }
}