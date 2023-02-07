using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;
using System;

namespace RiverHollow.Utilities
{
    public struct RHSize
    {
        public int Width;
        public int Height;
        public RHSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public RHSize(int[] array)
        {
            if(array.Length == 2)
            {
                Width = array[0];
                Height = array[1];
            }
            else
            {
                Width = 0;
                Height = 0;
            }
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
        public Vector2 Offset;
    }
}
