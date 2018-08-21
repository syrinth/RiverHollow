using RiverHollow.Actors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using RiverHollow.Actors.CombatStuff;
using System.IO;
using RiverHollow.Misc;
using System;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Game_Managers
{
    public static class ActorManager
    {
        private static Dictionary<int, Mob> _diMobs;
        private static Dictionary<int, string> _diMonsters;
        private static Dictionary<int, Villager> _diNPCs;
        public static Dictionary<int, Villager> DiNPC { get => _diNPCs; }
        private static Dictionary<int, string> _diActions;
        private static Dictionary<int, string> _diBuffs;
        private static Dictionary<int, string> _diClasses;
        private static Dictionary<string, Dictionary<string, string>> _diSchedule;

        static List<int> _liForest;
        static List<int> _liMountain;
        static List<int> _liNight;

        public static void LoadContent(ContentManager Content)
        {
            _liForest = new List<int>();
            _liMountain = new List<int>();
            _liNight = new List<int>();
            _diMobs = new Dictionary<int, Mob>();
            _diSchedule = new Dictionary<string, Dictionary<string, string>>();

            _diMonsters = Content.Load<Dictionary<int, string>>(@"Data\Monsters");
            _diActions = Content.Load<Dictionary<int, string>>(@"Data\CombatActions");
            _diBuffs = Content.Load<Dictionary<int, string>>(@"Data\Buffs");
            _diClasses = Content.Load<Dictionary<int, string>>(@"Data\Classes");

            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\Mobs"))
            {
                _diMobs.Add(kvp.Key, new Mob(kvp.Key, Util.FindTags(kvp.Value)));
            }

            foreach (string s in Directory.GetFiles(@"Content\Data\NPCData\Schedules"))
            {
                string temp = Path.GetFileNameWithoutExtension(s);
                _diSchedule.Add(temp, Content.Load<Dictionary<string, string>>(@"Data\NPCData\Schedules\" + temp));
            }

            _diNPCs = new Dictionary<int, Villager>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\NPCData\Characters"))
            {
                Villager n = null;
                string _characterData = kvp.Value;
                string[] _characterDataValues = Util.FindTags(_characterData);
                switch (_characterDataValues[0].Split(':')[1])
                {
                    case "ShopKeeper":
                        n = new ShopKeeper(kvp.Key, _characterDataValues);
                        break;
                    case "Eligible":
                        n = new EligibleNPC(kvp.Key, _characterDataValues);
                        break;
                    default:
                        n = new Villager(kvp.Key, _characterDataValues);
                        break;
                }
                _diNPCs.Add(kvp.Key, n);
            }
        }

        public static string GetCharacterNameByIndex(int i)
        {
            return _diNPCs[i].Name;
        }

        public static Monster GetMonsterByIndex(int id)
        {
            Monster m = null;
            if (_diMonsters.ContainsKey(id))
            {
                string _itemData = _diMonsters[id];
                string[] _itemDataValues = Util.FindTags(_itemData);
                m = new Monster(id, _itemDataValues);
            }
            return m;
        }

        public static Mob GetMobByIndex(int id)
        {
            return _diMobs[id];
        }

        public static Mob GetMobByIndex(int id, Vector2 pos)
        {
            Mob m = GetMobByIndex(id);
            m.Position = pos;
            return m;
        }

        public static MenuAction GetActionByIndex(int id)
        {
            if (id != -1)
            {
                string strData = _diActions[id];
                string[] strdataValues = Util.FindTags(strData);
                switch (strdataValues[0].Split(':')[1])
                {
                    case "Menu":
                        return new MenuAction(id, strdataValues);
                    case "Spell":
                        return new CombatAction(id, strdataValues);
                    case "Action":
                        return new CombatAction(id, strdataValues);
                }
            }

            return null;
        }
        public static Buff GetBuffByIndex(int id)
        {
            Buff b = null;
            if (id != -1)
            {
                string _stringData = _diBuffs[id];
                string[] _stringDataValues = _stringData.Split('/');
                b = new Buff(id, _stringDataValues);
            }
            return b;
        }

        public static int GetClassCount()
        {
            return _diClasses.Count;
        }
        public static CharacterClass GetClassByIndex(int id)
        {
            CharacterClass c = null;
            if (id != -1)
            {
                string strData = _diClasses[id];
                string[] strDataValues = Util.FindTags(strData);
                c = new CharacterClass(id, strDataValues);
            }
            return c;
        }

        public static Dictionary<string, string> GetSchedule(string npc)
        {
            Dictionary<string, string> rv = null;
            if (_diSchedule.ContainsKey(npc))
            {
                rv = _diSchedule[npc];
            }

            return rv;
        }

        public static void RollOver()
        {
            foreach(Villager n in _diNPCs.Values)
            {
                n.RollOver();
            }
        }

        #region Spawn Code
        public static void AddToForest(int ID) { _liForest.Add(ID); }
        public static void AddToMountain(int ID) { _liMountain.Add(ID); }
        public static void AddToNight(int ID) { _liNight.Add(ID); }
        internal static Mob GetMobToSpawn(SpawnConditionEnum eSpawnType)
        {
            List<Mob> allowedMobs = new List<Mob>();

            foreach(Mob m in _diMobs.Values)
            {
                if (m.CheckValidConditions(eSpawnType)){
                    allowedMobs.Add(m);
                }
            }


            return GetMobByIndex(new RHRandom().Next(1, allowedMobs.Count-1));
        }

        #endregion
    }
}
