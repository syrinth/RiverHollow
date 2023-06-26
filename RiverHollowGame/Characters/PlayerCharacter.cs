using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using RiverHollow.WorldObjects;

namespace RiverHollow.Characters
{
    public class PlayerCharacter : CombatActor
    {
        AnimatedSprite _sprEyes;
        public AnimatedSprite EyeSprite => _sprEyes;
        AnimatedSprite _sprHair;
        public AnimatedSprite HairSprite => _sprHair;
        public Color HairColor { get; private set; } = Color.White;
        public Color EyeColor { get; private set; } = Color.White;
        public int HairIndex { get; private set; } = 0;
        public int BodyType { get; private set; } = 1;
        public string BodyTypeStr => BodyType.ToString("00");

        public bool CanBecomePregnant { get; set; }
        public bool Pregnant { get; set; }

        public override List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { BodySprite, _sprEyes, _sprHair, Chest?.Sprite, Hat?.Sprite, Legs?.Sprite };
            liRv.RemoveAll(x => x == null);
            return liRv;
        }

        public override Rectangle CollisionBox => ActiveMount != null ? ActiveMount.CollisionBox : base.CollisionBox;
        public override Rectangle HitBox => new Rectangle(Position.X + 2, Position.Y + 22, 12, 10);

        #region Clothing
        public Clothing Hat { get; private set; }
        public Clothing Chest { get; private set; }
        Clothing Back;
        Clothing Hands;
        public Clothing Legs { get; private set; }
        Clothing Feet;
        #endregion

        Light _lightSource;

        public Pet ActivePet { get; private set; }
        public Mount ActiveMount { get; private set; }
        public bool Mounted => ActiveMount != null;

        public override float MaxHP => PlayerManager.MaxPlayerHP(); 

