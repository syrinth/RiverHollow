﻿using Microsoft.Xna.Framework;
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
        /// <summary>
        /// Gets the total directional movement speed required to ge to the target location
        /// </summary>
        /// <param name="currentPos">The current position of the Actor</param>
        /// <param name="targetPos">The end goal position for the Actor</param>
        /// <param name="speed">The actor's movement speed</param>
        /// <param name="direction">Reference to the direction the actor will move in</param>
        public static void GetMoveSpeed(Vector2 currentPos, Vector2 targetPos, float speed, ref Vector2 direction)
        {
            float newX = 0; float newY = 0;

            //Determine in which direction(s) the character needs to move
            if (targetPos.X != currentPos.X) { newX = (targetPos.X > currentPos.X) ? 1 : -1; }
            if (targetPos.Y != currentPos.Y) { newY = (targetPos.Y > currentPos.Y) ? 1 : -1; }

            //Get the absolute value of the movement
            float deltaX = Math.Abs(targetPos.X - currentPos.X);
            float deltaY = Math.Abs(targetPos.Y - currentPos.Y);

            //Normalize the Vector2 to a total length of 1
            Vector2 dir = new Vector2(deltaX, deltaY);
            dir.Normalize();
            dir = dir * speed;

            //If the absolute value of the X or Y movement is less than the speed then set the movement
            //direction to the delta, otherwise, multiply the needed movement Vector2 by the Normalized movement
            direction.X = (deltaX < speed) ? newX * deltaX : newX * dir.X;
            direction.Y = (deltaY < speed) ? newY * deltaY : newY * dir.Y;
        }

        public static Vector2 SnapToGrid(Vector2 p)
        {
            Vector2 newVec = Vector2.Zero;
            newVec.X = ((int)(p.X / TileSize)) * TileSize;
            newVec.Y = ((int)(p.Y / TileSize)) * TileSize;

            return newVec;
        }

        public static Vector2 GetGridCoords(int x, int y)
        {
            return GetGridCoords(new Vector2(x, y));
        }
        public static Vector2 GetGridCoords(Point p)
        {
            return GetGridCoords(p.ToVector2());
        }
        public static Vector2 GetGridCoords(Vector2 vec)
        {
            Vector2 rv = new Vector2(vec.X / TileSize, vec.Y / TileSize);
            return rv;
        }
        public static Vector2 MoveUpTo(Vector2 currPos, Vector2 moveTo, float speed)
        {
            Vector2 rv = Vector2.Zero;

            rv.X = EvalAxisChange(currPos.X, moveTo.X, speed);
            rv.Y = EvalAxisChange(currPos.Y, moveTo.Y, speed);

            return rv;
        }

        private static float EvalAxisChange(float currAxis, float moveToAxis, float speed)
        {
            float rv = 0;
            if (currAxis > moveToAxis)
            {
                if (currAxis - speed <= moveToAxis) { rv = -(currAxis - moveToAxis); }
                else { rv = -speed; }
            }
            else if (currAxis < moveToAxis)
            {
                if (currAxis + speed >= moveToAxis) { rv = moveToAxis - currAxis; }
                else { rv = speed; }
            }

            return rv;
        }

        public static void ParseContentFile(ref string filePath)
        {
            if (filePath.StartsWith("Content"))
            {
                FileInfo f = new FileInfo(filePath);
                filePath = filePath.Replace(@"Content\", "");
                filePath = filePath.Remove(filePath.Length - 4, 4);
            }
        }
        public static void ParseContentFileRetName(ref string filePath, ref string name)
        {
            if (filePath.StartsWith("Content"))
            {
                FileInfo f = new FileInfo(filePath);
                filePath = filePath.Replace(@"Content\", "");
                filePath = filePath.Remove(filePath.Length - 4, 4);

                name = f.Name.Remove(f.Name.Length - 4);
            }
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
                        else { nameSections[i] = DataManager.GetCharacterNameByIndex(val); }
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
                        itemSections[i] = DataManager.GetItem(val).Name;

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
        public static string GetEnumString<TEnum>(TEnum convertThis)
        {
            string rv = string.Empty;

            rv = Enum.GetName(typeof(TEnum), convertThis);

            return rv;
        }

        public static string GetActorString(VerbEnum verb)
        {
            return GetEnumString(verb);
        }
        public static string GetActorString(VerbEnum verb, DirectionEnum direction)
        {
            return GetEnumString(verb) + Util.GetEnumString(direction);
        }

        public static string[] FindTags(string data)
        {
            return data.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static String NumToString(int number, bool isCaps)
        {
            Char c = (Char)((isCaps ? 65 : 97) + (number - 1));

            return c.ToString();
        }

        public static bool InBetween(int check, int first, int second)
        {
            return InBetween((float)check, (float)first, (float)second);
        }
        public static bool InBetween(float check, float first, float second)
        {
            return (check >= first && check <= second);
        }

        public static float GetRandomFloat(int min, int max, int precision)
        {
            float rv = 0;
            string composite = RHRandom.Instance.Next(min, max).ToString() + ".";

            for (int i = 0; i < precision; i++)
            {
                composite += RHRandom.Instance.Next(0, 9);
            }

            rv = float.Parse(composite);

            return rv;
        }

        /// <summary>
        /// Uses the Camera's Translation Matrix to translate the given RHMap position
        /// to the Screen position.
        /// </summary>
        /// <param name="worldPosition">The World position to translate</param>
        /// <returns></returns>
        public static Vector2 GetScreenPositionFromWorld(Vector2 worldPosition)
        {
            Vector3 translate = Camera._transform.Translation;
            Vector2 newPos = Vector2.Zero;
            newPos.X = (int)translate.X + (worldPosition.X * Scale);
            newPos.Y = (int)translate.Y + (worldPosition.Y * Scale);

            return newPos;
        }
    }

    public class RHRandom : Random
    {
        private static RHRandom _rhInstance = new RHRandom();

        public static RHRandom Instance => _rhInstance;
        private RHRandom() : base() {}

        public override int Next(int min, int max)
        {
            int rv = 0;
            rv = base.Next(min, max + 1);
            return rv;
        }
    }
}
