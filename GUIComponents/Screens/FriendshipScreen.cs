using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class FriendshipScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;
        List<FriendshipBox> _villagerList;
        GUIWindow _friendshipWindow;

        public FriendshipScreen()
        {
            _villagerList = new List<FriendshipBox>();
            _friendshipWindow = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedWin, WIDTH, HEIGHT);

            foreach(NPC n in CharacterManager.DiNPC.Values)
            {
                FriendshipBox f = new FriendshipBox(n, _friendshipWindow.MidWidth());

                if (_villagerList.Count == 0) { f.AnchorToInnerSide(_friendshipWindow, GUIObject.SideEnum.TopLeft); }
                else {
                    f.AnchorAndAlignToObject(_villagerList[_villagerList.Count - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);   //-2 because we start at i=1
                }

                _villagerList.Add(f);
            }
          
            Controls.Add(_friendshipWindow);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }

    public class FriendshipBox : GUIObject
    {
        private SpriteFont _font;
        GUIText _gTextName;
        //GUIText _gTextLevel;
        GUIText _gTextPoints;
        GUIWindow _gWin;

        public FriendshipBox(NPC c, int mainWidth)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _gTextName = new GUIText(c.Name + " - ");
            _gTextPoints = new GUIText(c.Friendship);
            _gWin = new GUIWindow(GUIWindow.BrownWin, mainWidth, 16);

            _gTextName.AnchorToInnerSide(_gWin, SideEnum.TopLeft);
            _gTextPoints.AnchorAndAlignToObject(_gTextName, SideEnum.Right, SideEnum.Bottom);

            _gWin.Resize();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gWin.Draw(spriteBatch);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gWin.Position(value);

            Width = _gWin.Width;
            Height = _gWin.Height;
        }
    }
}
