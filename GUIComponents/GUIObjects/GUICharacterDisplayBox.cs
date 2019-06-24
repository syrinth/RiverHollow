using Microsoft.Xna.Framework;
using RiverHollow.Actors;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
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
        public delegate void ClickDelegate(CombatAdventurer selectedCharacter);
        private ClickDelegate _delAction;

        CombatAdventurer _actor;
        public CombatAdventurer Actor => _actor;

        public NPCDisplayBox(ClickDelegate action = null)
        {
            _winData = GUIWindow.GreyWin;
            _delAction = action;
        }

        public virtual void PlayAnimation<TEnum>(TEnum animation)
        {

        }

        public class CharacterDisplayBox : NPCDisplayBox
        {
            public WorldAdventurer WorldAdv;
            GUISprite _sprite;
            public GUISprite Sprite => _sprite;

            public CharacterDisplayBox(WorldCombatant w, ClickDelegate del) : base(del)
            {
                if (w != null)
                {
                    _actor = w.Combat;
                    _sprite = new GUISprite(w.BodySprite, true);
                }
                Setup();
            }

            public CharacterDisplayBox(EligibleNPC n, ClickDelegate del) : base(del)
            {
                if (n != null)
                {
                    _actor = n.Combat;
                    _sprite = new GUISprite(n.BodySprite, true);
                }
                Setup();
            }

            public void AssignToBox(WorldAdventurer adv)
            {
                if (adv != null)
                {
                    WorldAdv = adv;
                    _actor = adv.Combat;
                    _sprite = new GUISprite(adv.BodySprite, true);

                    _sprite.SetScale((int)GameManager.Scale);
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    PlayAnimation(WActorBaseAnim.IdleDown);
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

                    PlayAnimation(WActorBaseAnim.IdleDown);
                }
            }

            public override void Update(GameTime gameTime)
            {
                if (_sprite != null)
                {
                    _sprite.Update(gameTime);
                }
            }

            public override void PlayAnimation<TEnum>(TEnum animation)
            {
                _sprite.PlayAnimation(animation);
            }

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

                public ClassSelectionBox(WorldAdventurer w, ClickDelegate del) : base(w, null)
                {
                    _iClassID = w.Combat.CharacterClass.ID;
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
                _actor = PlayerManager.Combat;
                Configure();
            }

            public override void Update(GameTime gameTime)
            {
                _playerSprite.Update(gameTime);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (Contains(mouse) && _delAction != null)
                {
                    _delAction(PlayerManager.Combat);
                    rv = true;
                }

                return rv;
            }

            public void Configure()
            {
                Controls.Clear();
                _playerSprite = new GUICharacterSprite(_bOverwrite);
                _playerSprite.SetScale((int)GameManager.Scale);
                _playerSprite.PlayAnimation(WActorBaseAnim.IdleDown);

                Width = _playerSprite.Width + _playerSprite.Width / 3;
                Height = _playerSprite.Height + (_winData.Edge * 2);

                _playerSprite.CenterOnWindow(this);
                _playerSprite.AnchorToInnerSide(this, SideEnum.Bottom);
            }
        }
    }
}
