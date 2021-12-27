using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class List : GUIObject
    {
        //Passes all action handlers to it's sub controls.
        public static int BTNSIZE = ScaledTileSize;
        public int _iMaxShownItems;

        int _iListPos = 0;
        int _iSpacing;

        GUIButton _btnUp;
        GUIButton _btnDown;
        List<GUIObject> _liObjects;

        /// <summary>
        /// The constructor for GUIList. GUILists will display a subset of an array of GUIObjects and allow for scrolling if necessary.
        /// </summary>
        /// <param name="objects">The array of GUIObjects to be displayed by the GUIList</param>
        /// <param name="maxItems">The maximum number of objects that should be displayed in the List at any given time.</param>
        /// <param name="spacing">The amount of space (in px) to set inbetween items in the List.</param>
        /// <param name="maxHeight">The maximum height the GUIList is allowed to take up on the screen. If set, it will override maxItems if necessary.</param>
        public List(List<GUIObject> objects, int maxItems, int spacing, int maxHeight = 0)
        {
            _iMaxShownItems = maxItems;
            _liObjects = objects;
            _iSpacing = spacing;

            int mostWidth = 0;
            int mostHeight = 0;
            foreach (GUIObject o in _liObjects)
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

            int calcHeight = (mostHeight * maxItems) + (spacing * (maxItems - 1));

            //the following set of if statmenets handles if the maxHeight parameter is used.
            if (maxHeight > 0)
            {
                if (calcHeight > maxHeight)
                {
                    int calcItems = maxHeight / (mostHeight + spacing);
                    calcHeight = (mostHeight * calcItems) + (spacing * (calcItems - 1));
                    _iMaxShownItems = calcItems;
                    Debug.Assert(calcHeight <= maxHeight);
                }
            }

            this.Height = calcHeight;
            this.Width = mostWidth;

            PopulateList();

            if (_iMaxShownItems < objects.Count)
            {
                this.Width = mostWidth + BTNSIZE;

                _btnUp = new GUIButton(new Rectangle(272, 96, 16, 16), BTNSIZE, BTNSIZE, DataManager.DIALOGUE_TEXTURE, BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 16, 16), BTNSIZE, BTNSIZE, DataManager.DIALOGUE_TEXTURE, BtnDownClick);

                _btnUp.AnchorToInnerSide(this, SideEnum.TopRight);
                _btnDown.AnchorToInnerSide(this, SideEnum.BottomRight);

                AddControl(_btnUp);
                AddControl(_btnDown);

                _btnUp.Show(false);
            }
        }

        public GUIObject GetMousedOverEntry(Point mouse)
        {
            GUIObject rv = null;

            foreach(GUIObject obj in _liObjects)
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
            return _liObjects;
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
            if (_iListPos < _liObjects.Count - _iMaxShownItems) {
                _iListPos++;
                _btnUp.Show(true);
            }

            if (_iListPos == _liObjects.Count - _iMaxShownItems) { _btnDown.Show(false); }

            PopulateList();
        }

        /// <summary>
        /// Handles the population of the to be displayed objects to the screen canvas.
        /// Should be called whenever the list of what should be displayed in the object changes.
        /// </summary>
        private void PopulateList()
        {
            Vector2 position = GetAnchorToInnerSide(this, GUIObject.SideEnum.Top);

            foreach (GUIObject o in _liObjects)
            {
                o.Show(false);
            }

            for (int s = _iListPos; s < _iListPos + _iMaxShownItems && s < _liObjects.Count; s++)
            {
                GUIObject o = _liObjects[s];

                o.Position(position);
                position.Y += o.Height + _iSpacing;
                o.Show(true);
            }
        }
    }
}