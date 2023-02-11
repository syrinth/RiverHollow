using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Map_Handling;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;
using System;

namespace RiverHollow.Utilities
{
    struct NewMapInfo
    {
        public RHMap NextMap;
        public Point PlayerPosition;
        public Building EnteredBuilding;
        public NewMapInfo(RHMap map, Point pos, Building b)
        {
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
}
