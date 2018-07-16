using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class Summon : CombatCharacter
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;

        public bool HasAttacked;

        public Summon()
        {
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\Eye"));
            _bodySprite.AddAnimation("Idle", 0, 0, 16, 16, 2, 0.9f);
            _bodySprite.AddAnimation("Attack", 32, 0, 16, 16, 4, 0.1f);
            _bodySprite.SetCurrentAnimation("Idle");
            _bodySprite.SetScale(5);
        }

        public void SetStats(int magStat)
        {
            _statStr = 2 * magStat + 10;
            _statDef = 2 * magStat + 10;
            _statVit = (3 * magStat) + 80;
            _statMag = 2 * magStat + 10;
            _statRes = 2 * magStat + 10;
            _statSpd = 10;
        }
    }
}
