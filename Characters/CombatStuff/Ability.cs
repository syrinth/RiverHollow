using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System;

namespace RiverHollow.Characters.CombatStuff
{
    public class Ability
    {
        private int _id;
        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        private int _dmg;
        public int Dmg { get => _dmg; }
        private Rectangle _sourceRect;
        public Rectangle SourceRect { get => _sourceRect; }

        private Texture2D _texture;
        public Ability(int id, string[] stringData)
        {
            ImportBasics(id, stringData);

            _texture = GameContentManager.GetTexture(@"Textures\Abilities");
        }

        protected int ImportBasics(int id, string[] stringData)
        {
            int i = 0;
            _name = stringData[i++];
            _description = stringData[i++];
            _dmg = int.Parse(stringData[i++]);
            string[] split = stringData[i++].Split(' ');
            int x = int.Parse(split[0]);
            int y = int.Parse(split[1]);
            _sourceRect = new Rectangle(x * 100, y * 100, 100, 100);

            _id = id;

            return i;
        }
    }
}
