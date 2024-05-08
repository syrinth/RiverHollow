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

                if (int.TryParse(data[0], out int projectileID) && CanFire(gTime, data))
                {
                    _cooldownTimer.Reset(Cooldown + (Cooldown * RHRandom.Instance().Next(1, 5) / 10));

                    Projectile p = DataManager.CreateProjectile(projectileID);

                    //Important! Note that this starts at the CollisionBox of the Enemy!
                    //Always check the CollisionBox size and location
                    Point startPoint = CollisionBoxLocation;
                    if (data.Length > 1)
                    {
                        Point newP = Util.ParsePoint(data[(int)Facing]);
                        switch (Facing)
                        {
                            case DirectionEnum.Down:
                            case DirectionEnum.Right:
                                startPoint += newP;
                                break;
                            case DirectionEnum.Up:
                                startPoint += new Point(newP.X, -newP.Y);
                                break;
                            case DirectionEnum.Left:
                                startPoint += new Point(-newP.X, newP.Y);
                                break;

                        }
                    }
                        
                    p.Kickstart(this, startPoint, GetBoolByIDKey("Aim"));

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
                if (GetBoolByIDKey("Aim")) { rv = true; }
                else if (Facing == Util.GetDirection(GetPlayerDirection())) { rv = true; }
            }

            return rv;
        }
    } 
}
