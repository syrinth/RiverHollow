using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;

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
