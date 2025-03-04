﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Map_Handling
{
    public class Dungeon
    {
        protected List<string> _liMapNames;
        protected List<TriggerObject> _liTriggerObjects;
        protected string _sEntranceMapName;
        protected Point _pRecallPoint;
        public int NumKeys { get; private set; }
        public string Name { get; private set; }
        protected List<WarpPoint> _liWarpPoints;
        public IList<WarpPoint> WarpPoints { get { return _liWarpPoints.AsReadOnly(); } }

        ColorStateEnum _eCurrentColorState;

        protected Dictionary<string, string> _diDungeonInfo;

        public Dungeon(string name)
        {
            Name = name;

            _liMapNames = new List<string>();
            _liWarpPoints = new List<WarpPoint>();
            _liTriggerObjects = new List<TriggerObject>();
        }

        public virtual void AddMap(RHMap map)
        {
            _liMapNames.Add(map.Name);

            if (map.GetMapProperties().ContainsKey("Color"))
            {
                _eCurrentColorState = Util.ParseEnum<ColorStateEnum>(map.GetMapProperties()["Color"]);
            }
        }

        public void AddWarpPoint(WarpPoint obj)
        {
            _liWarpPoints.Add(obj);
        }

        public void AddTriggerObject(TriggerObject obj)
        {
            _liTriggerObjects.Add(obj);
        }

        public void GoToEntrance()
        {
            MapManager.FadeToNewMap(MapManager.Maps[_sEntranceMapName], _pRecallPoint, DirectionEnum.Down);
            PlayerManager.PlayerActor.SetFacing(DirectionEnum.Down);
        }

        public void AddKey() { NumKeys++; }
        public void UseKey() { NumKeys--; }

        public void ActivateTrigger(string triggerName)
        {
            foreach (TriggerObject obj in _liTriggerObjects)
            {
                obj.AttemptToTrigger(triggerName);
            }
        }

        public virtual void ResetDungeon() { }
    }
}
