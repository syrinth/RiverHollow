using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;
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
            _friendshipWindow = new GUIWindow(GUIWindow.RedWin, WIDTH, HEIGHT);
            _friendshipWindow.CenterOnScreen();

            foreach (Villager n in ActorManager.DiNPC.Values)
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
        GUIText _gTextPoints;
        GUIWindow _gWin;
        GUIImage _gAdventure;
        GUIImage _gGift;
        List<GUIImage> _liFriendship;
        
        public FriendshipBox(Villager c, int mainWidth)
        {
            _liFriendship = new List<GUIImage>();
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _gTextName = new GUIText("XXXXXXXXXX");
            if(c.GetFriendshipLevel() == 0)
            {
                _liFriendship.Add(new GUIImage(new Rectangle(0, 64, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog"));
            }
            else
            {
                int notches = c.GetFriendshipLevel() - 1;
                int x = 0;
                if(notches <= 3) { x = 16; }
                else if(notches <= 6) { x = 32; }
                else { x = 48; }

                while (notches > 0)
                {
                    _liFriendship.Add(new GUIImage(new Rectangle(x, 64, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog"));
                    notches--;
                }
            }

            _gWin = new GUIWindow(GUIWindow.BrownWin, mainWidth, 16);

            _gTextName.AnchorToInnerSide(_gWin, SideEnum.TopLeft);
            for (int j = 0; j < _liFriendship.Count; j++)
            {
                if (j == 0) { _liFriendship[j].AnchorAndAlignToObject(_gTextName, SideEnum.Right, SideEnum.CenterY); }
                else { _liFriendship[j].AnchorAndAlignToObject(_liFriendship[j - 1], SideEnum.Right, SideEnum.CenterY); }
            }
            _gTextName.SetText(c.Name);

            _gGift = new GUIImage(new Rectangle(16, 48, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog");
            _gGift.AnchorToInnerSide(_gWin, SideEnum.Right);
            _gGift.AlignToObject(_gTextName, SideEnum.CenterY);
            _gGift.Alpha = (c.CanGiveGift) ? 1 : 0.3f;

            if (c.IsEligible()) {
                EligibleNPC e = (EligibleNPC)c;
                _gAdventure = new GUIImage(new Rectangle(0, 48, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog");
                _gAdventure.AnchorAndAlignToObject(_gGift, SideEnum.Left, SideEnum.CenterY);
                if (PlayerManager.GetParty().Contains(e.Combat))
                {
                    _gAdventure.SetColor(Color.Gold);
                }
                else { _gAdventure.Alpha = (e.CanJoinParty) ? 1 : 0.3f; }
            }

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
