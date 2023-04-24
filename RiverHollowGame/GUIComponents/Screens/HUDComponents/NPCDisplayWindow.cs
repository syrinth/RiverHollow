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

        public NPCDisplayWindow(Actor npc) : base(GUIWindow.Codex_NPC_Window, GameManager.ScaleIt(32), GameManager.ScaleIt(44))
        {
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
                else
                {
                    Found = true;
                    Point pos = new Point(208, 72);
                    pos.X += 10 * villager.GetFriendshipLevel();

                    GUIImage heart = new GUIImage(new Rectangle(pos.X, pos.Y, 10, 9), DataManager.HUD_COMPONENTS);
                    heart.PositionAndMove(this, 22, 34);

                    if (!villager.CanGiveGift)
                    {
                        GUIImage heartGlow = new GUIImage(new Rectangle(192, 72, 12, 11), DataManager.HUD_COMPONENTS);
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
    }
}
