using Microsoft.Xna.Framework;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    class GUICharacterDisplayBox
    {
    }

    public class NPCDisplayBox : GUIWindow
    {
        public delegate void ClickDelegate(ClassedCombatant selectedCharacter);
        private ClickDelegate _delAction;
        public ClassedCombatant Actor { get; private set; }

        public NPCDisplayBox(ClickDelegate action = null)
        {
            _winData = GUIWindow.GreyWin;
            _delAction = action;
        }

        public virtual void PlayAnimation(AnimationEnum animation) { }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum dir) { }

        public class CharacterDisplayBox : NPCDisplayBox
        {
            public ClassedCombatant WorldAdv;
            GUISprite _sprite;
            public GUISprite Sprite => _sprite;

            public CharacterDisplayBox(ClassedCombatant w, ClickDelegate del = null) : base(del)
            {
                if (w != null)
                {
                    Actor = w;
                    _sprite = new GUISprite(w.BodySprite, true);
                }
                Setup();
            }

            public void AssignToBox(ClassedCombatant adv)
            {
                if (adv != null)
                {
                    WorldAdv = adv;
                    Actor = adv;
                    _sprite = new GUISprite(adv.BodySprite, true);

                    _sprite.SetScale(GameManager.CurrentScale);
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                }
                else
                {
                    RemoveControl(_sprite);
                    WorldAdv = null;
                    Actor = null;
                    _sprite = null;
                }
            }

            public void Setup()
            {
                Width = ScaledTileSize + WidthEdges();
                Height = ScaleIt((TILE_SIZE * 2) + 2) + HeightEdges();

                if (Actor != null)
                {
                    _sprite.SetScale(GameManager.CurrentScale);
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
                    _delAction(Actor);
                    rv = true;
                }
                return rv;
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
                Actor = PlayerManager.PlayerCombatant;
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
                    _delAction(PlayerManager.PlayerCombatant);
                    rv = true;
                }

                return rv;
            }

            public void Configure()
            {
                Controls.Clear();
                _playerSprite = new GUICharacterSprite(_bOverwrite);
                _playerSprite.SetScale(GameManager.CurrentScale);
                _playerSprite.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);

                Width = _playerSprite.Width + WidthEdges();
                Height = _playerSprite.Height + HeightEdges();

                _playerSprite.CenterOnWindow(this);
                _playerSprite.AnchorToInnerSide(this, SideEnum.Bottom);
            }
        }
    }
}
