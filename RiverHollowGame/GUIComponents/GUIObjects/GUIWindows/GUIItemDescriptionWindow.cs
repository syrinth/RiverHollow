using Microsoft.Xna.Framework;
using RiverHollow.Items;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIItemDescriptionWindow : GUIWindow
    {
        public GUIItemDescriptionWindow(Item it) : base(GUIUtils.WINDOW_BROWN)
        {
            Color typeColor = Color.Black;
            var strType = Util.GetEnumString(it.ItemType);
            if (it.ItemType == ItemTypeEnum.Resource)
            {
                var itemGroup = it.GetEnumByIDKey<ResourceTypeEnum>("Subtype");
                if (itemGroup != ResourceTypeEnum.None)
                {
                    switch (itemGroup)
                    {
                        case ResourceTypeEnum.Artifact:
                            typeColor = Color.Gold; break;
                        case ResourceTypeEnum.Gem:
                            typeColor = Color.DeepPink; break;
                        case ResourceTypeEnum.Fish:
                            typeColor = Color.Blue; break;
                        case ResourceTypeEnum.Herb:
                            typeColor = Color.Green; break;
                        case ResourceTypeEnum.Ingredient:
                            typeColor = Color.DarkOrange; break;
                        case ResourceTypeEnum.MonsterPart:
                            typeColor = Color.DarkRed; break;
                        case ResourceTypeEnum.Ore:
                            typeColor = Color.Silver; break;
                    }
                    strType = Util.GetEnumString(itemGroup, true);
                }
            }
            else if (it is Merchandise merchItem)
            {
                var merchGroup = merchItem.MerchType;
                if (merchGroup != MerchandiseTypeEnum.Generic)
                {
                    switch (merchGroup)
                    {
                        case MerchandiseTypeEnum.Magic:
                            typeColor = Color.Purple; break;
                        case MerchandiseTypeEnum.Potion:
                            typeColor = Color.DarkGreen; break;
                        case MerchandiseTypeEnum.Clothing:
                            typeColor = Color.DarkBlue; break;
                    }
                    strType = Util.GetEnumString(merchGroup, true);
                }
            }
            else
            {
                strType = Util.GetEnumString(it.GetEnumByIDKey<BuildableEnum>("Subtype"));
            }

            var gName = new GUIText(it.Name());
            gName.AnchorToInnerSide(this, SideEnum.TopLeft);

            var gType = new GUIText(strType);
            gType.SetColor(typeColor);
            gType.AnchorAndAlignWithSpacing(gName, SideEnum.Bottom, SideEnum.Left, 2);

            GUIText gDetails = null;
            var details = it.GetDetails();
            if (!string.IsNullOrEmpty(details))
            {
                gDetails = new GUIText(details);
            }

            var gDescription = new GUIText(it.Description());
            gDescription.ParseAndSetText(gDescription.Text, Constants.MAX_ITEM_DESC_SIZE - WidthEdges(), 10, true);

            var gBar = new GUIImage(GUIUtils.HUD_DIVIDER, gDescription.Width, ScaleIt(1));
            gBar.AnchorAndAlignWithSpacing(gType, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);

            if (gDetails != null)
            {
                gDetails.AnchorAndAlignWithSpacing(gBar, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);
                gDescription.AnchorAndAlignWithSpacing(gDetails, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);
            }
            else
            {
                gDescription.AnchorAndAlignWithSpacing(gBar, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);
            }

            DetermineHeight();
            Width = Constants.MAX_ITEM_DESC_SIZE;

            if (it is Merchandise m)
            {
                GUIImage classIcon = GUIUtils.GetClassIcon(m.ClassType);
                classIcon.AnchorToInnerSide(this, SideEnum.TopRight, 1);
            }
        }
    }
}
