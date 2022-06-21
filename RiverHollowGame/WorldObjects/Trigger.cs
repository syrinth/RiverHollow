﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.Screens;

namespace RiverHollow.WorldObjects
{
    public class Trigger : TriggerObject
    {
        string _sSoundEffect;
        Item _item;
        public Trigger(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _item = DataManager.GetItem(_iItemKeyID);

            Util.AssignValue(ref _sSoundEffect, "SoundEffect", stringData);

            if (_iItemKeyID == -1)
            {
                _sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Width, _pImagePos.Y, _uSize);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                float visibility = _bHasBeenTriggered ? 1f : 0.25f;
                _item.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X), (int)(_vMapPosition.Y - 6), TILE_SIZE, TILE_SIZE), true, _sprite.LayerDepth + 1, visibility);
            }
        }

        /// <summary>
        /// Called when the player interacts with the object.
        /// 
        /// If it's already triggered, do nothing.
        /// </summary>
        public override void ProcessRightClick()
        {
            GameManager.SetSelectedWorldObject(this);

            if (!_bHasBeenTriggered)
            {
                //If there's an itemKeyID, display appropriate text
                if (_iItemKeyID != -1)
                {
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
                }
                else
                {
                    SoundManager.PlayEffectAtLoc(_sSoundEffect, MapName, MapPosition, this);
                    FireTrigger();
                }
            }
        }

        public override void AttemptToTrigger(string name)
        {
            if (CanTrigger(name))
            {
                FireTrigger();
            }
        }

        public override void FireTrigger()
        {
            if (CanTrigger())
            {
                _bHasBeenTriggered = true;
                _sprite.PlayAnimation(AnimationEnum.Action1);
                GameManager.ActivateTriggers(_sOutTrigger);
            }
        }
    }
}
