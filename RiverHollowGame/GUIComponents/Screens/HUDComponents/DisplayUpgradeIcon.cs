using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using RiverHollow.Misc;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Utilities.Constants;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    internal abstract class DisplayUpgradeIcon : GUIImage
    {
        protected const float FADE = 0.8f;

        public bool Locked { get; protected set; }
        public int Priority { get; protected set; }
        protected readonly IntDelegate _delAction;

        public DisplayUpgradeIcon(IntDelegate btnClick) : base(GUIUtils.CODEX_ITEM)
        {
            HoverControls = false;
            _delAction = btnClick;
        }

        protected void MakeIcon(Point iconPosition)
        {
            iconPosition = Util.MultiplyPoint(iconPosition, Constants.TILE_SIZE);

            var iconRect = new Rectangle(iconPosition, Constants.BASIC_TILE);
            GUIImage spr = new GUIImage(iconRect, DataManager.UPGRADE_ICONS);
            spr.CenterOnObject(this);
            AddControl(spr);

            if (Locked)
            {
                spr.SetColor(Color.Black * FADE);
            }
        }
    }
    internal class DisplayBuildingUpgradeIcon : DisplayUpgradeIcon
    {
        public int BuildingID { get; }

        public DisplayBuildingUpgradeIcon(int buildingID, IntDelegate btnClick) : base(btnClick)
        {
            BuildingID = buildingID;
            Locked = TownManager.GetBuildingByID(buildingID) == null;

            var upgradeData = DataManager.GetStringParamsByIDKey(buildingID, "Upgradable", DataType.WorldObject);
            Priority = int.Parse(upgradeData[0]);

            MakeIcon(Util.ParsePoint(upgradeData[1]));
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                _delAction(BuildingID);
            }

            return rv;
        }

        protected override void BeginHover()
        {
            var infoWindow = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL);

            string strText = Locked ? "???" : DataManager.GetTextData(BuildingID, "Name", DataType.WorldObject);
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(infoWindow, SideEnum.TopLeft);

            if (!Locked)
            {
                string strDescText = DataManager.GetTextData(BuildingID, "Description", DataType.WorldObject);
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
    internal class DisplayGlobalUpgradeIcon : DisplayUpgradeIcon
    {
        public int UpgradeID { get; }
        public DisplayGlobalUpgradeIcon(Upgrade upgrade, IntDelegate btnClick) : base(btnClick)
        {
            UpgradeID = upgrade.ID;
            Locked = upgrade.Status == UpgradeStatusEnum.Locked;
            Priority = upgrade.Priority;

            MakeIcon(upgrade.Icon);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                _delAction(UpgradeID);
            }

            return rv;
        }

        protected override void BeginHover()
        {
            var infoWindow = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL);

            string strText = Locked ? "???" : DataManager.GetTextData(UpgradeID, "Name", DataType.Upgrade);
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(infoWindow, SideEnum.TopLeft);

            if (!Locked)
            {
                string strDescText = DataManager.GetTextData(UpgradeID, "Description", DataType.Upgrade);
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
