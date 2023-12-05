using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDTownHallOverview : GUIObject
    {

        public HUDTownHallOverview(GUIWindow mainWindow)
        {
            var townScore = new GUIText("Town Score: " + TownManager.GetTownScore());
            townScore.AnchorToInnerSide(mainWindow, SideEnum.Top);

            int plantcount = 0;
            foreach (KeyValuePair<int, List<WorldObject>> kvp in TownManager.GetTownObjects())
            {
                switch (kvp.Value[0].Type)
                {
                    case ObjectTypeEnum.Plant:
                        plantcount += kvp.Value.Count;
                        break;
                }
            }

            var buildings = new GUIText(string.Format("Buildings: 0"));
            buildings.AnchorAndAlignWithSpacing(townScore, SideEnum.Bottom, SideEnum.Left, 10);

            var plants = new GUIText(string.Format("Plants: {0}", plantcount));
            plants.AnchorAndAlignWithSpacing(buildings, SideEnum.Bottom, SideEnum.Left, 10);
        }
    }
}
