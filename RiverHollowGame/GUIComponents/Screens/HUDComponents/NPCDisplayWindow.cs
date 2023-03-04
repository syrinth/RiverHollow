﻿using Microsoft.Xna.Framework;
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

        public delegate void CodexDelegate(NPCDisplayWindow window);
        protected CodexDelegate _delAction;
        public NPCDisplayWindow(Actor npc, CodexDelegate del) : base(GUIWindow.Codex_NPC_Window, GameManager.ScaleIt(32), GameManager.ScaleIt(44))
        {
            ID = npc.ID;
            _delAction = del;
            npc.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);

            GUISprite spr = new GUISprite(npc.BodySprite, true);
            spr.AnchorToInnerSide(this, SideEnum.Bottom, GameManager.ScaleIt(1));
            AddControl(spr);

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
                    heart.Position(Position());
                    heart.ScaledMoveBy(22, 34);
                    AddControl(heart);

                    if (!villager.CanGiveGift)
                    {
                        GUIImage heartGlow = new GUIImage(new Rectangle(192, 72, 12, 11), DataManager.HUD_COMPONENTS);
                        heartGlow.CenterOnObject(heart);
                        AddControl(heartGlow);
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

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            //if (Contains(mouse) && Enabled && _delAction != null)
            //{
            //    _delAction(this);
            //    rv = true;
            //}

            return rv;
        }
    }
}
