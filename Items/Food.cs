using Adventure.Game_Managers;
using System;

namespace Adventure.Items
{
    public class Food : Item
    {
        private int _stam;
        public int Stamina { get => _stam; }
        private int _health;
        public int Health { get => _health; }

        public Food(int id, string[] itemValue, int num)
        {
            if (itemValue.Length == 8)
            {
                int i = ImportBasics(itemValue, id, num);
                _stam = int.Parse(itemValue[i++]);
                _health = int.Parse(itemValue[i++]);

                _doesItStack = true;
                _texture = GameContentManager.GetTexture(@"Textures\items");

                CalculateSourcePos();
            }
        }

    }
}