        public PlayerCharacter() : base()
        {
            HairColor = Color.Red;
            EyeColor = Color.Blue;

            CurrentHP = MaxHP;

            _liTilePath = new List<RHTile>();

            // SetClothes((Clothes)DataManager.GetItem(int.Parse(DataManager.Config[6]["ItemID"])));

            BodySprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17]), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, BodyTypeStr));

            //Hair type has already been set either by default or by being allocated.
            SetHairType(HairIndex);

            //Loads the Sprites for the players body for the appropriate class
            _sprEyes = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17]), string.Format(@"{0}Eyes", DataManager.FOLDER_PLAYER));

            BodySprite.SetColor(Color.White);
            _sprHair.SetColor(HairColor);
            _sprEyes.SetColor(EyeColor);

            SpdMult = Constants.NORMAL_SPEED;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (_lightSource != null)
            {
                _lightSource.Position = Position - new Point((_lightSource.Width - Width) / 2, (_lightSource.Height - Height) / 2);
            }

            if (HasKnockbackVelocity())
            {
                ApplyKnockbackVelocity();
            }

            CheckDamageTimers(gTime);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);

            //_sprEyes.Draw(spriteBatch, useLayerDepth);
            //_sprHair.Draw(spriteBatch, useLayerDepth);

            //Chest?.Sprite.Draw(spriteBatch, useLayerDepth);
            //Hat?.Sprite.Draw(spriteBatch, useLayerDepth);
            //Legs?.Sprite.Draw(spriteBatch, useLayerDepth);
        }
        protected override void HandleMove()
        {
            base.HandleMove();

            if (!HasKnockbackVelocity() && PlayerManager.GrabbedObject != null && PlayerManager.MoveObjectToPosition != Point.Zero && PlayerManager.GrabbedObject.CollisionPosition != PlayerManager.MoveObjectToPosition)
            {
                PlayerManager.MoveGrabbedObject(Util.GetMoveSpeed(PlayerManager.GrabbedObject.CollisionPosition, PlayerManager.MoveObjectToPosition, BuffedSpeed));
            }
        }

        public void NewLantern()
        {
            _lightSource = DataManager.GetLight(7 + PlayerManager.LanternLevel);
        }
        public void DrawLight(SpriteBatch spriteBatch)
        {
            _lightSource?.Draw(spriteBatch);
        }

        private List<AnimationData> LoadPlayerAnimations(Dictionary<string, string> data)
        {
            List<AnimationData> rv;
            rv = Util.LoadPlayerAnimations(data);

            Util.AddToAnimationsList(ref rv, data, VerbEnum.UseTool, true, true);
            Util.AddToAnimationsList(ref rv, data, AnimationEnum.Pose);
            return rv;
        }

        public void SetColor(AnimatedSprite sprite, Color c)
        {
            sprite.SetColor(c);
        }

        public void SetHairColor(Color c)
        {
            HairColor = c;
            SetColor(_sprHair, c);
        }

        public void SetEyeColor(Color c)
        {
            EyeColor = c;
            SetColor(_sprEyes, c);
        }
        public void SetHairType(int index)
        { 
            HairIndex = index;
            //Loads the Sprites for the players hair animations for the class based off of the hair ID
            _sprHair = LoadSpriteAnimations(Util.LoadPlayerAnimations(DataManager.Config[17]), string.Format(@"{0}Hairstyles\Hair_{1}", DataManager.FOLDER_PLAYER, HairIndex));
            _sprHair.SetLayerDepthMod(Constants.HAIR_DEPTH);
            _sprHair.SetColor(HairColor);
        }

        public override void PlayAnimation<TEnum>(TEnum anim)
        {
            GetSprites().ForEach(spr => spr.PlayAnimation(anim));
            Chest?.Sprite.PlayAnimation(anim);
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            if (verb == VerbEnum.Walk && ActiveMount != null) { verb = VerbEnum.Idle; }

            GetSprites().ForEach(spr => spr.PlayAnimation(verb, dir));
            Chest?.Sprite.PlayAnimation(verb, dir);
        }

        public void SetScale(int scale = 1)
        {
            GetSprites().ForEach(spr => spr.SetScale(scale));
        }

        public void SetClothes(Clothing c)
        {
            if (c != null)
            {
                string clothingTexture = string.Format(@"Textures\Items\Clothing\{0}\{1}", c.ClothingType.ToString(), c.TextureKey);
                if (!c.GenderNeutral) { clothingTexture += ("_" + BodyTypeStr); }

                c.Sprite = LoadSpriteAnimations(Util.LoadPlayerAnimations(DataManager.Config[17]), clothingTexture);

                if (c.SlotMatch(ClothingEnum.Chest)) { Chest = c; }
                else if (c.SlotMatch(ClothingEnum.Hat))
                {
                    _sprHair.FrameCutoff = 9;
                    Hat = c;
                }
                else if (c.SlotMatch(ClothingEnum.Legs)) { Legs = c; }

                //MAR AWKWARD
                c.Sprite.Position = Position;
                c.Sprite.PlayAnimation(BodySprite.CurrentAnimation);
                c.Sprite.SetLayerDepthMod(0.004f);
            }
        }

        public void RemoveClothes(ClothingEnum c)
        {
            if (c.Equals(ClothingEnum.Chest)) { Chest = null; }
            else if (c.Equals(ClothingEnum.Hat))
            {
                _sprHair.FrameCutoff = 0;
                Hat = null;
            }
        }

        public void SetBodyType(int val)
        {
            BodyType = val;
            SetClothes(Hat);
            SetClothes(Chest);
            SetClothes(Legs);
        }

        public void SetPet(Pet actor)
        {
            if (actor == null) { ActivePet.SetFollow(false); }

            ActivePet = actor;
            ActivePet?.SetFollow(true);
        }

        public void MountUp(Mount actor)
        {
            ActiveMount = actor;
            SpdMult = 1.5f;

            SetPosition(ActiveMount.Position + new Point((ActiveMount.Width - Width) / 2, 8));

            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(ActiveMount.BodySprite);
            }
        }
        public void Dismount()
        {
            SetPosition(ActiveMount.Position + new Point(Constants.TILE_SIZE, 0));
            ActiveMount = null;
            SpdMult = Constants.NORMAL_SPEED;

            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(null);
            }
        }

        public override bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = base.DealDamage(value, hitbox);
            if (rv && !HasHP)
            {
                PlayerManager.FinishedWithTool();
                Kill();
            }

            return rv;
        }
        protected override void Flicker(bool value)
        {
            base.Flicker(value);
            GetSprites().ForEach(x => x.SetColor(Color.White * (_bFlicker ? 1 : 0)));
        }

        public override void SetMoveTo(Point v, bool update = true)
        {
            base.SetMoveTo(v, State != ActorStateEnum.Grab);
        }
    }
}
