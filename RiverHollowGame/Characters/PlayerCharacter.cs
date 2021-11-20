﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class PlayerCharacter : ClassedCombatant
    {
        AnimatedSprite _sprEyes;
        public AnimatedSprite EyeSprite => _sprEyes;
        AnimatedSprite _sprHair;
        public AnimatedSprite HairSprite => _sprHair;
        public Color HairColor { get; private set; } = Color.White;
        public int HairIndex { get; private set; } = 0;
        public int BodyType { get; private set; } = 1;
        public string BodyTypeStr => BodyType.ToString("00");

        public bool CanBecomePregnant { get; set; }
        public bool Pregnant { get; set; }

        protected override List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { _sprBody, _sprEyes, _sprHair, Body?.Sprite, Hat?.Sprite, Legs?.Sprite };
            liRv.RemoveAll(x => x == null);
            return liRv;
        }

        public Vector2 BodyPosition => _sprBody.Position;
        public override Vector2 Position
        {
            get { return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - TILE_SIZE); }
            set
            {
                Vector2 vPos = new Vector2(value.X, value.Y - _sprBody.Height + TILE_SIZE);
                foreach (AnimatedSprite spr in GetSprites()) { spr.Position = vPos; }
            }
        }
        public override Rectangle CollisionBox => ActiveMount != null ? ActiveMount.CollisionBox : new Rectangle((int)Position.X + 2, (int)Position.Y + 2, Width - 4, TILE_SIZE - 4);

        #region Clothing
        public Clothes Hat { get; private set; }
        public Clothes Body { get; private set; }
        Clothes Back;
        Clothes Hands;
        public Clothes Legs { get; private set; }
        Clothes Feet;
        #endregion

        public Pet ActivePet { get; private set; }
        public Mount ActiveMount { get; private set; }
        public bool Mounted => ActiveMount != null;

        public PlayerCharacter() : base()
        {
            _sName = PlayerManager.Name;
            _iBodyWidth = TILE_SIZE;
            _iBodyHeight = HUMAN_HEIGHT;

            HairColor = Color.Red;

            _liTilePath = new List<RHTile>();

            //Sets a default class so we can load and display the character to start
            SetClass(DataManager.GetClassByIndex(1));
            // SetClothes((Clothes)DataManager.GetItem(int.Parse(DataManager.Config[6]["ItemID"])));

            _sprBody.SetColor(Color.White);
            _sprHair.SetColor(HairColor);

            SpdMult = NORMAL_SPEED;
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);

            _sprEyes.Draw(spriteBatch, useLayerDepth);
            //_sprHair.Draw(spriteBatch, useLayerDepth);

            Body?.Sprite.Draw(spriteBatch, useLayerDepth);
            Hat?.Sprite.Draw(spriteBatch, useLayerDepth);
            Legs?.Sprite.Draw(spriteBatch, useLayerDepth);
        }

        private List<AnimationData> LoadPlayerAnimations(Dictionary<string, string> data)
        {
            List<AnimationData> rv;
            rv = LoadWorldAndCombatAnimations(data);

            AddToAnimationsList(ref rv, data, VerbEnum.UseTool, true, true);
            return rv;
        }

        /// <summary>
        /// Override for the ClassedCombatant SetClass methog. Calls the super method and then
        /// loads the appropriate sprites.
        /// </summary>
        /// <param name="x">The class to set</param>
        /// <param name="assignGear">Whether or not to assign starting gear</param>
        public override void SetClass(CharacterClass x)
        {
            base.SetClass(x);

            //Loads the Sprites for the players body for the appropriate class
            LoadSpriteAnimations(ref _sprBody, LoadPlayerAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, BodyTypeStr));

            //Hair type has already been set either by default or by being allocated.
            SetHairType(HairIndex);

            //Loads the Sprites for the players body for the appropriate class
            LoadSpriteAnimations(ref _sprEyes, LoadPlayerAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Eyes", DataManager.FOLDER_PLAYER));
            //_sprEyes.SetDepthMod(EYE_DEPTH);
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
        public void SetHairType(int index)
        {
            HairIndex = index;
            //Loads the Sprites for the players hair animations for the class based off of the hair ID
            LoadSpriteAnimations(ref _sprHair, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[CharacterClass.ID]), string.Format(@"{0}Hairstyles\Hair_{1}", DataManager.FOLDER_PLAYER, HairIndex));
            _sprHair.SetLayerDepthMod(HAIR_DEPTH);
        }

        public void MoveBy(int x, int y)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.MoveBy(x, y); }
            ActiveMount?.BodySprite.MoveBy(x, y);
        }

        public override void PlayAnimation<TEnum>(TEnum anim)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.PlayAnimation(anim); }
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            if (verb == VerbEnum.Walk && ActiveMount != null) { verb = VerbEnum.Idle; }

            foreach (AnimatedSprite spr in GetSprites()) { spr.PlayAnimation(verb, dir); }
        }

        public void SetScale(int scale = 1)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.SetScale(scale); }
        }

        public void SetClothes(Clothes c)
        {
            if (c != null)
            {
                string clothingTexture = string.Format(@"Textures\Items\Gear\{0}\{1}", c.ClothesType.ToString(), c.TextureAnimationName);
                if (!c.GenderNeutral) { clothingTexture += ("_" + BodyTypeStr); }

                LoadSpriteAnimations(ref c.Sprite, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[CharacterClass.ID]), clothingTexture);

                if (c.SlotMatch(ClothesEnum.Body)) { Body = c; }
                else if (c.SlotMatch(ClothesEnum.Hat))
                {
                    _sprHair.FrameCutoff = 9;
                    Hat = c;
                }
                else if (c.SlotMatch(ClothesEnum.Legs)) { Legs = c; }

                //MAR AWKWARD
                c.Sprite.Position = _sprBody.Position;
                c.Sprite.PlayAnimation(_sprBody.CurrentAnimation);
                c.Sprite.SetLayerDepthMod(0.004f);
            }
        }

        public void RemoveClothes(ClothesEnum c)
        {
            if (c.Equals(ClothesEnum.Body)) { Body = null; }
            else if (c.Equals(ClothesEnum.Hat))
            {
                _sprHair.FrameCutoff = 0;
                Hat = null;
            }
        }

        public void SetBodyType(int val)
        {
            BodyType = val;
            SetClass(_class);
            SetClothes(Hat);
            SetClothes(Body);
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

            Position = ActiveMount.BodySprite.Position + new Vector2((ActiveMount.BodySprite.Width - BodySprite.Width) / 2, 8);

            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(ActiveMount.BodySprite);
            }
        }
        public void Dismount()
        {
            Position = ActiveMount.BodySprite.Position + new Vector2(TILE_SIZE, 0);
            ActiveMount = null;
            SpdMult = NORMAL_SPEED;

            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(null);
            }
        }
    }
}
