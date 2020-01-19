using Microsoft.Xna.Framework;
using RiverHollow.Actors;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    class GUICharacterDisplayBox
    {
    }

    public class NPCDisplayBox : GUIWindow
    {
        public delegate void ClickDelegate(ClassedCombatant selectedCharacter);
        private ClickDelegate _delAction;

        ClassedCombatant _actor;
        public ClassedCombatant Actor => _actor;

        public NPCDisplayBox(ClickDelegate action = null)
        {
            _winData = GUIWindow.GreyWin;
            _delAction = action;
        }

        public virtual void PlayAnimation(AnimationEnum animation) { }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum dir) { }

        public class CharacterDisplayBox : NPCDisplayBox
        {
            public Adventurer WorldAdv;
            GUISprite _sprite;
            public GUISprite Sprite => _sprite;

            public CharacterDisplayBox(ClassedCombatant w, ClickDelegate del) : base(del)
            {
                if (w != null)
                {
                    _actor = w;
                    _sprite = new GUISprite(w.BodySprite, true);
                }
                Setup();
            }

            public void AssignToBox(Adventurer adv)
            {
                if (adv != null)
                {
                    WorldAdv = adv;
                    _actor = adv;
                    _sprite = new GUISprite(adv.BodySprite, true);

                    _sprite.SetScale((int)GameManager.Scale);
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                }
                else
                {
                    RemoveControl(_sprite);
                    WorldAdv = null;
                    _actor = null;
                    _sprite = null;
                }
            }

            public void Setup()
            {
                Width = ((int)Scale * TileSize) + ((int)Scale * TileSize) / 4;
                Height = (int)Scale * ((TileSize * 2) + 2) + (_winData.Edge * 2);

                if (_actor != null)
                {
                    _sprite.SetScale((int)GameManager.Scale);
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                }
            }

            public override void Update(GameTime gTime)
            {
                if (_sprite != null)
                {
                    _sprite.Update(gTime);
                }
            }

            public override void PlayAnimation(AnimationEnum animation) { _sprite.PlayAnimation(animation); }
            public override void PlayAnimation(VerbEnum verb, DirectionEnum dir) { _sprite.PlayAnimation(verb, dir); }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse) && _delAction != null)
                {
                    _delAction(_actor);
                    rv = true;
                }
                return rv;
            }

            public class ClassSelectionBox : CharacterDisplayBox
            {
                private ClickDelegate _delAction;
                public new delegate void ClickDelegate(ClassSelectionBox o);

                private int _iClassID;
                public int ClassID => _iClassID;

                public ClassSelectionBox(Adventurer w, ClickDelegate del) : base(w, null)
                {
                    _iClassID = w.CharacterClass.ID;
                    _delAction = del;
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (Contains(mouse) && _delAction != null)
                    {
                        _delAction(this);
                        rv = true;
                    }
                    return rv;
                }
            }
        }

        public class PlayerDisplayBox : NPCDisplayBox
        {
            GUICharacterSprite _playerSprite;
            public GUICharacterSprite PlayerSprite => _playerSprite;

            bool _bOverwrite = false;

            public PlayerDisplayBox(bool overwrite = false, ClickDelegate action = null) : base(action)
            {
                _bOverwrite = overwrite;
                _actor = PlayerManager.World;
                Configure();
            }

            public override void Update(GameTime gTime)
            {
                _playerSprite.Update(gTime);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (Contains(mouse) && _delAction != null)
                {
                    _delAction(PlayerManager.World);
                    rv = true;
                }

                return rv;
            }

            public void Configure()
            {
                Controls.Clear();
                _playerSprite = new GUICharacterSprite(_bOverwrite);
                _playerSprite.SetScale((int)GameManager.Scale);
                _playerSprite.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);

                Width = _playerSprite.Width + _playerSprite.Width / 3;
                Height = _playerSprite.Height + (_winData.Edge * 2);

                _playerSprite.CenterOnWindow(this);
                _playerSprite.AnchorToInnerSide(this, SideEnum.Bottom);
            }
        }
    }
}
