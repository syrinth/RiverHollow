using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    public class HUDFriendship : GUIMainObject
    {
        GUIWindow _gWindow;
        GUIList _villagerList;

        public HUDFriendship()
        {
            _gWindow = SetMainWindow(GUIManager.MAIN_COMPONENT_WIDTH + 100, GUIManager.MAIN_COMPONENT_WIDTH);
            List<GUIObject> vList;
            vList = new List<GUIObject>();

            foreach (Villager n in TownManager.DIVillagers.Values)
            {
                if (n.RelationshipState == Utilities.Enums.RelationShipStatusEnum.None) { continue; }
                FriendshipBox f = new FriendshipBox(n, _gWindow.InnerWidth() - GUIList.BTNSIZE);

                /*if (vList.Count == 0) { f.AnchorToInnerSide(_gWindow, GUIObject.SideEnum.TopLeft); }
                else
                {
                    f.AnchorAndAlignToObject(vList[vList.Count - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);   //-2 because we start at i=1
                }*/

                vList.Add(f);
            }

            _villagerList = new GUIList(vList, 10, 4, _gWindow.InnerHeight());
            _villagerList.CenterOnScreen(); //.AnchorToInnerSide(_gWindow, GUIObject.SideEnum.TopLeft);//
            AddControl(_villagerList);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
        }
        public class FriendshipBox : GUIWindow
        {
            private BitmapFont _font;
            GUIText _gTextName;
            GUIImage _gAdventure;
            GUIImage _gGift;
            List<GUIImage> _liFriendship;

            public FriendshipBox(Villager v, int mainWidth) : base(GUIWindow.Window_2, mainWidth, 16)
            {
                _liFriendship = new List<GUIImage>();
                _font = DataManager.GetBitMapFont(DataManager.FONT_NEW);
                _gTextName = new GUIText("XXXXXXXXXX");
                if (v.GetFriendshipLevel() == 0)
                {
                    _liFriendship.Add(new GUIImage(new Rectangle(51, 68, 10, 9), ScaleIt(10), ScaleIt(9), DataManager.DIALOGUE_TEXTURE));
                }
                else
                {
                    int notches = v.GetFriendshipLevel() - 1;
                    int x = 0;
                    if (notches <= 3) { x = 16; }
                    else if (notches <= 6) { x = 32; }
                    else { x = 51; }


                    while (notches > 0)
                    {
                        _liFriendship.Add(new GUIImage(new Rectangle(x, 68, 10, 9), ScaleIt(10), ScaleIt(9), DataManager.DIALOGUE_TEXTURE));
                        notches--;
                    }
                }

                _liFriendship[0].AnchorToInnerSide(this, SideEnum.TopLeft);
                _gTextName.AlignToObject(_liFriendship[0], SideEnum.CenterY);
                _gTextName.AnchorToInnerSide(this, SideEnum.Left);
                for (int j = 0; j < _liFriendship.Count; j++)
                {
                    if (j == 0) { _liFriendship[j].AnchorAndAlignToObject(_gTextName, SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN); }
                    else { _liFriendship[j].AnchorAndAlignToObject(_liFriendship[j - 1], SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN); }
                }
                _gTextName.SetText(v.Name());

                _gGift = new GUIImage(new Rectangle(19, 52, 10, 8), ScaleIt(10), ScaleIt(8), DataManager.DIALOGUE_TEXTURE);
                _gGift.AnchorToInnerSide(this, SideEnum.Right);
                _gGift.AlignToObject(_gTextName, SideEnum.CenterY);
                _gGift.Alpha((v.CanGiveGift) ? 1 : 0.3f);

                if (v.CanBeMarried)
                {
                    _gAdventure = new GUIImage(new Rectangle(4, 52, 8, 9), ScaleIt(8), ScaleIt(9), DataManager.DIALOGUE_TEXTURE);
                    _gAdventure.AnchorAndAlignToObject(_gGift, SideEnum.Left, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);
                    if (Array.Find(PlayerManager.GetParty(), x => x == v.CombatVersion) != null)
                    {
                        _gAdventure.SetColor(Color.Gold);
                    }
                    else { _gAdventure.Alpha(v.Combatant ? 1 : 0.3f); }
                }

                Resize();
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;
                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = true;
                return rv;
            }
        }
    }
}
