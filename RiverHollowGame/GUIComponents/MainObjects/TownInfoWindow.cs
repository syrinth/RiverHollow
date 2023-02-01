using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.MainObjects
{
    class TownInfoWindow : GUIMainObject
    {
        public TownInfoWindow()
        {
            _winMain = SetMainWindow();
            GUIText text = new GUIText("Town Score: " + TownManager.GetTownScore());
            text.AnchorToInnerSide(_winMain, SideEnum.Top);

            int plants = 0;
            foreach (KeyValuePair<int, List<WorldObject>> kvp in TownManager.GetTownObjects())
            {
                switch (kvp.Value[0].Type)
                {
                    case ObjectTypeEnum.Plant:
                        plants += kvp.Value.Count;
                        break;
                }
            }

            Anchor(ref text, "Buildings: 0");// + PlayerManager.BuildingList.Count);
            Anchor(ref text, "Plants: " + plants);
        }

        private void Anchor(ref GUIText t, string str)
        {
            GUIText temp = t;
            t = new GUIText(str);
            t.AnchorToInnerSide(_winMain, SideEnum.Left);
            t.AnchorToObject(temp, SideEnum.Bottom);
        }
    }
}
