using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using RiverHollow.WorldObjects;
using RiverHollow.GUIComponents;

using static RiverHollow.Utilities.Enums;
using MonoGame.Extended.Sprites;
using System;

namespace RiverHollow.Characters
{
    public class PlayerCharacter : CombatActor
    {
        public AnimatedSprite EyeSprite { get; private set; }
        public AnimatedSprite HairSprite { get; private set; }
        public Color HairColor { get; private set; } = Color.White;
        public Color EyeColor { get; private set; } = Color.White;
        public int HairIndex { get; private set; } = 0;
        public int BodyType { get; private set; } = 1;
        public string BodyTypeStr => BodyType.ToString("00");

        public bool CanBecomePregnant { get; set; }
        public bool Pregnant { get; set; }

        public Item[,] PlayerGear;

        public override List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { BodySprite, EyeSprite, HairSprite, Chest?.Sprite, Hat?.Sprite, Legs?.Sprite };
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

            PlayerGear = new Item[Constants.PLAYER_GEAR_ROWS, Constants.PLAYER_GEAR_COLUMNS];
            _liTilePath = new List<RHTile>();

            // SetClothes((Clothes)DataManager.GetItem(int.Parse(DataManager.Config[6]["ItemID"])));

#if DEBUG
            BodySprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17]), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, "03"));// BodyTypeStr));

            //Hair type has already been set either by default or by being allocated.
            SetHairType(HairIndex);

            //Loads the Sprites for the players body for the appropriate class
            EyeSprite = new AnimatedSprite(string.Format(@"{0}Eyes", DataManager.FOLDER_PLAYER));
            EyeSprite.AddAnimation(DirectionEnum.Down, 0, 0, new Point(1, 1));
            EyeSprite.AddAnimation(DirectionEnum.Right, Constants.TILE_SIZE, 0, new Point(1, 1));
            EyeSprite.AddAnimation(DirectionEnum.Up, Constants.TILE_SIZE * 2, 0, new Point(1, 1));
            EyeSprite.AddAnimation(DirectionEnum.Left, Constants.TILE_SIZE, 0, new Point(1, 1));
            EyeSprite.GetFrameAnimation(Util.GetEnumString(DirectionEnum.Left)).Flip = true;
            EyeSprite.SetLinkedSprite(BodySprite, false);

            BodySprite.SetColor(Color.White);
            HairSprite.SetColor(HairColor);
            EyeSprite.SetColor(EyeColor);
#else
            BodySprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17]), string.Format(@"{0}Body_01", DataManager.FOLDER_PLAYER));// BodyTypeStr));
#endif
            _lightSource = DataManager.GetLight(7);

            SpdMult = Constants.NORMAL_SPEED;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (_lightSource != null)
            {
                _lightSource.Position = Position - new Point((_lightSource.Width - Width) / 2, (_lightSource.Height - Height) / 2);
            }

            SyncSprite(HairSprite);
            SyncSprite(EyeSprite);

            if (HasKnockbackVelocity())
            {
                ApplyKnockbackVelocity();
            }

            CheckDamageTimers(gTime);
        }

        private void SyncSprite(AnimatedSprite spr)
        {
            var currFrame = BodySprite.CurrentFrameAnimation;
            var frameIndex = currFrame.CurrentFrame;
            switch (frameIndex)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                    spr.Position = Position - new Point(0, 1);
                    break;
                case 2:
                case 6:
                    spr.Position = Position - new Point(0, 2);
                    break;
                default:
                    spr.Position = Position;
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);

            if (OnTheMap && Constants.DRAW_ADJACENCY)
            {
                foreach (var r in PlayerManager.AdjacencyRects)
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), r, GUIUtils.BLACK_BOX, Color.Red * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, GetSprites()[0].LayerDepth - 1);
                }
            }

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
            SetColor(HairSprite, c);
        }

        public void SetEyeColor(Color c)
        {
            EyeColor = c;
            SetColor(EyeSprite, c);
        }
        public void SetHairType(int index)
        { 
            HairIndex = index;
            var size = new Point(1, 1);

            int row = HairIndex / Constants.PLAYER_HAIR_COLUMNS;
            int column = (HairIndex - (row * Constants.PLAYER_HAIR_COLUMNS)) * 4;
            int xCrawl = 0;

            HairSprite = new AnimatedSprite(string.Format(@"{0}Hair", DataManager.FOLDER_PLAYER));
            foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
            {
                if (e == DirectionEnum.None) { continue; }

                HairSprite.AddAnimation(e, (column * Constants.TILE_SIZE) + xCrawl, (row * Constants.TILE_SIZE), size);
                xCrawl += Constants.TILE_SIZE;
            }

            HairSprite.SetColor(HairColor);
            HairSprite.SetLinkedSprite(BodySprite, false);
        }

        public override void PlayAnimation<TEnum>(TEnum anim)
        {
            BodySprite.PlayAnimation(anim);
            Chest?.Sprite.PlayAnimation(anim);
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            if (verb == VerbEnum.Walk && ActiveMount != null) { verb = VerbEnum.Idle; }

            BodySprite.PlayAnimation(verb, dir);
            Chest?.Sprite.PlayAnimation(verb, dir);
        }

        public override void SetFacing(DirectionEnum dir)
        {
            base.SetFacing(dir);
            HairSprite.PlayAnimation(dir);
            EyeSprite.PlayAnimation(dir);
        }

        public void SetScale(int scale = 1)
        {
            GetSprites().ForEach(spr => spr.SetScale(scale));
        }

        public void SetClothes(Clothing c)
        {
            if (c != null)
            {
                //c.Sprite = LoadSpriteAnimations(Util.LoadPlayerAnimations(DataManager.Config[17]), c.Texture.Name);

                //if (c.SlotMatch(ClothingEnum.Shirt)) { Chest = c; }
                //else if (c.SlotMatch(ClothingEnum.Hat))
                //{
                //    _sprHair.FrameCutoff = 9;
                //    Hat = c;
                //}
                //else if (c.SlotMatch(ClothingEnum.Pants)) { Legs = c; }

                ////MAR AWKWARD
                //c.Sprite.Position = Position;
                //c.Sprite.PlayAnimation(BodySprite.CurrentAnimation);
                //c.Sprite.SetLayerDepthMod(0.004f);
            }
        }

        public void RemoveClothes(EquipmentEnum c)
        {
            if (c.Equals(EquipmentEnum.Shirt)) { Chest = null; }
            else if (c.Equals(EquipmentEnum.Hat))
            {
                HairSprite.FrameCutoff = 0;
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
                spr.SetLinkedSprite(ActiveMount.BodySprite, true);
            }
        }
        public void Dismount()
        {
            SetPosition(ActiveMount.Position + new Point(Constants.TILE_SIZE, 0));
            ActiveMount = null;
            SpdMult = Constants.NORMAL_SPEED;

            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(null, false);
            }
        }

        public override bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = base.DealDamage(value, hitbox);
            if (rv)
            {
                GameManager.ExitTownMode();
                if (!HasHP)
                {
                    PlayerManager.FinishedWithTool();
                    Kill();
                }
            }

            return rv;
        }
        protected override void Flicker(bool value)
        {
            base.Flicker(value);
            BodySprite.SetColor(Color.White * (_bFlicker ? 1 : 0));
            HairSprite?.SetColor(HairColor * (_bFlicker ? 1 : 0));
            EyeSprite?.SetColor(EyeColor * (_bFlicker ? 1 : 0));
        }

        public override void SetMoveTo(Point v, bool update = true)
        {
            base.SetMoveTo(v, State != ActorStateEnum.Grab);
        }
    }
}
