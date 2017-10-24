using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow
{
    public class Monster : CombatCharacter
    {
        #region Properties
        #region CombatProperties
        protected double _actionDuration = 0;
        protected double _globalCooldown = 0;
        protected double CoolDown = 1;
        protected double _attackSpeed = 0.1;    //total time for one attack.

        protected bool _canCharge;
        protected Vector2 _chargeTarget;
        protected double _chargeCooldown = 3;
        protected int _chargeSpeed = 2;
        protected int _minCharge;
        protected int _maxCharge;
        protected bool _canJump;
        protected bool _canSmash;
        #endregion

        public enum ActionType { Idle, Attack, Charge, Jump, Smash}
        public ActionType _currentAction = ActionType.Idle;
        protected string _name;
        protected int _minDmg;
        protected int _maxDmg;
        protected int _idleFor;
        protected int _leash = 400;
        
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;

        #endregion

        public Monster(int id, string[] monsterData)
        {
            ImportBasics(monsterData, id);
            LoadContent(_textureName, 100, 100, 2, 0.7f);
        }

        protected int ImportBasics(string[] monsterData, int id)
        {
            int i = 0;
            _name = monsterData[i++];
            _textureName = @"Textures\" + monsterData[i++];
            _hp = int.Parse(monsterData[i++]);
            _currentHP = _hp;
            string[] dmg = monsterData[i++].Split(' ');
            _minDmg = int.Parse(dmg[0]);
            _minDmg = int.Parse(dmg[1]);
            string[] actions = monsterData[i++].Split('|');
            foreach(string s in actions)
            {
                string[] data = s.Split(' ');
                switch (data[0])
                {
                    case "Charge":
                        _canCharge = true;
                        _minCharge = int.Parse(data[1]);
                        _maxCharge = int.Parse(data[2]);
                        _chargeCooldown = int.Parse(data[3]);
                        break;
                    default:
                        break;
                }

            }

            return i;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }
    }
}
