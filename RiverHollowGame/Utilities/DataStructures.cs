using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Map_Handling;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Utilities
{
    struct NewMapInfo
    {
        public DirectionEnum Facing;
        public RHMap NextMap;
        public Point PlayerPosition;
        public Building EnteredBuilding;
        public NewMapInfo(RHMap map, Point pos, DirectionEnum f, Building b)
        {
            Facing = f;
            NextMap = map;
            PlayerPosition = pos;
            EnteredBuilding = b;
        }
    }

    public struct AttributeStatusEffect
    {
        public int Duration;
        public int Value;

        public AttributeStatusEffect(int v, int d)
        {
            Value = v;
            Duration = d;
        }
    }

    public struct TacticalStatusEffectData
    {
        public int BuffID;
        public int Duration;
        public string Tags;
        public AnimatedSprite Sprite;
    }

    public struct LiteStatusEffectData
    {
        public int BuffID;
        public int Duration;
        public string Tags;
        public GUISprite Sprite;
    }

    public struct LightInfo
    {
        public Light LightObject;
        public Point Offset;
    }

    public struct MapNode
    {
        public Point MapPosition;
        public int Cost;
        public Dictionary<string, int> MapConnections;

        public MapNode(Point position, int time, string connections)
        {
            MapPosition = position;
            Cost= time;

            MapConnections = new Dictionary<string, int>();
            string[] allConnections = Util.FindParams(connections);
            foreach (var s in allConnections)
            {
                string[] connectionData = Util.FindArguments(s);
                MapConnections[connectionData[0]] = int.Parse(connectionData[1]);
            }
        }
    }

    public struct MapPathInfo
    {
        public string MapName { get; }
        public string MapConnection { get; }
        public int Time { get; }
        public bool Unlocked;

        public MapPathInfo(string name, string connection, int travel)
        {
            MapName = name;
            MapConnection = connection;
            Time = travel;
            Unlocked = false;
        }
    }
}
