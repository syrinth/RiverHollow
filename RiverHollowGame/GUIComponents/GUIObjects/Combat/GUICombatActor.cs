using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;

namespace RiverHollow.GUIComponents.GUIObjects.Combat
{
    public class GUICombatActor : GUIObject
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
