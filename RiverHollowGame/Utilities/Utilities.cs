using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            newVec.X = ((int)(p.X / TILE_SIZE)) * TILE_SIZE;
            newVec.Y = ((int)(p.Y / TILE_SIZE)) * TILE_SIZE;

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
            newVec.X = tile.X * TILE_SIZE;
            newVec.Y = tile.Y  * TILE_SIZE;

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
            Vector2 rv = new Vector2(vec.X / TILE_SIZE, vec.Y / TILE_SIZE);
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

        public static Rectangle FloatRectangle(Vector2 pos, RHSize size)
        {
            return FloatRectangle(pos.X, pos.Y, size.Width, size.Height);
        }
        public static Rectangle FloatRectangle(Vector2 pos, float width, float height)
        {
            return FloatRectangle(pos.X, pos.Y, width, height);
        }
        public static Rectangle FloatRectangle(float x, float y, float width, float height)
        {
            return new Rectangle((int)x, (int)y, (int)width, (int)height);
        }

        public static string ProcessText(string text)
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

            if (string.IsNullOrEmpty(rv)) { rv = text; }
            return rv;
        }

        public static bool StringIsEnum<TEnum>(string val)
        {
            bool rv = false;

            foreach(TEnum e in Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList())
            {
                if (val.Equals(GetEnumString(e)))
                {
                    rv = true;
                    break;
                }
            }

            return rv;
        }
        public static TEnum ParseEnum<TEnum>(string convertThis) where TEnum : struct
        {
            TEnum rv = default;

            rv = Enum.TryParse(convertThis, true, out rv) ? rv : default;
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

        public static Dictionary<string, string> DictionaryFromTaggedString(string taggedString)
        {
            Dictionary<string, string> dss = new Dictionary<string, string>();
            foreach (string s in Util.FindTags(taggedString))
            {
                if (s.Contains(":"))
                {
                    string[] tagSplit = s.Split(':');
                    dss[tagSplit[0]] = tagSplit[1];
                }
                else
                {
                    dss[s] = "";
                }
            }

            return dss;
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
            string composite = RHRandom.Instance().Next(min, max).ToString() + ".";

            for (int i = 0; i < precision; i++)
            {
                composite += RHRandom.Instance().Next(0, 9);
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
            newPos.X = (int)translate.X + (worldPosition.X * CurrentScale);
            newPos.Y = (int)translate.Y + (worldPosition.Y * CurrentScale);

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
        public static void AssignValue(ref bool value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                value = true;
            }
        }
        public static void AssignValue(ref string value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                value = dict[key];
            }
        }
        public static void AssignValue(ref int value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                value = int.Parse(dict[key]);
            }
        }
        public static void AssignValue(ref double value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                value = double.Parse(dict[key]);
            }
        }
        public static void AssignValue(ref float value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                value = float.Parse(dict[key]);
            }
        }
        public static void AssignValue(ref RHSize value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                string[] splitVal = dict[key].Split('-');
                value = new RHSize(int.Parse(splitVal[0]), int.Parse(splitVal[1]));
            }
        }
        public static void AssignValue(ref Vector2 value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                string[] splitVal = dict[key].Split('-');
                value = new Vector2(float.Parse(splitVal[0]), float.Parse(splitVal[1]));
            }
        }
        public static void AssignValue(ref Point value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                string[] splitVal = dict[key].Split('-');
                value = new Point(int.Parse(splitVal[0]), int.Parse(splitVal[1]));
            }
        }
        public static void AssignValue(ref Rectangle value, string key, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                string[] ent = dict[key].Split('-');
                value.X = int.Parse(ent[0]);
                value.Y = int.Parse(ent[1]);
                value.Width = int.Parse(ent[2]);
                value.Height = int.Parse(ent[3]);
            }
        }
        public static void AssignValue<TEnum>(ref TEnum value, string key, Dictionary<string, string> dict) where TEnum : struct
        {
            if (dict.ContainsKey(key)) {
                value = Util.ParseEnum<TEnum>(dict[key]);
            }
        }
        public static void AssignValue(ref Dictionary<int, int> dictValue, string key, Dictionary<string, string> dict)
        {
            dictValue = new Dictionary<int, int>();
            if (dict.ContainsKey(key))
            {
                //Split by "|" for each item set required
                string[] split = Util.FindParams(dict[key]);
                foreach (string s in split)
                {
                    string[] splitData = s.Split('-');
                    dictValue[int.Parse(splitData[0])] = int.Parse(splitData[1]);
                }
            }
        }
        public static bool AssignValues(ref int value1, ref int value2, string key, Dictionary<string, string> dict)
        {
            bool rv = false;
            if (dict.ContainsKey(key))
            {
                string[] dimensions = dict[key].Split('-');
                value1 = int.Parse(dimensions[0]);
                value2 = int.Parse(dimensions[1]);

                rv = true;
            }

            return rv;
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

            int rarityIndex = (int)RHRandom.Instance().Next(1, 1000);

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

        public static List<Vector2> GetAllPointsInArea(Vector2 position, Size2 dimensions, float incrementSize = 1)
        {
            return GetAllPointsInArea(position.X, position.Y, dimensions.Width, dimensions.Height, incrementSize);
        }
        public static List<Vector2> GetAllPointsInArea(float startX, float startY, float width, float height, float incrementSize = 1)
        {
            return GetAllPointsInArea((int)startX, (int)startY, (int)width, (int)height);
        }
        public static List<Vector2> GetAllPointsInArea(int startX, int startY, int width, int height, int incrementSize = 1)
        {
            List<Vector2> rv = new List<Vector2>();

            for (int y = startY; y < startY + height; y += incrementSize)
            {
                for (int x = startX; x < startX + width; x += incrementSize)
                {
                    rv.Add(new Vector2(x, y));
                }
            }

            return rv;
        }

        public static T GetRandomItem<T>(List<T> theList)
        {
            if (theList.Count > 0) { return theList[RHRandom.Instance().Next(0, theList.Count - 1)]; }
            return default;
        }

        public static DirectionEnum GetOppositeDirection(DirectionEnum value)
        {
            switch (value)
            {
                case DirectionEnum.Down:
                    return DirectionEnum.Up;
                case DirectionEnum.Up:
                    return DirectionEnum.Down;
                case DirectionEnum.Left:
                    return DirectionEnum.Right;
                case DirectionEnum.Right:
                    return DirectionEnum.Left;
            }

            return DirectionEnum.Up;
        }

        public static void SwitchValues(ref int x, ref int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }
        public static void SwitchValues(ref RHSize x, ref RHSize y)
        {
            RHSize temp = x;
            x = y;
            y = temp;
        }
    }

    public class RHRandom : Random
    {
        private static RHRandom _randomizer;

        public static RHRandom Instance()
        {
            if(_randomizer == null) { _randomizer = new RHRandom(); }
            return _randomizer;
        }

        private RHRandom() : base() {}

        public override int Next(int min, int max)
        {
            int rv = 0;
            rv = base.Next(min, max + 1);
            return rv;
        }

        public bool RollPercent(int percent)
        {
            int roll = RHRandom.Instance().Next(1, 100);
            return roll <= percent;
        }
    }

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
}
