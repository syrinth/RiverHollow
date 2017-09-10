using Adventure.Game_Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class AdventureMap : Item
    {
        private int _difficulty;
        public int Difficulty { get => _difficulty; }
        public AdventureMap(int id, string[] itemValue, int num)
        {
            int i = ImportBasics(itemValue, id, num);
            _difficulty = RandNumber(4, 5, 0, 0);

            _doesItStack = false;
            _texture = GameContentManager.GetTexture(@"Textures\items");

            CalculateSourcePos();
        }
    }
}
