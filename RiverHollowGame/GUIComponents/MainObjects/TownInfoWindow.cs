using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.MainObjects
{
    class TownInfoWindow : GUIMainObject
    {
        public TownInfoWindow()
        {
            _winMain = SetMainWindow();
            GUIText text = new GUIText("Town Score: " + PlayerManager.GetTownScore());
            text.AnchorToInnerSide(_winMain, SideEnum.Top);

            int plants = 0;
            foreach (KeyValuePair<int, List<WorldObject>> kvp in PlayerManager.GetTownObejcts())
            {
                switch (kvp.Value[0].Type)
                {
                    case GameManager.ObjectTypeEnum.Plant:
                        plants += kvp.Value.Count;
                        break;
                }
            }

            Anchor(ref text, "Buildings: " + PlayerManager.BuildingList.Count);
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
