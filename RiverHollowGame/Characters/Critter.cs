using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Critter : WorldActor
    {
        private bool _bFlee = false;
        private double _dNextPlay = 0;
        private double _dCountdown = 0;
        public int ID { get; } = -1;

        public Critter(int id, Dictionary<string, string> stringData)
        {
            ID = id;
            _eActorType = WorldActorTypeEnum.Critter;
            _bIgnoreCollisions = true;
            _dNextPlay = 1 + SetRandom(4, 0.5);
            _iBodyHeight = TILE_SIZE;

            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action1);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action2);
            LoadSpriteAnimations(ref _sprBody, liData, NPC_FOLDER + "NPC_" + ID);

            Facing = DirectionEnum.Down;
            PlayAnimation(VerbEnum.Idle);

            _sprBody.SetNextAnimation(Util.GetActorString(VerbEnum.Action1, Facing), Util.GetActorString(VerbEnum.Idle, Facing));
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (!_bFlee)
            {
                if (_dCountdown < _dNextPlay) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
                else
                {
                    _dNextPlay = 1 + SetRandom(4, 0.5);
                    _dCountdown = 0;
                    PlayAnimation(VerbEnum.Action1);
                }

                if (PlayerManager.PlayerInRange(_sprBody.Center, 80))
                {
                    _bFlee = true;
                    _dCountdown = 0;

                    PlayAnimation(VerbEnum.Action2);
                }
            }
            else
            {
                if (_dCountdown < 1) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
                else { _sprBody.SetLayerDepthMod(9999); }

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
