﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Critter : WorldActor
    {
        private bool _bFlee = false;
        private RHTimer _animationTimer;

        public Critter(int id, Dictionary<string, string> stringData) : base(id)
        {
            ActorType = WorldActorTypeEnum.Critter;
            _bIgnoreCollisions = true;
            SlowDontBlock = true;
            _animationTimer = new RHTimer(1 + SetRandom(4, 0.5));
            _iBodyHeight = Constants.TILE_SIZE;

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action1);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action2);
            LoadSpriteAnimations(ref _sprBody, liData, SpriteName());

            Facing = DirectionEnum.Down;
            PlayAnimation(VerbEnum.Idle);

            _sprBody.SetNextAnimation(Util.GetActorString(VerbEnum.Action1, Facing), Util.GetActorString(VerbEnum.Idle, Facing));
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (!_bFlee)
            {
                _animationTimer.TickDown(gTime);
                if(_animationTimer.Finished())
                {
                    _animationTimer.Reset(1 + SetRandom(4, 0.5));
                    PlayAnimation(VerbEnum.Action1, Facing);
                }

                if (PlayerManager.PlayerInRange(_sprBody.Center, 80))
                {
                    _bFlee = true;
                    _animationTimer.Stop();

                    PlayAnimation(VerbEnum.Action2, Facing);
                }
            }
            else
            {
                _animationTimer.TickDown(gTime);
                if (_animationTimer.Finished())
                {
                    _sprBody.SetLayerDepthMod(Constants.MAX_LAYER_DEPTH);
                }

                Position += new Vector2(-2, -2);

                if (Position.X < 0 || Position.Y < 0)
                {
                    CurrentMap.RemoveActor(this);
                }
            }
        }

        private double SetRandom(int max, double mod)
        {
            return RHRandom.Instance().Next(max) * mod;
        }
    }
}
