using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.Misc
{
    public class Util
    {
        public static void GetMoveSpeed(Vector2 currentPos, Vector2 targetPos, int speed, ref Vector2 direction)
        {
            float newX = 0; float newY = 0;
            if (targetPos.X != currentPos.X)
            {
                newX = (targetPos.X > currentPos.X) ? 1 : -1;
            }
            if (targetPos.Y != currentPos.Y)
            {
                newY = (targetPos.Y > currentPos.Y) ? 1 : -1;
            }

            float deltaX = Math.Abs(targetPos.X - currentPos.X);
            float deltaY = Math.Abs(targetPos.Y - currentPos.Y);
            Vector2 dir = new Vector2(deltaX, deltaY);
            dir.Normalize();
            dir = dir * speed;

            direction.X = (deltaX < speed) ? newX * deltaX : newX * dir.X;
            direction.Y = (deltaY < speed) ? newY * deltaY : newY * dir.Y;
        }

        public static Vector2 Normalize(Vector2 p)
        {
            Vector2 newVec = Vector2.Zero;
            newVec.X = ((int)(p.X / TileSize)) * TileSize;
            newVec.Y = ((int)(p.Y / TileSize)) * TileSize;

            return newVec;
        }

        public static Point Normalize(Point p)
        {
            Point newVec = Point.Zero;
            newVec.X = ((int)(p.X / TileSize)) * TileSize;
            newVec.Y = ((int)(p.Y / TileSize)) * TileSize;

            return newVec;
        }

        public static void ParseContentFile(ref string filePath, ref string name)
        {
            FileInfo f = new FileInfo(filePath);
            filePath = filePath.Replace(@"Content\", "");
            filePath = filePath.Remove(filePath.Length - 4, 4);

            name = f.Name.Remove(f.Name.Length - 4);
        }

        public static Rectangle FloatRectangle(Vector2 pos, float width, float height)
        {
            return FloatRectangle(pos.X, pos.Y, width, height);
        }
        public static Rectangle FloatRectangle(float x, float y, float width, float height)
        {
            return new Rectangle((int)x, (int)y, (int)width, (int)height);
        }

        public static string ProcessText(string text, string name = "")
        {
            string rv = string.Empty;
            text = text.Replace(@"\n", System.Environment.NewLine);
            string[] nameSections = text.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameSections.Length > 1)
            {
                for (int i = 0; i < nameSections.Length; i++)
                {
                    if (int.TryParse(nameSections[i], out int val))
                    {
                        if (val == 0) { nameSections[i] = name; }
                        else { nameSections[i] = CharacterManager.GetCharacterNameByIndex(val); }
                    }
                    else if (nameSections[i] == "^") { nameSections[i] = PlayerManager.Name; }

                    rv += nameSections[i];
                }
            }
            string[] itemSections = text.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            if (itemSections.Length > 1)
            {
                for (int i = 0; i < itemSections.Length; i++)
                {
                    if (int.TryParse(itemSections[i], out int val))
                    {
                        itemSections[i] = ObjectManager.GetItem(val).Name;

                        if (itemSections[i].StartsWith("a", StringComparison.OrdinalIgnoreCase) || itemSections[i].StartsWith("e", StringComparison.OrdinalIgnoreCase) || itemSections[i].StartsWith("i", StringComparison.OrdinalIgnoreCase) || itemSections[i].StartsWith("o", StringComparison.OrdinalIgnoreCase) || itemSections[i].StartsWith("u", StringComparison.OrdinalIgnoreCase))
                        {
                            itemSections[i] = itemSections[i].Insert(0, "an ");
                        }
                        else {
                            itemSections[i] = itemSections[i].Insert(0, "a ");
                        }
                    }

                    rv += itemSections[i];
                }
            }

            if (string.IsNullOrEmpty(rv)) { rv = text; }
            return rv;
        }

        public static TEnum ParseEnum<TEnum>(string convertThis) where TEnum : struct
        {
            TEnum rv = default(TEnum);

            rv = Enum.TryParse<TEnum>(convertThis, true, out rv) ? rv : default(TEnum);
            return rv;
        }

        public static string[] FindTags(string data)
        {
            return data.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class RHRandom : Random
    {
        public RHRandom() : base()
        {
            Thread.Sleep(1);
        }

        public override int Next(int min, int max)
        {
            //Thread.Sleep(1);
            int rv = 0;
            rv = base.Next(min, max + 1);
            return rv;
        }
    }
}
