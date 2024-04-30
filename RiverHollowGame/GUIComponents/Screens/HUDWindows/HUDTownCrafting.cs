using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens.HUDWindows.TabPages;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    internal class HUDTownCrafting : GUITabWindow
    {
        public HUDTownCrafting() : base(186, 152)
        {
            AddTab(ShowStructures, GUIUtils.TAB_STRUCTURE_ICON);
            AddTab(ShowFlooring, GUIUtils.TAB_FLOOR_ICON);
            AddTab(ShowWalls, GUIUtils.TAB_WALL_ICON);
            AddTab(ShowFurniture, GUIUtils.TAB_FURNITURE_ICON);
            AddTab(ShowLighting, GUIUtils.TAB_LIGHTING_ICON);

            _gTabToggles[0].AssignToggleGroup(true, _gTabToggles.Where(x => x != _gTabToggles[0]).ToArray());
        }

        #region PageToggles
        public void ShowStructures()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownStructurePage(_winMain));
        }
        public void ShowFlooring()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownCraftingFloors(_winMain));
        }
        public void ShowWalls()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownCraftingWalls(_winMain));
        }
        public void ShowFurniture()
        {
            CleanTabWindow();
            ShowTabPage(new HUDTownCraftingFurniture(_winMain));
        }
        public void ShowLighting()
        {
        }
        #endregion
    }
}

