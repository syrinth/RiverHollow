using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Map_Handling
{
    public class Dungeon
    {
        
        protected string _sEntranceMapName;
        protected Vector2 _vRecallPoint;
        public int NumKeys { get; private set; }
        public string Name { get; private set; }
        protected List<WarpPoint> _liWarpPoints;
        public IList<WarpPoint> WarpPoints { get { return _liWarpPoints.AsReadOnly(); } }

        protected Dictionary<string, string> _diDungeonInfo;

        public Dungeon(string name)
        {
            _liWarpPoints = new List<WarpPoint>();
            Name = name;
        }

        public void LoadDungeon(string currentMap)
        {
            //_diDungeonInfo =
        }

        public void AddWarpPoint(WarpPoint obj)
        {
            _liWarpPoints.Add(obj);
        }

        public void GoToEntrance()
        {
            MapManager.FadeToNewMap(MapManager.Maps[_sEntranceMapName], _vRecallPoint);
            PlayerManager.PlayerActor.DetermineFacing(new Vector2(0, 1));
        }

        public void AddKey() { NumKeys++; }
        public void UseKey() { NumKeys--; }




    }
}
