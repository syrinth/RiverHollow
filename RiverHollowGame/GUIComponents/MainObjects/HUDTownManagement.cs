using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.GUIComponents.Screens.HUDWindows.TabPages;
using System.Linq;

namespace RiverHollow.GUIComponents.MainObjects
{
    class HUDTownManagement : GUITabWindow
    {
        public HUDTownManagement() : base(186, 177)
        {
            AddTab(ShowOverview, GUIUtils.TAB_OVERVIEW_ICON);
            AddTab(ShowBuildingUpgrades, GUIUtils.TAB_STRUCTURE_ICON);
            AddTab(ShowTownUpgrades, GUIUtils.TAB_TOWN_UPGRADE_ICON);

            _gTabToggles[0].AssignToggleGroup(true, _gTabToggles.Where(x => x != _gTabToggles[0]).ToArray());
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if(_gTabObject != null)
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

        private void ShowOverview()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownHallOverview(_winMain));
        }

        private void ShowBuildingUpgrades()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownHallUpgradePage(_winMain));
        }

        private void ShowTownUpgrades()
        {
            CleanTabWindow();
        }
    }
}
