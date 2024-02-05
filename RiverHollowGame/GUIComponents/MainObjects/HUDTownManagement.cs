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

        private void ShowOverview()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownHallOverview(_winMain));
        }

        private void ShowBuildingUpgrades()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownHallBuildingUpgradePage(_winMain));
        }

        private void ShowTownUpgrades()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownHallGlobalUpgradePage(_winMain));
        }
    }
}
