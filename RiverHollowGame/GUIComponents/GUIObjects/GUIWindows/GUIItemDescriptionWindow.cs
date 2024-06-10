using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIItemDescriptionWindow : GUIObject
    {
        readonly int MAX_TEXT_WIDTH = 166 * GameManager.CurrentScale;

        public GUIItemDescriptionWindow(Item it) : base()
        {
            //HEADER
            var top = new GUIImage(GUIUtils.HUD_DESC_TOP);
            AddControl(top);

            var gItem = new GUIItem(it, ItemBoxDraw.Never);
            gItem.PositionAndMove(top, 7, 7);
            
            var gName = new GUIText(it.Name());
            gName.PositionAndMove(top, 29, 7);

            if (it is Merchandise m)
            {
                var classIcon = GUIUtils.GetClassIcon(m.ClassType);
                classIcon.PositionAndMove(top, 161, 5);
            }

            //MID
            var gDescription = new GUIText(it.Description());
            gDescription.ParseAndSetText(gDescription.Text, MAX_TEXT_WIDTH, 5, true);

            var middle = new GUIImage(GUIUtils.HUD_DESC_MID);
            middle.AnchorAndAlign(top, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);
            middle.Height = gDescription.Height + GameManager.CurrentScale;

            var strType = GetItemStats(it, out var typeColor);

            gDescription.PositionAndMove(middle, 4, 0);

            var gType = new GUIText(strType);
            gType.SetColor(typeColor);
            gType.PositionAndMove(top, 29, 17);
             
            var details = it.GetDetails();
            if (!string.IsNullOrEmpty(details))
            {
                GUIText gDetails = new GUIText(details);
                gDetails.PositionAndMove(top, 100, 7);
            }

            //BOTTOM
            var bottom = new GUIImage(GUIUtils.HUD_DESC_BOT);
            bottom.AnchorAndAlign(middle, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);

            DetermineSize();
        }

        private string GetItemStats(Item it, out Color typeColor)
        {
            typeColor = Color.Black;
            var rv = Util.GetEnumString(it.ItemType);
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
                    rv = Util.GetEnumString(itemGroup, true);
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
                    rv = Util.GetEnumString(merchGroup, true);
                }
            }
            else
            {
                rv = Util.GetEnumString(it.GetEnumByIDKey<BuildableEnum>("Subtype"));
            }

            return rv;
        }
    }
}
