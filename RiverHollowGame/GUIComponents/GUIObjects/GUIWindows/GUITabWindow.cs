using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    internal class GUITabWindow : GUIMainObject
    {
        protected GUIObject _gTabObject;
        protected List<GUIToggle> _gTabToggles;

        protected int _ePageIndex = 0;

        public GUITabWindow(int width, int height)
        {
            _winMain = SetMainWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(width), GameManager.ScaleIt(height));
            _gTabToggles = new List<GUIToggle>();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_gTabObject != null)
            {
                rv = _gTabObject.ProcessLeftButtonClick(mouse);
            }

            if (!rv)
            {
                rv = base.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (_gTabObject != null)
            {
                rv = _gTabObject.ProcessRightButtonClick(mouse);
            }

            if (!rv)
            {
                rv = base.ProcessRightButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (_gTabObject != null)
            {
                rv = _gTabObject.ProcessHover(mouse);
            }

            if (!rv)
            {
                rv = base.ProcessHover(mouse);
            }

            return rv;
        }

        protected void AddTab(EmptyDelegate del, Rectangle icon)
        {
            var index = _gTabToggles.Count;
            _gTabToggles.Add(new GUIToggle(icon, DataManager.HUD_COMPONENTS, del));
            AddControl(_gTabToggles[index]);
            if (index == 0)
            {
                _gTabToggles[index].PositionAndMove(_winMain, 10, -16);
            }
            else
            {
                _gTabToggles[index].AnchorAndAlign(_gTabToggles[index - 1], SideEnum.Right, SideEnum.Bottom);
            }

        }

        public void CleanTabWindow()
        {
            var copy = new List<GUIObject>(_winMain.Controls);
            foreach(GUIObject g in copy)
            {
                if(!_gTabToggles.Contains(g))
                {
                    _winMain.RemoveControl(g);
                }
            }
        }

        protected void ShowTabPage(GUIObject obj)
        {
            RemoveControl(_gTabObject);
            _gTabObject = obj;
            AddControl(_gTabObject);
        }
    }
}
