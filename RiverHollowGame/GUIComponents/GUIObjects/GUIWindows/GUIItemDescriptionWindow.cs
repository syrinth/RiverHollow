using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
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
            gName.SetTextColors(Color.White, GUIUtils.DESCRIPTION_HEADER_SHADOW);
            gName.PositionAndMove(top, 29, 5);

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

            var strTypeColor = GetItemStats(it, out var typeColor);

            gDescription.PositionAndMove(middle, 4, 0);

            var gType = new GUIText(strTypeColor);
            gType.SetTextColors(typeColor, GUIUtils.DESCRIPTION_HEADER_SHADOW);
            gType.PositionAndMove(top, 29, 15);

            var middleTemp = middle;
            var details = it.GetDetails();
            if (details != null)
            {
                var middleIcons = new GUIImage(GUIUtils.HUD_DESC_MID);
                middleIcons.AnchorAndAlign(middle, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);
                middleIcons.Height = 9 * GameManager.CurrentScale;

                var icons = new List<GUIIconText>();
                foreach (var iconData in details)
                {
                    var source = Rectangle.Empty;
                    switch (iconData.Item1)
                    {
                        case GameIconEnum.Health:
                            source = GUIUtils.ICON_DESC_HEALTH;
                            break;
                        case GameIconEnum.Energy:
                            source = GUIUtils.ICON_DESC_ENERGY;
                            break;
                        case GameIconEnum.Time:
                            source = GUIUtils.ICON_DESC_TIME;
                            break;
                        case GameIconEnum.Level:
                            source = GUIUtils.ICON_DESC_LEVEL;
                            break;
                        case GameIconEnum.Coin:
                            source = GUIUtils.ICON_DESC_COIN;
                            break;
                    }

                    icons.Add(new GUIIconText(iconData.Item2.ToString(), 1, source, iconData.Item1, SideEnum.Left, SideEnum.CenterY, DataManager.FONT_NUMBERS));
                }

                icons.Reverse();
                for(int i =0; i < icons.Count; i++)
                {
                    var icon = icons[i];
                    icon.SetColor(Color.White);
                    middleIcons.AddControl(icon);

                    if(i == 0)
                    {
                        //icon.AnchorAndAlignWithSpacing(middle, SideEnum.Bottom, SideEnum.Right, 2);
                        icon.AnchorAndAlign(middle, SideEnum.Bottom, SideEnum.Right);
                        icon.ScaledMoveBy(-5, 0);
                    }
                    else
                    {
                        icon.AnchorAndAlignWithSpacing(icons[i-1], SideEnum.Left, SideEnum.Top, 3);
                    }
                }

                middleTemp = middleIcons;
            }

            //BOTTOM
            var bottom = new GUIImage(GUIUtils.HUD_DESC_BOT);
            bottom.AnchorAndAlign(middleTemp, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);

            DetermineSize();
        }

        private string GetItemStats(Item it, out Color typeColor)
        {
            typeColor = Color.White;
            var rv = Util.GetEnumString(it.ItemType);
            if (it.ItemType == ItemTypeEnum.Resource)
            {
                var itemGroup = it.GetEnumByIDKey<ResourceTypeEnum>("Subtype");
                if (itemGroup != ResourceTypeEnum.None)
                {
                    switch (itemGroup)
                    {
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
            else if (it is Relic)
            {
                typeColor = Color.Gold;
            }
            else
            {
                rv = Util.GetEnumString(it.GetEnumByIDKey<BuildableEnum>("Subtype"));
            }

            return rv;
        }
    }
}
