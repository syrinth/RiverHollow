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

        protected override void DetermineAction(GameTime gTime)
        {
            if (_cooldownTimer.TickDown(gTime))
            {
                _cooldownTimer.Reset();

                if (_liMinions.Count < GetIntByIDKey("Max", 1))
                {
                    var minion = DataManager.CreateActor<Mob>(GetIntByIDKey("Summon"));
                    if (minion != null)
                    {
                        _liMinions.Add(minion);
                        CurrentMap.AddActor(minion);

                        var MyTile = CurrentMap.GetTileByPixelPosition(CollisionCenter);
                        var tiles = MyTile.GetWalkableNeighbours(true);
                        minion.SetPosition(Util.GetRandomItem(tiles).Position);
                        minion.SetInitialPoint(Util.GetRandomItem(tiles).Position);
                    }
                }
            }
        }
    }
}
