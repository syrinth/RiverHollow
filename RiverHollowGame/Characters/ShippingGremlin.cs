using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Windows.Input;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class ShippingGremlin : TalkingActor
    {
        private int _iRows = 4;
        private int _iCols = 10;
        private Item[,] _arrInventory;

        public override Point Position
        {
            get
            {
                return new Point(BodySprite.Position.X, BodySprite.Position.Y + BodySprite.Height - Constants.TILE_SIZE);
            }
            set
            {
                BodySprite.Position = new Point(value.X, value.Y - BodySprite.Height + Constants.TILE_SIZE);
            }
        }

        public ShippingGremlin(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            //_bLivesInTown = true;
            //_diRequiredObjectIDs = new Dictionary<int, int>();
            _arrInventory = new Item[_iRows, _iCols];

            //_eSpawnStatus = VillagerSpawnStatus.NonTownMap;
            Size = new Point(32, 32);

            _sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Gremlin", stringData["Key"]);
            //_sPortrait = _sPortraitFolder + "WizardPortrait";

           // Util.AssignValue(ref _sStartMap, "StartMap", stringData);
           // Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);

            BodySprite = new AnimatedSprite(SpriteName());
            
            BodySprite.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, Width, Height);
            BodySprite.AddAnimation(AnimationEnum.Action1, 32, 0, Width, Height, 3, 0.1f);
            BodySprite.AddAnimation(AnimationEnum.Action_Finished, 128, 0, Width, Height);
            BodySprite.AddAnimation(AnimationEnum.Action2, 160, 0, Width, Height, 3, 0.1f);
            PlayAnimation(AnimationEnum.ObjectIdle);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (IsCurrentAnimation(AnimationEnum.Action1) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.Action_Finished);
                PlayerManager.AllowMovement = true;
                base.StartConversation(false);
            }
            else if (IsCurrentAnimation(AnimationEnum.Action2) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
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
            BodySprite.PlayAnimation(AnimationEnum.Action1);
        }

        /// <summary>
        /// After done talking, play the close animation
        /// </summary>
        public override void StopTalking()
        {
            base.StopTalking();
            BodySprite.PlayAnimation(AnimationEnum.Action2);
        }

        public void OpenShipping()
        {
            GUIManager.OpenMainObject(new HUDInventoryDisplay(_arrInventory, DisplayTypeEnum.Ship));
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
                    //val += i.SellPrice * i.Number;
                    PlayerManager.AddMoney(val);
                }
            }
            _arrInventory = new Item[_iRows, _iCols];

            return val;
        }
    }
}
