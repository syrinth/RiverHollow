using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class ActorDisplayBox : GUIObject
    {
        VerbEnum _eLastVerb = VerbEnum.Idle;
        DirectionEnum _eLastDir = DirectionEnum.Down;

        Actor _act;
        List<GUISprite> _liSprites;

        GUIImage _gBackdrop;

        public ActorDisplayBox(PlayerCharacter actor, GUIImage backDrop)
        {
            _act = actor;

            _gBackdrop = backDrop;
            AssignActor(actor);

            Width = backDrop.Width;
            Height = backDrop.Height;
        }

        public void AssignActor(PlayerCharacter actor)
        {
            CleanControls();
            AddControl(_gBackdrop);

            _liSprites = new List<GUISprite>();
            for (int i = 0; i < actor.GetSprites().Count; i++)
            {
                AddSprite(actor.GetSprites()[i]);
            }
            AddSprite(actor.Chest.Sprite);

            PlayAnimation(_eLastVerb, _eLastDir);
        }

        private void AddSprite(AnimatedSprite sprite)
        {
            if (sprite != null)
            {
                GUISprite spr = new GUISprite(sprite);
                spr.SetScale(GameManager.ScaledPixel);
                spr.Position(this.Position());
                spr.ScaledMoveBy(17, 14);
                _liSprites.Add(spr);
                AddControl(spr);
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
