using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    internal class DisplayUpgradeWindow : GUIImage
    {
        public int ID { get; }
        public int Priority { get; }
        public bool Found { get; private set; }
        const float FADE = 0.8f;

        private IntDelegate _delAction;
        public DisplayUpgradeWindow(int buildingID, IntDelegate btnclick) : base(GUIUtils.CODEX_ITEM)
        {
            HoverControls = false;
            _delAction = btnclick;

            ID = buildingID;
            Found = TownManager.GetBuildingByID(buildingID) != null;

            var upgradeData = DataManager.GetStringParamsByIDKey(buildingID, "Upgradable", DataType.WorldObject);
            Priority = int.Parse(upgradeData[0]);

            var iconRect = Util.ParseRectangle(upgradeData[1]);
            GUIImage spr = new GUIImage(iconRect, DataManager.UPGRADE_ICONS);
            spr.CenterOnObject(this);
            AddControl(spr);

            if (!Found)
            {
                spr.SetColor(Color.Black * FADE);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                _delAction(ID);
            }

            return rv;
        }

        protected override void BeginHover()
        {
            var infoWindow = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL);

            string strText = Found ? DataManager.GetTextData(ID, "Name", DataType.WorldObject) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(infoWindow, SideEnum.TopLeft);

            if (Found)
            {
                string strDescText = DataManager.GetTextData(ID, "Description", DataType.WorldObject);
                if (!string.IsNullOrEmpty(strDescText))
                {
                    GUIText descText = new GUIText(strDescText);
                    descText.AnchorAndAlignWithSpacing(text, SideEnum.Bottom, SideEnum.Left, 2);
                }
            }

            infoWindow.Resize(false);
            infoWindow.AnchorAndAlignWithSpacing(this, SideEnum.Bottom, SideEnum.CenterX, -1, GUIUtils.ParentRuleEnum.Skip);
            text.AlignToObject(infoWindow, SideEnum.CenterX);

            GUIManager.OpenHoverObject(infoWindow, DrawRectangle, true);
        }
    }
}
