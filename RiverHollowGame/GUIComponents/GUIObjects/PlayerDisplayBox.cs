﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class PlayerDisplayBox : GUIImage
    {
        VerbEnum _eLastVerb = VerbEnum.Idle;
        DirectionEnum _eLastDir = DirectionEnum.Down;

        Point _pMoveBy;

        List<GUISprite> _liSprites;

        public PlayerDisplayBox(PlayerCharacter actor, Rectangle pane, Point moveBy) : base (pane)
        {
            _pMoveBy = moveBy;
            AssignActor(actor);
        }

        public void AssignActor(PlayerCharacter actor)
        {
            CleanControls();

            _liSprites = new List<GUISprite>();
            for (int i = 0; i < actor.GetSprites().Count; i++)
            {
                AddSprite(actor.GetSprites()[i]);
            }
            //AddSprite(actor.Chest.Sprite);

            PlayAnimation(_eLastVerb, _eLastDir);
        }

        private void AddSprite(AnimatedSprite sprite)
        {
            if (sprite != null)
            {
                GUISprite spr = new GUISprite(sprite, true);
                spr.PositionAndMove(this, _pMoveBy);
                _liSprites.Add(spr);
            }
        }

        public void PlayAnimation(VerbEnum verb, DirectionEnum dir) {
            _eLastVerb = verb;
            _eLastDir = dir;

            for (int i = 0; i < _liSprites.Count; i++)
            {
                _liSprites[i].PlayAnimation(_eLastVerb, _eLastDir);
            }
        }
    }
}
