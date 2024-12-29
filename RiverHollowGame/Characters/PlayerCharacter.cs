﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using RiverHollow.WorldObjects;
using RiverHollow.GUIComponents;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Utilities.Constants;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Characters
{
    public class PlayerCharacter : CombatActor
    {
        readonly Dictionary<CosmeticSlotEnum, AppliedCosmetic> AppliedCosmetics;

        public AnimatedSprite ArmSprite { get; private set; }
        public int BodyType { get; private set; } = 1;
        public string BodyTypeStr => BodyType.ToString("00");

        public bool CanBecomePregnant { get; set; }
        public bool Pregnant { get; set; }

        public override List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { BodySprite, ArmSprite};
            liRv.AddRange(CosmeticSprites());
            return liRv;
        }

        public override Rectangle CollisionBox => ActiveMount != null ? ActiveMount.CollisionBox : base.CollisionBox;
        public override Rectangle HitBox => new Rectangle(Position.X + 2, Position.Y + 22, 12, 10);

        Light _lightSource;

        public Pet ActivePet { get; private set; }
        public Mount ActiveMount { get; private set; }
        public bool Mounted => ActiveMount != null;

        public override float MaxHP => PlayerManager.MaxPlayerHP(); 

        public PlayerCharacter() : base()
        {
            AppliedCosmetics = new Dictionary<CosmeticSlotEnum, AppliedCosmetic>();
            AddDefaultCosmetics();

            CurrentHP = MaxHP;

            _liTilePath = new List<RHTile>();

#if DEBUG
            BodySprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17], Point.Zero), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, "03"));// BodyTypeStr));
            ArmSprite = LoadSpriteAnimations(LoadPlayerAnimations(DataManager.Config[17], new Point(0, 35)), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, "03"));// BodyTypeStr));
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

            SyncSprite(CosmeticSprite(CosmeticSlotEnum.Hair), 0);
            SyncSprite(CosmeticSprite(CosmeticSlotEnum.Eyes), 0);
            SyncSprite(CosmeticSprite(CosmeticSlotEnum.Head), Constants.PLAYER_HAT_OFFSET);
            SyncSprite(CosmeticSprite(CosmeticSlotEnum.Body), Constants.PLAYER_SHIRT_OFFSET);

            var pantsSprite = CosmeticSprite(CosmeticSlotEnum.Legs);
            if (pantsSprite != null)
            {
                pantsSprite.Position = BodySprite.Position + new Point(0, Constants.PLAYER_PANTS_OFFSET);
            }

            if (HasKnockbackVelocity())
            {
                ApplyKnockbackVelocity();
            }

            CheckDamageTimers(gTime);
        }

        #region Cosmetics
        private void AddDefaultCosmetics()
        {
            foreach (CosmeticSlotEnum e in Enum.GetValues(typeof(CosmeticSlotEnum)))
            {
                AppliedCosmetics[e] = new AppliedCosmetic();
            }

        }
        public List<Cosmetic> GetCosmetics()
        {
            return AppliedCosmetics.Values.Select(x => x.MyCosmetic).ToList();
        }
        public Cosmetic GetCosmetic(CosmeticSlotEnum e)
        {
            return AppliedCosmetics[e].MyCosmetic;
        }
        public Color GetCosmeticColor(CosmeticSlotEnum e)
        {
            return AppliedCosmetics[e].CosmeticColor;
        }
        public void SetCosmeticColor(CosmeticSlotEnum e, Color c)
        {
            AppliedCosmetics[e].SetColor(c);
        }

        public void SetCosmetic(CosmeticSlotEnum e, int id)
        {
            var newCosmetic = DataManager.GetCosmetic(id);
            if (e == newCosmetic.CosmeticSlot)
            {
                AppliedCosmetics[e].SetCosmetic(newCosmetic);

                LinkSprites();
            }
        }

        private AnimatedSprite CosmeticSprite(CosmeticSlotEnum e)
        {
            return AppliedCosmetics[e].MySprite;
        }
        private List<AnimatedSprite> CosmeticSprites()
        {
            List<AnimatedSprite> rv = new List<AnimatedSprite>();
            foreach (var kvp in AppliedCosmetics)
            {
                if (kvp.Value.MySprite != null)
                {
                    rv.Add(kvp.Value.MySprite);
                }
            }
            return rv;
        }

        public CosmeticData SaveCosmeticData(CosmeticSlotEnum e)
        {
            var cosmetic = AppliedCosmetics[e];

            CosmeticData data = new CosmeticData()
            {
                id = cosmetic.MyCosmetic.ID,
                cosmeticColor = cosmetic.CosmeticColor
            };

            return data;
        }

        public void LoadCosmeticData(CosmeticData data)
        {
            var cosmetic = DataManager.GetCosmetic(data.id);
            AppliedCosmetics[cosmetic.CosmeticSlot].SetColor(data.cosmeticColor);
            AppliedCosmetics[cosmetic.CosmeticSlot].SetCosmetic(cosmetic);
        }
        #endregion

        private void LinkSprites()
        {
            var head = CosmeticSprite(CosmeticSlotEnum.Head);
            var hair = CosmeticSprite(CosmeticSlotEnum.Hair);
            var eyes = CosmeticSprite(CosmeticSlotEnum.Eyes);
            var top = CosmeticSprite(CosmeticSlotEnum.Body);
            var legs = CosmeticSprite(CosmeticSlotEnum.Legs);
            var feet = CosmeticSprite(CosmeticSlotEnum.Feet);

            head.SetLayerDepthMod(1);
            hair.SetLayerDepthMod(0.9f);
            eyes.SetLayerDepthMod(0.8f);
            top.SetLayerDepthMod(0.7f);
            ArmSprite.SetLayerDepthMod(0.6f);
            legs.SetLayerDepthMod(0.5f);
            feet.SetLayerDepthMod(0.4f);
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

            if (!HasKnockbackVelocity() && PlayerManager.GrabbedObject != null && PlayerManager.MoveObjectToPosition != Point.Zero && PlayerManager.GrabbedObject.BasePosition != PlayerManager.MoveObjectToPosition)
            {
                PlayerManager.MoveGrabbedObject(Util.GetMoveSpeed(PlayerManager.GrabbedObject.BasePosition, PlayerManager.MoveObjectToPosition, BuffedSpeed));
            }
        }

        public void SetLightSource()
        {
            SetLightSource(7 + PlayerManager.LanternLevel);
        }
        public void SetLightSource(int lightID)
        {
            _lightSource = DataManager.GetLight(lightID);
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

        

        public override void PlayAnimation<TEnum>(TEnum anim)
        {
            BodySprite.PlayAnimation(anim);
            ArmSprite.PlayAnimation(anim);
            foreach(var spr in CosmeticSprites())
            {
                spr.PlayAnimation(anim);
            }
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            if (verb == VerbEnum.Walk && ActiveMount != null) { verb = VerbEnum.Idle; }

            BodySprite.PlayAnimation(verb, dir);
            ArmSprite.PlayAnimation(verb, dir);

            foreach (var spr in CosmeticSprites())
            {
                spr.PlayAnimation(verb, dir);
            }
        }

        public override void SetFacing(DirectionEnum dir)
        {
            base.SetFacing(dir);
            foreach (var spr in CosmeticSprites())
            {
                spr.PlayAnimation(dir);
            }
        }

        public void SetScale(int scale = 1)
        {
            GetSprites().ForEach(spr => spr.SetScale(scale));
        }

        public void SetBodyType(int val)
        {
            BodyType = val;
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
        }

        public override void SetMoveTo(Point v, bool update = true)
        {
            base.SetMoveTo(v, State != ActorStateEnum.Grab);
        }
    }
}
