using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters.Mobs
{
    internal class Shooter : Mob
    {
        protected List<Projectile> _liProjectiles;

        public Shooter(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
            _liProjectiles = new List<Projectile>();
        }

        public override void Update(GameTime gTime)
        {
            _liProjectiles.ForEach(x => x.Update(gTime));
            _liProjectiles.RemoveAll(x => x.Finished);

            base.Update(gTime);
        }

        protected override void DetermineAction(GameTime gTime)
        {
            if (HasProjectiles && PlayerManager.PlayerActor.HasHP)
            {
                var data = Util.FindParams(GetStringByIDKey("Projectile"));

                if (CanFire(gTime, data))
                {
                    _cooldownTimer.Reset(Cooldown + (Cooldown * RHRandom.Instance().Next(1, 5) / 10));

                    Projectile p = DataManager.CreateProjectile(int.Parse(data[0]));
                    p.Kickstart(this, AimsProjectiles(data));

                    _liProjectiles.Add(p);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
            _liProjectiles.ForEach(x => x.Draw(spriteBatch));
        }

        private bool CanFire(GameTime gTime, string[] data)
        {
            bool rv = false;

            if (MapManager.MapChangeTimer.Finished() && _cooldownTimer.TickDown(gTime))
            {
                if (AimsProjectiles(data)) { rv = true; }
                else if (Facing == Util.GetDirection(GetPlayerDirection())) { rv = true; }
            }

            return rv;
        }

        private bool AimsProjectiles(string[] data)
        {
            return data.Length > 1 && data[1].Equals("Aim") && _eCurrentState == NPCStateEnum.TrackPlayer;
        }
    } 
}
