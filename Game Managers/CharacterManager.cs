using Adventure.Characters;
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

        public static void LoadContent(ContentManager Content)
        {
            _characterDictionary = new Dictionary<int, NPC>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\Characters"))
            {
                string _characterData = kvp.Value;
                string[] _characterDataValues = _characterData.Split('/');
                _characterDictionary.Add(kvp.Key, new NPC(_characterDataValues));
            }
        }

        public static string GetCharacterNameByIndex(int i)
        {
            return _characterDictionary[i].Name;
        }
    }
}
