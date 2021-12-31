using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    /// <summary>
    /// This class represents a combat actor, as well as the display information for them
    /// </summary>
    public class GUILiteCombatActorInfo : GUIObject
    {
        CombatActor _actor;
        GUIHealthBar _gHP;
        GUICombatActor _gLiteCombatActor;
        public GUISprite CharacterSprite => _gLiteCombatActor.CharacterSprite;
        public GUISprite CharacterWeaponSprite => _gLiteCombatActor.CharacterWeaponSprite;
        public GUICombatTile AssignedTile { get; private set; }

        public GUILiteCombatActorInfo(CombatActor actor)
        {
            _actor = actor;
            _gLiteCombatActor = new GUICombatActor(actor.BodySprite);
            AddControl(_gLiteCombatActor);

            SetWeapon();

            _gLiteCombatActor.CharacterSprite.Reset();
            _gLiteCombatActor.CharacterWeaponSprite?.Reset();

            _gHP = new GUIHealthBar(actor.CurrentHP, actor.MaxHP);
            _gHP.AnchorAndAlignToObject(_gLiteCombatActor, SideEnum.Bottom, SideEnum.CenterX);
            _gHP.ScaledMoveBy(0, 1);
            AddControl(_gHP);


            Width = _gLiteCombatActor.Width;
            Height = _gLiteCombatActor.Bottom - _gLiteCombatActor.Top;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.PerformAction && CombatManager.ActiveCharacter == _actor)
            {
                _gHP.Show(false);
            }

            foreach (GUIObject g in Controls)
            {
                g.Draw(spriteBatch);
            }

            _gHP.Show(true);
        }

        public void UpdateHealthBar()
        {
            _gHP.SetCurrentValue(_actor.CurrentHP);
        }

        public void SetWeapon()
        {
            if (_actor.IsActorType(ActorEnum.PartyMember))
            {
                ClassedCombatant adv = (ClassedCombatant)_actor;
                CharacterClass cClass = adv.CharacterClass;

                AnimatedSprite sprWeaponSprite = new AnimatedSprite(DataManager.FOLDER_ITEMS + "Combat\\Weapons\\" + cClass.WeaponType.ToString() + "\\" + adv.GetEquipment(EquipmentEnum.Weapon).ItemID);

                int xCrawl = 0;
                RHSize frameSize = new RHSize(2, 2);
                sprWeaponSprite.AddAnimation(CombatActionEnum.Idle, xCrawl, 0, frameSize, 2, 0.5f);// cClass.IdleFrames, cClass.IdleFramesLength);
                xCrawl += 2 * frameSize.Width; //cClass.IdleFrames;
                sprWeaponSprite.AddAnimation(CombatActionEnum.Cast, (xCrawl * TILE_SIZE), 0, frameSize, 3, 0.4f);//cClass.CastFrames, cClass.CastFramesLength);
                xCrawl += 3 * frameSize.Width; //cClass.CastFrames;
                sprWeaponSprite.AddAnimation(CombatActionEnum.Hurt, (xCrawl * TILE_SIZE), 0, frameSize, 1, 0.5f);//cClass.HitFrames, cClass.HitFramesLength);
                xCrawl += 1 * frameSize.Width; //cClass.HitFrames;
                sprWeaponSprite.AddAnimation(CombatActionEnum.Attack, (xCrawl * TILE_SIZE), 0, frameSize, 1, 0.3f);//cClass.AttackFrames, cClass.AttackFramesLength);
                xCrawl += 1 * frameSize.Width; //cClass.AttackFrames;
                sprWeaponSprite.AddAnimation(CombatActionEnum.Critical, (xCrawl * TILE_SIZE), 0, frameSize, 2, 0.9f);//cClass.CriticalFrames, cClass.CriticalFramesLength);
                xCrawl += 2 * frameSize.Width; //cClass.CriticalFrames;
                sprWeaponSprite.AddAnimation(CombatActionEnum.KO, (xCrawl * TILE_SIZE), 0, frameSize, 1, 0.5f);//cClass.KOFrames, cClass.KOFramesLength);
                xCrawl += 1 * frameSize.Width; //cClass.KOFrames;
                sprWeaponSprite.AddAnimation(CombatActionEnum.Victory, (xCrawl * TILE_SIZE), 0, frameSize, 2, 0.5f);//cClass.WinFrames, cClass.WinFramesLength);
                sprWeaponSprite.SetScale(GameManager.CurrentScale);

                _gLiteCombatActor.SetWeapon(sprWeaponSprite);
            }
        }

        public void AssignTile(GUICombatTile tile)
        {
            AssignedTile = tile;
        }

        public void Reset()
        {
            _gLiteCombatActor.Reset();
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            _gLiteCombatActor.PlayAnimation(animation);
        }

        private class GUICombatActor : GUIObject
        {
            public GUISprite CharacterSprite { get; }
            public GUISprite CharacterWeaponSprite { get; private set; }

            public GUICombatActor(AnimatedSprite sprite)
            {
                CharacterSprite = new GUISprite(sprite);
                AddControl(CharacterSprite);

                Width = CharacterSprite.Width;
                Height = CharacterSprite.Height;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                foreach (GUIObject g in Controls)
                {
                    g.Draw(spriteBatch);
                }
            }

            public void SetWeapon(AnimatedSprite sprite)
            {
                CharacterWeaponSprite = new GUISprite(sprite);
                AddControl(CharacterWeaponSprite);
            }

            public void Reset()
            {
                CharacterSprite.Reset();
                if (CharacterWeaponSprite != null) { CharacterWeaponSprite.Reset(); }
            }

            public void PlayAnimation<TEnum>(TEnum animation)
            {
                CharacterSprite.PlayAnimation(animation);
                if (CharacterWeaponSprite != null) { CharacterWeaponSprite.PlayAnimation(animation); }
            }
        }
    }
}
