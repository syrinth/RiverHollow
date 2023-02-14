using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Critter : Actor
    {
        private bool _bFlee = false;
        private RHTimer _animationTimer;

        public Critter(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            IgnoreCollisions = true;
            SlowDontBlock = true;
            _animationTimer = new RHTimer(1 + SetRandom(4, 0.5));
            Size.Y = Constants.TILE_SIZE;

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action1);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action2);
            BodySprite = LoadSpriteAnimations(liData, SpriteName());

            Facing = DirectionEnum.Down;
            PlayAnimation(VerbEnum.Idle);

            BodySprite.SetNextAnimation(Util.GetActorString(VerbEnum.Action1, Facing), Util.GetActorString(VerbEnum.Idle, Facing));
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (!_bFlee)
            {
                if (_animationTimer.TickDown(gTime))
                {
                    _animationTimer.Reset(1 + SetRandom(4, 0.5));
                    PlayAnimation(VerbEnum.Action1, Facing);
                }

                CheckFlee(PlayerManager.PlayerActor);
                foreach(Mob npc in CurrentMap.Mobs)
                {
                    CheckFlee(npc);
                    if (_bFlee)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (_animationTimer.TickDown(gTime))
                {
                    BodySprite.SetLayerDepthMod(Constants.MAX_LAYER_DEPTH);
                }

                MoveActor(new Vector2(-2, -2));

                if (Position.X < 0 || Position.Y < 0)
                {
                    CurrentMap.RemoveActor(this);
                }
            }
        }

        private void CheckFlee(Actor actor)
        {
            if (Util.GetDistance(actor.Center, CollisionCenter) <= 80)
            {
                _bFlee = true;
                _animationTimer.Stop();

                PlayAnimation(VerbEnum.Action2, Facing);
            }
        }

        private double SetRandom(int max, double mod)
        {
            return RHRandom.Instance().Next(max) * mod;
        }
    }
}
