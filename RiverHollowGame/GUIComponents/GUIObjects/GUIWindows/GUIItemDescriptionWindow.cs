using Microsoft.Xna.Framework;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
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
            if (it.ItemType != ItemEnum.Buildable)
            {
                var itemGroup = it.GetEnumByIDKey<ItemGroupEnum>("Subtype");
                if (itemGroup != ItemGroupEnum.None)
                {
                    switch (itemGroup)
                    {
                        case ItemGroupEnum.Artifact:
                            typeColor = Color.Gold; break;
                        case ItemGroupEnum.Clothing:
                            typeColor = Color.DarkBlue; break;
                        case ItemGroupEnum.Gem:
                            typeColor = Color.DeepPink; break;
                        case ItemGroupEnum.Fish:
                            typeColor = Color.Blue; break;
                        case ItemGroupEnum.Herb:
                            typeColor = Color.Green; break;
                        case ItemGroupEnum.Ingredient:
                            typeColor = Color.DarkOrange; break;
                        case ItemGroupEnum.Magic:
                            typeColor = Color.Purple; break;
                        case ItemGroupEnum.Potion:
                            typeColor = Color.DarkGreen; break;
                        case ItemGroupEnum.MonsterPart:
                            typeColor = Color.DarkRed; break;
                        case ItemGroupEnum.Ore:
                            typeColor = Color.Silver; break;
                    }
                    strType = Util.GetEnumString(itemGroup, true);
                }
            }
            else
            {
                strType = Util.GetEnumString(it.GetEnumByIDKey<BuildableEnum>("Subtype"));
            }
            Setup(it.Name(), strType, typeColor, it.GetDetails(), it.Description());
        }

        public GUIItemDescriptionWindow(WorldObject it) : base(GUIUtils.WINDOW_BROWN)
        {
            Setup(it.Name(), Util.GetEnumString(it.GetEnumByIDKey<BuildableEnum>("Subtype")), Color.Black, string.Empty, it.Description());
        }

        private void Setup(string name, string type, Color typeColor, string details, string description)
        {
            var gName = new GUIText(name);
            gName.AnchorToInnerSide(this, SideEnum.TopLeft);

            var gType = new GUIText(type);
            gType.SetColor(typeColor);
            gType.AnchorAndAlignWithSpacing(gName, SideEnum.Bottom, SideEnum.Left, 2);

            GUIText gDetails = null;
            if(!string.IsNullOrEmpty(details))
            {
                gDetails = new GUIText(details);
            }

            var gDescription = new GUIText(description);
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

            DetermineSize();
        }
    }
}
