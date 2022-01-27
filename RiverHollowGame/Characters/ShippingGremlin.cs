using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
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

        public ShippingGremlin(int index, Dictionary<string, string> stringData) : base(index)
        {
            //_bLivesInTown = true;
            _liRequiredBuildingIDs = new List<int>();
            _diRequiredObjectIDs = new Dictionary<int, int>();
            _arrInventory = new Item[_iRows, _iCols];
            _eActorType = WorldActorTypeEnum.ShippingGremlin;
            _iBodyWidth = 32;
            _iBodyHeight = 32;

            _diDialogue = DataManager.GetNPCDialogue(stringData["Key"]);
            _sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Gremlin", ID.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";
            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);

            _sprBody = new AnimatedSprite(DataManager.NPC_FOLDER + "NPC_" + ID.ToString("00"));
            _sprBody.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.Action1, 32, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            _sprBody.AddAnimation(AnimationEnum.Action_Finished, 128, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.Action2, 160, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            PlayAnimation(AnimationEnum.ObjectIdle);

            if (GameManager.ShippingGremlin == null) { GameManager.ShippingGremlin = this; }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (IsCurrentAnimation(AnimationEnum.Action1) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.Action_Finished);
                PlayerManager.AllowMovement = true;
                base.Talk(false);
            }
            else if (IsCurrentAnimation(AnimationEnum.Action2) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
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
            _sprBody.PlayAnimation(AnimationEnum.Action1);
        }

        /// <summary>
        /// After done talking, play the close animation
        /// </summary>
        public override void StopTalking()
        {
            base.StopTalking();
            _sprBody.PlayAnimation(AnimationEnum.Action2);
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
