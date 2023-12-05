using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using RiverHollow.GUIComponents.Screens.HUDWindows.TabPages;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    internal class HUDCodex : GUITabWindow
    {
        public HUDCodex() : base(186, 177)
        {
            AddTab(ShowVillagers, GUIUtils.TAB_VILLAGER_ICON);
            AddTab(ShowMerchants, GUIUtils.TAB_MERCHANT_ICON);
            AddTab(ShowTravelers, GUIUtils.TAB_TRAVELER_ICON);
            AddTab(ShowMobs, GUIUtils.TAB_MOB_ICON);
            AddTab(ShowItems, GUIUtils.TAB_ITEM_ICON);

            _gTabToggles[0].AssignToggleGroup(true, _gTabToggles.Where(x => x != _gTabToggles[0]).ToArray()); 
        }

        #region PageToggles
        public void ShowVillagers()
        {
            CleanTabWindow();
            ShowTabPage(new HUDCodexVillagers(_winMain));
        }
        public void ShowMerchants()
        {
            CleanTabWindow();
            ShowTabPage(new HUDCodexMerchants(_winMain));
        }
        public void ShowTravelers()
        {
            CleanTabWindow();
            ShowTabPage(new HUDCodexTravelers(_winMain));
        }
        public void ShowMobs()
        {
            CleanTabWindow();
            ShowTabPage(new HUDCodexMobs(_winMain));
        }
        public void ShowItems()
        {
            CleanTabWindow();
            ShowTabPage(new HUDCodexItems(_winMain));
        }
        #endregion
    }
}
