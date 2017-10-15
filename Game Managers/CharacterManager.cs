﻿using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    public static class CharacterManager
    {
        private static Dictionary<int, string> _mobDictionary;
        private static Dictionary<int, string> _monsterDictionary;
        private static Dictionary<int, NPC> _characterDictionary;
        public static Dictionary<string, bool> _talkedTo;

        public static void LoadContent(ContentManager Content)
        {
            _monsterDictionary = Content.Load<Dictionary<int, string>>(@"Data\Monsters");
            _mobDictionary = Content.Load<Dictionary<int, string>>(@"Data\Mobs");

            _talkedTo = new Dictionary<string, bool>();
            _characterDictionary = new Dictionary<int, NPC>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\Characters"))
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
                _characterDictionary.Add(kvp.Key, n);
                _talkedTo.Add(n.Name, false);
            }
        }

        public static string GetCharacterNameByIndex(int i)
        {
            return _characterDictionary[i].Name;
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
    }
}
