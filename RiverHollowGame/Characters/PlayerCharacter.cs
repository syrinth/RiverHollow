using System;
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

namespace RiverHollow.Characters
{
    public class PlayerCharacter : CombatActor
    {
        public AnimatedSprite EyeSprite { get; private set; }
        public AnimatedSprite HairSprite { get; private set; }
        public AnimatedSprite ArmSprite { get; private set; }
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
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { BodySprite, PantsSprite, ShirtSprite, ArmSprite, EyeSprite, HairSprite, HatSprite };
            liRv.RemoveAll(x => x == null);
            return liRv;
        }

        public override Rectangle CollisionBox => ActiveMount != null ? ActiveMount.CollisionBox : base.CollisionBox;
        public override Rectangle HitBox => new Rectangle(Position.X + 2, Position.Y + 22, 12, 10);

        #region Clothing
        public Clothing Hat => PlayerGear[0, 0] as Clothing;
        public AnimatedSprite HatSprite { get; private set; }
        public Clothing Shirt => PlayerGear[1, 0] as Clothing;
        public AnimatedSprite ShirtSprite { get; private set; }
        public Clothing Pants => PlayerGear[2, 0] as Clothing;
        public AnimatedSprite PantsSprite { get; private set; }
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

#if DEBUG
            BodySprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17], Point.Zero), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, "03"));// BodyTypeStr));
            ArmSprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17], new Point(0, 35)), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, "03"));// BodyTypeStr));

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

            SyncSprite(HairSprite, 0);
            SyncSprite(EyeSprite, 0);
            SyncSprite(HatSprite, Constants.PLAYER_HAT_OFFSET);
            SyncSprite(ShirtSprite, Constants.PLAYER_SHIRT_OFFSET);

            if (PantsSprite != null)
            {
                PantsSprite.Position = BodySprite.Position + new Point(0, Constants.PLAYER_PANTS_OFFSET);
            }

            if (HasKnockbackVelocity())
            {
                ApplyKnockbackVelocity();
            }

            CheckDamageTimers(gTime);
        }

        private void SyncSprite(AnimatedSprite spr, int mod)
        {
            if (spr != null)
            {
                var currFrame = BodySprite.CurrentFrameAnimation;
                var frameIndex = currFrame.CurrentFrame;
                switch (frameIndex)
                {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                        spr.Position = Position + new Point(0, -1 + mod);
                        break;
                    case 2:
                    case 6:
                        spr.Position = Position + new Point(0, -2 + mod);
                        break;
                    default:
                        spr.Position = Position + new Point(0, mod);
                        break;
                }
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

        private List<AnimationData> LoadPlayerAnimations(Dictionary<string, string> data, Point shiftedValue)
        {
            List<AnimationData> rv;
            rv = Util.LoadPlayerAnimations(data);

            Util.AddToAnimationsList(ref rv, data, VerbEnum.UseTool, true, true);
            Util.AddToAnimationsList(ref rv, data, AnimationEnum.Pose);

            if (shiftedValue != Point.Zero)
            {
                foreach (AnimationData d in rv)
                {
                    d.ChangeLocation(shiftedValue);
                }
            }
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

            int xCrawl = 0;
            Point pos = Util.GetPointFromIndex(index, Constants.PLAYER_EXTRAS_COLUMNS, 4);
            HairSprite = new AnimatedSprite(string.Format(@"{0}Hair", DataManager.FOLDER_PLAYER));
            foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
            {
                if (e == DirectionEnum.None) { continue; }

                HairSprite.AddAnimation(e, (pos.X * Constants.TILE_SIZE) + xCrawl, (pos.Y * Constants.TILE_SIZE), size);
                xCrawl += Constants.TILE_SIZE;
            }

            HairSprite.SetColor(HairColor);
            HairSprite.SetLinkedSprite(BodySprite, false);
        }

        public override void PlayAnimation<TEnum>(TEnum anim)
        {
            BodySprite.PlayAnimation(anim);
            ArmSprite.PlayAnimation(anim);
            PantsSprite?.PlayAnimation(anim);
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            if (verb == VerbEnum.Walk && ActiveMount != null) { verb = VerbEnum.Idle; }

            BodySprite.PlayAnimation(verb, dir);
            ArmSprite.PlayAnimation(verb, dir);
            PantsSprite?.PlayAnimation(verb, dir);
        }

        public override void SetFacing(DirectionEnum dir)
        {
            base.SetFacing(dir);
            HairSprite?.PlayAnimation(dir);
            EyeSprite?.PlayAnimation(dir);
            HatSprite?.PlayAnimation(dir);
            ShirtSprite?.PlayAnimation(dir);
            PantsSprite?.PlayAnimation(dir);
        }

        public void SetScale(int scale = 1)
        {
            GetSprites().ForEach(spr => spr.SetScale(scale));
        }

        public void SetClothing(Clothing c)
        {
            if (c != null)
            {
                AnimatedSprite newSprite = c.GetSprite();
                newSprite.Position = Position;
                newSprite.PlayAnimation(BodySprite.CurrentAnimation);
                newSprite.SetLinkedSprite(HairSprite, false);

                //ToDo: Fix the clothing layer syncing.
                switch (c.ClothingType)
                {
                    case EquipmentEnum.Hat:
                        newSprite.Position += new Point(0, Constants.PLAYER_HAT_OFFSET);
                        HatSprite = newSprite;
                        break;
                    case EquipmentEnum.Shirt:
                        newSprite.Position += new Point(0, Constants.PLAYER_SHIRT_OFFSET);
                        ShirtSprite = newSprite;
                        ArmSprite.SetLinkedSprite(ShirtSprite, false);
                        break;
                    case EquipmentEnum.Pants:
                        PantsSprite = newSprite;
                        ArmSprite.SetLinkedSprite(PantsSprite, false);
                        break;
                }

                SetLinkedSprites();
            }
        }

        public void RemoveClothing(Clothing c)
        {
            switch (c.ClothingType)
            {
                case EquipmentEnum.Hat:
                    HatSprite = null;
                    break;
                case EquipmentEnum.Shirt:
                    ShirtSprite = null;
                    break;
                case EquipmentEnum.Pants:
                    PantsSprite = null;
                    break;
            }

            SetLinkedSprites();
        }

        public void SetBodyType(int val)
        {
            BodyType = val;
            SetClothing(Hat);
            SetClothing(Shirt);
            SetClothing(Pants);
        }

        private void SetLinkedSprites()
        {
            AnimatedSprite temp = BodySprite;
            LinkedSpriteHelper(PantsSprite, ref temp);
            LinkedSpriteHelper(ShirtSprite, ref temp);

            ArmSprite.SetLinkedSprite(temp, false);
            EyeSprite.SetLinkedSprite(temp, false);
            HairSprite.SetLinkedSprite(EyeSprite, false);
            temp = HairSprite;

            LinkedSpriteHelper(HatSprite, ref temp);
        }

        private void LinkedSpriteHelper(AnimatedSprite testSprite, ref AnimatedSprite temp)
        {
            if (testSprite != null)
            {
                testSprite.SetLinkedSprite(temp, false);
                temp = testSprite;
            }
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
