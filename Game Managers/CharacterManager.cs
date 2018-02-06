using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiverHollow.Characters.CombatStuff;
using System.IO;

namespace RiverHollow.Game_Managers
{
    public static class CharacterManager
    {
        private static Dictionary<int, string> _mobDictionary;
        private static Dictionary<int, string> _monsterDictionary;
        private static Dictionary<int, NPC> _npcDictionary;
        public static Dictionary<int, NPC> DiNPC { get => _npcDictionary; }
        private static Dictionary<int, string> _abilityDictionary;
        private static Dictionary<int, string> _buffDictionary;
        private static Dictionary<int, string> _classDictionary;
        private static Dictionary<string, Dictionary<string, string>> _dictSchedule;

        public static void LoadContent(ContentManager Content)
        {
            _dictSchedule = new Dictionary<string, Dictionary<string, string>>();
            _monsterDictionary = Content.Load<Dictionary<int, string>>(@"Data\Monsters");
            _mobDictionary = Content.Load<Dictionary<int, string>>(@"Data\Mobs");
            _abilityDictionary = Content.Load<Dictionary<int, string>>(@"Data\Abilities");
            _buffDictionary = Content.Load<Dictionary<int, string>>(@"Data\Buffs");
            _classDictionary = Content.Load<Dictionary<int, string>>(@"Data\Classes");
            
            foreach (string s in Directory.GetFiles(@"Content\Data\NPCData\Schedules"))
            {
                string temp = Path.GetFileNameWithoutExtension(s);
                _dictSchedule.Add(temp, Content.Load<Dictionary<string, string>>(@"Data\NPCData\Schedules\" + temp));
            }

            _npcDictionary = new Dictionary<int, NPC>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\NPCData\Characters"))
            {
                NPC n = null;
                string _characterData = kvp.Value;
                string[] _characterDataValues = _characterData.Split('/');
                switch (_characterDataValues[0])
                {
                    case "Shopkeeper":
                        n = new ShopKeeper(kvp.Key, _characterDataValues);
                        break;
                    default:
                        n = new NPC(kvp.Key, _characterDataValues);
                        break;
                }
                _npcDictionary.Add(kvp.Key, n);
            }
        }

        public static string GetCharacterNameByIndex(int i)
        {
            return _npcDictionary[i].Name;
        }

        public static Monster GetMonsterByIndex(int id)
        {
            Monster m = null;
            if (_monsterDictionary.ContainsKey(id))
            {
                string _itemData = _monsterDictionary[id];
                string[] _itemDataValues = _itemData.Split('/');
                m = new Monster(id, _itemDataValues);
            }
            return m;
        }

        public static Mob GetMobByIndex(int id)
        {
            Mob m = null;
            if (id != -1)
            {
                string _itemData = _mobDictionary[id];
                string[] _itemDataValues = _itemData.Split('/');
                m = new Mob(id, _itemDataValues);
            }
            return m;
        }

        public static Mob GetMobByIndex(int id, Vector2 pos)
        {
            Mob m = GetMobByIndex(id);
            m.Position = pos;
            return m;
        }

        public static Ability GetAbilityByIndex(int id)
        {
            Ability a = null;
            if (id != -1)
            {
                string _stringData = _abilityDictionary[id];
                string[] _stringDataValues = _stringData.Split('/');
                a = new Ability(id, _stringDataValues);
            }
            return a;
        }
        public static Buff GetBuffByIndex(int id)
        {
            Buff b = null;
            if (id != -1)
            {
                string _stringData = _buffDictionary[id];
                string[] _stringDataValues = _stringData.Split('/');
                b = new Buff(id, _stringDataValues);
            }
            return b;
        }

        public static CharacterClass GetClassByIndex(int id)
        {
            CharacterClass c = null;
            if (id != -1)
            {
                string _stringData = _classDictionary[id];
                string[] _stringDataValues = _stringData.Split('/');
                c = new CharacterClass(id, _stringDataValues);
            }
            return c;
        }

        public static Dictionary<string, string> GetSchedule(string npc)
        {
            Dictionary<string, string> rv = null;
            if (_dictSchedule.ContainsKey(npc))
            {
                rv = _dictSchedule[npc];
            }

            return rv;
        }

        public static void RollOver()
        {
            foreach(NPC n in _npcDictionary.Values)
            {
                n.RollOver();
            }
        }
    }
}
