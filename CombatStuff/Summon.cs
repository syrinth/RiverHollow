using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class Summon : CombatCharacter
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;

        int _iMagStat;

        public bool Acted;
        public bool Swapped;
        bool _bTwinCast;
        public bool TwinCast => _bTwinCast;
        bool _bAggressive;
        public bool Aggressive => _bAggressive;
        bool _bDefensive;
        public bool Defensive => _bDefensive;

        public CombatCharacter linkedChar;

        public Summon()
        {
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\Eye"));
            _bodySprite.AddAnimation("Walk", 0, 0, 16, 16, 2, 0.9f);
            _bodySprite.AddAnimation("Attack", 32, 0, 16, 16, 4, 0.1f);
            _bodySprite.AddAnimation("Cast", 32, 0, 16, 16, 4, 0.1f);
            _bodySprite.SetCurrentAnimation("Idle");
            _bodySprite.SetScale(5);
        }

        public Summon Clone()
        {
            Summon copy = new Summon();
            copy.SetStats(_iMagStat);
            if (TwinCast) { copy.SetTwincast(); }
            if (Aggressive) { copy.SetAggressive(); }
            if (Counter) { copy.Counter = Counter; }
            if (Defensive) { copy.SetDefensive(); }

            copy._element = _element;
            copy.Tile = Tile;

            return copy;
        }

        public void SetStats(int magStat)
        {
            _iMagStat = magStat;
            _statStr = 2 * magStat + 10;
            _statDef = 2 * magStat + 10;
            _statVit = (3 * magStat) + 80;
            _statMag = 2 * magStat + 10;
            _statRes = 2 * magStat + 10;
            _statSpd = 10;

            CurrentHP = MaxHP;
        }

        public override int DecreaseHealth(int value)
        {
            int rv = base.DecreaseHealth(value);

            if (CurrentHP == 0)
            {
                linkedChar.UnlinkSummon();
            }

            return rv;
        }
        public override GUISprite GetSprite()
        {
            return Tile.GUITile.SummonSprite;
        }

        public void SetTwincast() { _bTwinCast = true; }
        public void SetAggressive() { _bAggressive = true; }
        public void SetDefensive() { _bDefensive = true; }
        public void SetElement(ElementEnum el) { _element = el; }

        public override bool IsSummon() { return true; }
    }
}
