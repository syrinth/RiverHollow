using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Misc;
using System.Security.RightsManagement;

namespace RiverHollow.Characters
{
    internal class Fish : Actor
    {

        RHTimer _reelTimer;

        public Fish(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            BodySprite = LoadSpriteAnimations(liData, SpriteName());

            SetFacing(DirectionEnum.Down);
            PlayAnimation(VerbEnum.Idle);

            Wandering = true;
            _iMinWander = 1;
            _iMaxWander = 3;
            _fBaseWanderTimer = 3;
            _movementTimer = new RHTimer(_fBaseWanderTimer);
            _fWanderSpeed = 0.1f;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (OnTheMap && FishingManager.Waiting)
            {
                if (_reelTimer == null) {
                    if (FishingManager.BobberRectangle.Contains(CollisionCenter))
                    {
                        Wandering = false;
                        SetMoveTo(FishingManager.BobberRectangle.Location);
                        _reelTimer = new RHTimer(RHRandom.Instance().Next(3));
                    }
                }
                else if (_reelTimer.TickDown(gTime))
                {
                    FishingManager.StartReeling(this);
                }
            }

            ProcessStateEnum(gTime, true);
            CheckBumpedIntoSomething();
        }

        public override void PlayAnimationVerb(VerbEnum verb)
        {
            PlayAnimation(VerbEnum.Idle);
        }
        public override void PlayAnimation(string verb, DirectionEnum dir)
        {
            PlayAnimation(VerbEnum.Idle);
        }
    }
}
