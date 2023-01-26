using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    /// <summary>
    /// This class represents a combat actor, as well as the display information for them
    /// </summary>
    public class GUICombatActorInfo : GUIObject
    {
        CombatActor _actor;
        GUIHealthBar _gHP;
        GUICombatActor _gLiteCombatActor;
        public GUISprite CharacterSprite => _gLiteCombatActor.CharacterSprite;
        public GUISprite CharacterWeaponSprite => _gLiteCombatActor.CharacterWeaponSprite;
        public GUICombatTile AssignedTile { get; private set; }

        public GUICombatActorInfo(CombatActor actor)
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
            if (_actor.CurrentHP == 0 || (CombatManager.CurrentPhase == CombatManager.PhaseEnum.PerformAction && CombatManager.ActiveCharacter == _actor))
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
            if (_actor.IsActorType(CombatActorTypeEnum.PartyMember))
            {
                ClassedCombatant adv = (ClassedCombatant)_actor;
                Job cClass = adv.CharacterClass;

                string textureName = DataManager.FOLDER_ITEMS + "Gear\\Weapons\\" + cClass.WeaponType.ToString() + "\\" + adv.GetEquipment(GearTypeEnum.Weapon).ID;
                AnimatedSprite sprWeaponSprite = null;
                LoadSpriteAnimations(ref sprWeaponSprite, Util.LoadCombatAnimations(cClass.ClassStringData), textureName);

                _gLiteCombatActor.SetWeapon(sprWeaponSprite);
            }
        }

        protected void LoadSpriteAnimations(ref AnimatedSprite sprite, List<AnimationData> listAnimations, string textureName)
        {
            sprite = new AnimatedSprite(textureName);

            foreach (AnimationData data in listAnimations)
            {
                sprite.AddAnimation(data.Animation, data.XLocation, data.YLocation, _actor.Width, _actor.Height, data.Frames, data.FrameSpeed, data.PingPong);
            }

            PlayAnimation(AnimationEnum.Idle);
            sprite.SetScale(Constants.NORMAL_SCALE);
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
    }
}
