using Microsoft.Xna.Framework;
using RiverHollow.Characters.CombatStuff;

namespace RiverHollow
{
    public class Monster : CombatCharacter
    {
        #region Properties
        private int _id;
        public int ID { get => _id; }
        private int _xp;
        public int XP { get => _xp; }
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;

        #endregion

        public Monster(int id, string[] stringData)
        {
            ImportBasics(stringData, id);
            LoadContent(_textureName, 100, 100, 2, 0.2f);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            int i = 0;
            _name = stringData[i++];
            _textureName = @"Textures\" + stringData[i++];
            _xp = int.Parse(stringData[i++]);
            _statDmg = int.Parse(stringData[i++]);
            _statDef = int.Parse(stringData[i++]);
            _statHP = int.Parse(stringData[i++]);
            _statMagic = int.Parse(stringData[i++]);
            _statSpd = int.Parse(stringData[i++]);

            _currentHP = MaxHP;

            return i;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }
    }
}
