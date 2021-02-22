using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Utilities
{
    public class Util
    {
        /// <summary>
        /// Gets the total directional movement speed required to get to the target location
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

        /// <summary>
        /// Takes in a Vector2 representing a tile on the map and converts it to find its
        /// actual pixel world position.
        /// </summary>
        /// <param name="tile">The Vector2 of the tile</param>
        /// <returns>The world position</returns>
        public static Vector2 GetMapPositionOfTile(Vector2 tile)
        {
            Vector2 newVec = Vector2.Zero;
            newVec.X = tile.X * TileSize;
            newVec.Y = tile.Y  * TileSize;

            return newVec;
        }

        #region GetGridCoords
        /// <summary>
        /// These methods return a Vector2 representing the grid coordinates of an
        /// RHTile based off of the input, which is a pixel centered map position.
        /// <returns></returns>
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
        #endregion

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

            if (string.IsNullOrEmpty(text)) { return rv; }

            text = text.Replace(@"\n", System.Environment.NewLine);
            string[] specialSections = text.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            if (specialSections.Length > 1)
            {
                for (int i = 0; i < specialSections.Length; i++)
                {
                    string value = specialSections[i];
                    if (value == "Name") { value = PlayerManager.Name; }
                    else if (value == "Town") { value = PlayerManager.TownName; }
                    else if (DataManager.TextDataHasKey(value))
                    {
                        DataManager.GetTextData(value, ref value, "Name");
                    }

                    rv += value;
                }
            }

            specialSections = Util.FindTags(rv);
            if (specialSections.Length > 1)
            {
                rv = specialSections[0];
                for(int i = 1; i <= specialSections.Length - 1; i++)
                {
                    string[] tagInfo = specialSections[i].Split(':');
                    if (tagInfo[0].Equals("Task"))
                    {
                        PlayerManager.AddToTaskLog(GameManager.DITasks[int.Parse(tagInfo[1])]);
                    }
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
        public static string GetActorString(string verb, DirectionEnum direction)
        {
            return verb + Util.GetEnumString(direction);
        }

        public static string[] FindTags(string data)
        {
            return data.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string[] FindParams(string data)
        {
            return data.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
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
        /// Retrieves the total number of moves to get from one tile to the next, "As the crow flies"
        /// Helpful for quick comparisons for things that don't worry about obstacles.
        /// </summary>
        public static int GetRHTileDelta(RHTile a, RHTile b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
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

        public static string GetPortraitLocation(string path, string callingfunction, string num)
        {
            string cf = "";
            switch(callingfunction)
            {
                case "Gremlin":
                    cf = "G";
                    break;
                case "Villager":
                    cf = "V";
                    break;
                case "Adventurer":
                    cf = "A";
                    break;
                case "Spirit":
                    cf = "S";
                    break;
            }

            string rv = $"{path}{cf}{num}";

            Texture2D text = DataManager.GetTexture(rv);

            if(text == null)
            {
                rv = path + "WizardPortrait";
            }

            return rv;
        }

        #region AssignValue
        /// <summary>
        /// These methods assign a value to the given variable reference if it is found inside of the provided dictionary
        /// </summary>
        /// <param name="obj">Reference to the object to assign to</param>
        /// <param name="key">The key to look for</param>
        /// <param name="dict">The dictionary to search</param>
        public static void AssignValue(ref bool obj, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                obj = true;
            }
        }
        public static void AssignValue(ref string obj, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                obj = dict[key];
            }
            else { obj = string.Empty; }
        }
        public static void AssignValue(ref int obj, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                obj = int.Parse(dict[key]);
            }
        }
        public static void AssignValue(ref float obj, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                obj = float.Parse(dict[key]);
            }
        }
        #endregion

        /// <summary>
        /// Takes in a parameter and returns the ID and the attached rarity.
        /// Will always be 'ID-Rarity'
        /// </summary>
        /// <param name="value">rThe param to check</param>
        /// <param name="ID">Ref to the ID</param>
        /// <param name="rarity">Ref to the Rarity</param>
        public static void GetRarity(string value, ref int ID, ref RarityEnum rarity)
        {
            //Monster info is written like ID-Rarity|ID-Rarity etc
            string[] info = value.Split('-');
            ID = int.Parse(info[0]);
            rarity = ParseEnum<RarityEnum>(info[1]);
        }

        /// <summary>
        /// Makes a roll against the rarity table and returns the determined rarity enum
        /// </summary>
        /// <param name="dictionary">The dictionary to check against the rarities</param>
        /// <returns>The highest valid rarity</returns>
        public static RarityEnum RollAgainstRarity(Dictionary<RarityEnum, List<int>> dictionary)
        {
            RarityEnum rv = RarityEnum.C;

            int rarityIndex = (int)RHRandom.Instance.Next(1, 1000);

            if (rarityIndex > 990 && dictionary.ContainsKey(RarityEnum.M)) { rv = RarityEnum.M; }
            else if (rarityIndex > 950 && dictionary.ContainsKey(RarityEnum.R)) { rv = RarityEnum.R; }
            else if (rarityIndex > 850 && dictionary.ContainsKey(RarityEnum.U)) { rv = RarityEnum.U; }

            return rv;
        }

        /// <summary>
        /// Adds an object to a list as long as the object is not already present in it
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="list">Reference to the list to add to</param>
        /// <param name="obj">The object to add.</param>
        public static void AddUniquelyToList<T>(ref List<T> list, T obj)
        {
            if (!list.Contains(obj))
            {
                list.Add(obj);
            }
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
