﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.WorldObjects
{
    public class Destructible : WorldObject
    {
        protected int _iHP = 1;
        public int HP => _iHP;

        protected ToolEnum _eToolType;
        public ToolEnum NeededTool => _eToolType;

        protected int _iNeededToolLevel;
        public int NeededToolLevel => _iNeededToolLevel;

        public Destructible(int id, Dictionary<string, string> stringData, bool loadSprite = true) : base(id)
        {
            LoadDictionaryData(stringData, loadSprite);

            if (stringData.ContainsKey("ItemID"))
            {
                string[] split = stringData["ItemID"].Split('-');
                int itemID = int.Parse(split[0]);
                int num = 1;

                if (split.Length == 2) { num = int.Parse(split[1]); }
                _kvpDrop = new KeyValuePair<int, int>(itemID, num);
            }

            if (stringData.ContainsKey("Tool")) { _eToolType = Util.ParseEnum<ToolEnum>(stringData["Tool"]); }
            if (stringData.ContainsKey("Hp")) { _iHP = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("ReqLvl")) { _iNeededToolLevel = int.Parse(stringData["ReqLvl"]); }

            if (loadSprite && stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                _sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), Constants.TILE_SIZE, Constants.TILE_SIZE, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (_iHP <= 0)
            {
                if (!_sprite.ContainsAnimation(AnimationEnum.KO) || _sprite.AnimationFinished(AnimationEnum.KO))
                {
                    MapManager.Maps[Tiles[0].MapName].RemoveWorldObject(this);
                }
            }
        }

        public override void ProcessLeftClick()
        {
            if (_iHP > 0)
            {
               // PlayerManager.SetTool(PlayerManager.RetrieveTool(NeededTool));
            }
        }

        public void DealDamage(Tool toolUsed)
        {
            if (NeededTool == toolUsed.ToolType && toolUsed.ToolLevel >= NeededToolLevel)
            {
                SoundManager.PlayEffectAtLoc(toolUsed.SoundEffect, MapName, CollisionCenter.ToVector2(), toolUsed);

                if (_iHP > 0)
                {
                    _iHP -= toolUsed.ToolLevel;

                    if (_iHP <= 0)
                    {
                        _bWalkable = true;
                        _sprite.PlayAnimation(AnimationEnum.KO);

                        MapManager.DropItemsOnMap(GetDroppedItems(), CollisionBox.Location.ToVector2());
                        CurrentMap.AlertSpawnPoint(this);
                    }
                }

                //Nudge the Object in the direction of the 'attack'
                int xMod = 0, yMod = 0;
                if (PlayerManager.PlayerActor.Facing == DirectionEnum.Left) { xMod = -1; }
                else if (PlayerManager.PlayerActor.Facing == DirectionEnum.Right) { xMod = 1; }

                if (PlayerManager.PlayerActor.Facing == DirectionEnum.Up) { yMod = -1; }
                else if (PlayerManager.PlayerActor.Facing == DirectionEnum.Down) { yMod = 1; }

                _sprite.Position = new Vector2(_sprite.Position.X + xMod, _sprite.Position.Y + yMod);
            }
            else if (NeededTool != toolUsed.ToolType)
            {
                GUIManager.OpenTextWindow("Wrong_Tool");
            }
            else if (toolUsed.ToolLevel < NeededToolLevel)
            {
                GUIManager.OpenTextWindow("Weak_Tool");
            }
        }
    }
}
