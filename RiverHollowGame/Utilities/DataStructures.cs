using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Utilities
{
    public class SpawnData
    {
        public readonly int ID;
        public readonly SpawnTypeEnum Type;

        public SpawnData(int id, SpawnTypeEnum t)
        {
            ID = id;
            Type = t;
        }

        public WorldObject GetDataObject()
        {
            WorldObject rv = null;

            switch (Type)
            {
                case SpawnTypeEnum.Item:
                    var item = DataManager.GetItem(ID);
                    if (item != null)
                    {
                        rv = new WrappedItem(item.ID);
                    }
                    break;
                case SpawnTypeEnum.Object:
                    rv = DataManager.CreateWorldObjectByID(ID);
                    break;

            }

            return rv;
        }
    }

    public class TravelerNeed
    {
        public MerchandiseTypeEnum MerchType;
        public int MerchID { get; } = -1;

        public int Tier { get; private set; } = 0;

        public TravelerNeed(int merchID)
        {
            MerchID = merchID;
            MerchType = MerchandiseTypeEnum.None;
        }

        public TravelerNeed(MerchandiseTypeEnum merchType)
        {
            MerchID = -1;
            MerchType = merchType;
        }

        public void SetTier(int tier)
        {
            Tier = tier;
        }

        public bool Validate(Merchandise m)
        {
            bool rv = true;

            if (MerchType != MerchandiseTypeEnum.None && m.MerchType != MerchType)
            {
                rv = false;
            }
            else if (MerchID > -1 && m.ID != MerchID)
            {
                rv = false;
            }

            if (Tier > 0 && m.Tier != Tier)
            {
                rv = false;
            }

            return rv;
        }
    }

    public struct ScheduleData
    {
        public string Time;
        public NPCActionState State;
        public string Data;

        public ScheduleData(string time, NPCActionState state, string data = "") {
            this.Time = time;
            this.State = state;
            this.Data = data;
        }
    }

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
