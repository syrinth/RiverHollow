﻿using Adventure.Characters;
using Adventure.Characters.NPCs;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public static class CharacterManager
    {
        private static Dictionary<int, NPC> _characterDictionary;
        public static Dictionary<string, bool> _talkedTo;

        public static void LoadContent(ContentManager Content)
        {
            _talkedTo = new Dictionary<string, bool>();
            _characterDictionary = new Dictionary<int, NPC>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\Characters"))
            {
                NPC n;
                string _characterData = kvp.Value;
                string[] _characterDataValues = _characterData.Split('/');
                if(_characterDataValues.Length == 4){
                    n = new NPC(kvp.Key, _characterDataValues);
                }
                else
                {
                    n = new ShopKeeper(kvp.Key, _characterDataValues);
                }
                _characterDictionary.Add(kvp.Key, n);
                _talkedTo.Add(n.Name, false);
            }
        }

        public static string GetCharacterNameByIndex(int i)
        {
            return _characterDictionary[i].Name;
        }
    }
}
