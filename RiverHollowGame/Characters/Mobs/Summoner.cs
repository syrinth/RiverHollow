using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Characters.Mobs
{
    internal class Summoner : Mob
    {
        List<Mob> _liMinions;
        public Summoner(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
            _liMinions = new List<Mob>();
        }

        public override void Update(GameTime gTime)
        {
            var copy = new List<Mob>(_liMinions);
            for (int i = 0; i < copy.Count; i++)
            {
                if (!copy[i].HasHP)
                {
                    _liMinions.Remove(copy[i]);
                }
            }

            base.Update(gTime);
        }

        private bool WantsToSummon()
        {
            return PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 6) || _liMinions.Count < GetIntByIDKey("Min", 0);
        }
        protected override void DetermineAction(GameTime gTime)
        {
            if (_cooldownTimer.TickDown(gTime))
            {
                _cooldownTimer.Reset();

                if (_liMinions.Count < GetIntByIDKey("Max", 1) && WantsToSummon())
                {
                    var minion = DataManager.CreateMob(GetIntByIDKey("Summon"));
                    _liMinions.Add(minion);
                    CurrentMap.AddActor(minion);

                    var MyTile = CurrentMap.GetTileByPixelPosition(CollisionCenter);
                    var tiles = MyTile.GetAdjacentTiles(true);
                    minion.SetPosition(Util.GetRandomItem(tiles).Position);
                }
            }
        }
    }
}
