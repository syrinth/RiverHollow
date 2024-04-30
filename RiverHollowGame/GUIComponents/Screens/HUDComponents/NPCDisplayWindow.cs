using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    internal class NPCDisplayWindow : GUIWindow
    {
        public int ID { get; }
        public bool Found { get; private set; }
        const float FADE = 0.8f;

        public NPCDisplayWindow(Actor npc, bool showHeart = true) : base(GUIUtils.WINDOW_CODEX_NPC, GameManager.ScaleIt(32), GameManager.ScaleIt(44))
        {
            HoverControls = false;

            ID = npc.ID;
            npc.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);

            GUISprite spr = new GUISprite(npc.BodySprite, true);
            spr.AnchorToInnerSide(this, SideEnum.Bottom, 1);

            if (npc is Villager v)
            {
                Found = v.Introduced;
                if (!v.Introduced)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else if(showHeart)
                {
                    Rectangle heartRectangle = GUIUtils.ICON_HEART;
                    heartRectangle.Offset(10 * v.GetFriendshipLevel(), 0);

                    GUIImage heart = new GUIImage(heartRectangle);
                    heart.PositionAndMove(this, 22, 34);

                    if (!v.CanGiveGift)
                    {
                        GUIImage heartGlow = new GUIImage(GUIUtils.ICON_HEART_GLOW);
                        heartGlow.CenterOnObject(heart);
                    }
                }
            }
            else if (npc is Merchant m)
            {
                if (!m.Introduced)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else
                {
                    Found = true;
                }
            }
            else if (npc is Traveler t)
            {
                if (!TownManager.DITravelerInfo[t.ID].Item1)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else
                {
                    Found = true;

                    GUIImage classIcon = GUIUtils.GetClassIcon(t.ClassType);
                    classIcon.PositionAndMove(this, 22, 34);
                }
            }
            else if (npc is Mob mob)
            {
                if (TownManager.DIMobInfo[mob.ID] == 0)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else
                {
                    Found = true;
                }
            }
        }

        protected override void BeginHover()
        {
            var infoWindow = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL);

            string strText = Found ? DataManager.GetTextData(ID, "Name", DataType.Actor) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(infoWindow, SideEnum.TopLeft);

            if (Found)
            {
                string strDescText = DataManager.GetTextData(ID, "Description", DataType.Actor);
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
