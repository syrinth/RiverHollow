using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;
using System.Diagnostics;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIList : GUIObject
    {
        public static int BTNSIZE = ScaledTileSize;
        public static int MAX_SHOWN_ITEMS;

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
        public GUIList(List<GUIObject> objects, int maxItems, int spacing, int maxHeight=0)
        {
            MAX_SHOWN_ITEMS = maxItems;
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

            int calcHeight = (mostHeight*maxItems) + (spacing*(maxItems-1));

            //the following set of if statmenets handles if the maxHeight parameter is used.
            if(maxHeight > 0)
            {
                if(calcHeight > maxHeight)
                {
                    int calcItems = maxHeight / (mostHeight + spacing);
                    calcHeight = (mostHeight * calcItems) + (spacing * (calcItems - 1));
                    MAX_SHOWN_ITEMS = calcItems;
                    Debug.Assert(calcHeight <= maxHeight);
                }
            }

            this.Height = calcHeight;
            this.Width = mostWidth; //The buttons created below are technically display outside of the GUIList object. Still functions but this could break in the future.

            PopulateList();

            if (MAX_SHOWN_ITEMS < objects.Count)
            {
                _btnUp = new GUIButton(new Rectangle(272, 96, 16, 16), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 16, 16), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnDownClick);

                _btnUp.AnchorAndAlignToObject(this, GUIObject.SideEnum.Right, GUIObject.SideEnum.Top);
                _btnDown.AnchorAndAlignToObject(this, GUIObject.SideEnum.Right, GUIObject.SideEnum.Bottom);

                AddControl(_btnUp);
                AddControl(_btnDown);
            }
        }

        public void BtnUpClick()
        {
            if (_iListPos > 0) { _iListPos--; }
            PopulateList();
        }
        public void BtnDownClick()
        {
            if (_iListPos < _liObjects.Count - MAX_SHOWN_ITEMS) { _iListPos++; }
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
                o.Show = false;
            }

            for (int s = _iListPos; s < _iListPos + MAX_SHOWN_ITEMS && s < _liObjects.Count; s++)
            {
                GUIObject o = _liObjects[s];

                o.Position(position);
                position.Y += o.Height + _iSpacing;
                o.Show = true;
            }
        }
    }
}