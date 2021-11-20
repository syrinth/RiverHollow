﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class ShippingGremlin : Villager
    {
        private int _iRows = 4;
        private int _iCols = 10;
        private Item[,] _arrInventory;

        public override Vector2 Position
        {
            get
            {
                return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - TILE_SIZE);
            }
            set
            {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + TILE_SIZE);
            }
        }

        public ShippingGremlin(int index, Dictionary<string, string> stringData)
        {
            _bLivesInTown = true;
            _liRequiredBuildingIDs = new List<int>();
            _diRequiredObjectIDs = new Dictionary<int, int>();
            _arrInventory = new Item[_iRows, _iCols];
            _eActorType = ActorEnum.ShippingGremlin;
            _iIndex = index;
            _iBodyWidth = 32;
            _iBodyHeight = 32;

            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Gremlin", _iIndex.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";
            DataManager.GetTextData("Character", _iIndex, ref _sName, "Name");

            Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);

            _sprBody = new AnimatedSprite(_sNPCFolder + "NPC_" + _iIndex.ToString("00"));
            _sprBody.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.Action_One, 32, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            _sprBody.AddAnimation(AnimationEnum.Action_Finished, 128, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.Action_Two, 160, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            PlayAnimation(AnimationEnum.ObjectIdle);

            if (GameManager.ShippingGremlin == null) { GameManager.ShippingGremlin = this; }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (IsCurrentAnimation(AnimationEnum.Action_One) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.Action_Finished);
                PlayerManager.AllowMovement = true;
                base.Talk(false);
            }
            else if (IsCurrentAnimation(AnimationEnum.Action_Two) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.ObjectIdle);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
        }

        /// <summary>
        /// When we talk to the ShippingGremlin, lock player movement and then play the open animation
        /// </summary>
        public override void ProcessRightButtonClick()
        {
            PlayerManager.AllowMovement = false;
            _sprBody.PlayAnimation(AnimationEnum.Action_One);
        }

        /// <summary>
        /// After done talking, play the close animation
        /// </summary>
        public override void StopTalking()
        {
            base.StopTalking();
            _sprBody.PlayAnimation(AnimationEnum.Action_Two);
        }

        public void OpenShipping()
        {
            GUIManager.OpenMainObject(new HUDInventoryDisplay(_arrInventory, GameManager.DisplayTypeEnum.Ship));
        }

        /// <summary>
        /// Iterate through every item in the Shipping Bin and calculate the
        /// sell price of each item. Add the total to the Player's inventory
        /// and then clear out the Shipping Bin.
        /// </summary>
        /// <returns></returns>
        public int SellAll()
        {
            int val = 0;
            foreach (Item i in _arrInventory)
            {
                if (i != null)
                {
                    val += i.SellPrice * i.Number;
                    PlayerManager.AddMoney(val);
                }
            }
            _arrInventory = new Item[_iRows, _iCols];

            return val;
        }
    }
}
