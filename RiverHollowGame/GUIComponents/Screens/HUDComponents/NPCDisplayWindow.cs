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

            if (npc.IsActorType(ActorTypeEnum.Villager))
            {
                Villager villager = (Villager)npc;

                if (!villager.Introduced)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else if(showHeart)
                {
                    Found = true;

                    Rectangle heartRectangle = GUIUtils.ICON_HEART;
                    heartRectangle.Offset(10 * villager.GetFriendshipLevel(), 0);

                    GUIImage heart = new GUIImage(heartRectangle);
                    heart.PositionAndMove(this, 22, 34);

                    if (!villager.CanGiveGift)
                    {
                        GUIImage heartGlow = new GUIImage(GUIUtils.ICON_HEART_GLOW);
                        heartGlow.CenterOnObject(heart);
                    }
                }
            }
            else if (npc.IsActorType(ActorTypeEnum.Merchant))
            {
                Merchant villager = (Merchant)npc;

                if (!villager.Introduced)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else
                {
                    Found = true;
                }
            }
            else if (npc.IsActorType(ActorTypeEnum.Traveler))
            {
                if (!TownManager.DITravelerInfo[npc.ID].Item1)
                {
                    spr.SetColor(Color.Black * FADE);
                }
                else
                {
                    Found = true;
                }
            }
            else if (npc.IsActorType(ActorTypeEnum.Mob))
            {
                if (PlayerManager.DIMobInfo[npc.ID] == 0)
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
