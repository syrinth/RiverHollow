using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    internal class HUDCraftStructures : GUIMainObject
    {
        int _iIndex = 0;

        
        readonly GUIImage _gBackgroundBox;
        readonly List<int> _liCanBuild;
        readonly GUIImage _gScroll;

        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly GUIButton _btnBuild;
        List<GUIItemBox> _liRequiredItems;

        GUIText _gName;
        GUISprite _gStructure;

        private CloseMenuDelegate _closeMenu;
        public HUDCraftStructures(CloseMenuDelegate closeMenu)
        {
            _liCanBuild = new List<int>();
            _liRequiredItems= new List<GUIItemBox>();

            _closeMenu = closeMenu;

            var idList = PlayerManager.GetCraftingList().FindAll(x => ((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);
            foreach (int i in idList)
            {
                Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(i);
                if (obj.RequiredToMake.Count > 0 && obj.CanBuild())
                {
                    _liCanBuild.Add(obj.ID);
                }
            }
            
            _winMain = SetMainWindow(GUIWindow.DarkBlue_Window, GameManager.ScaleIt(186), GameManager.ScaleIt(152));

            _gBackgroundBox = new GUIImage(new Rectangle(0, 208, 68, 68), GameManager.ScaleIt(68), GameManager.ScaleIt(68), DataManager.DIALOGUE_TEXTURE);
            _gBackgroundBox.Position(_winMain);
            _gBackgroundBox.ScaledMoveBy(59, 12);
            _winMain.AddControl(_gBackgroundBox);

            _btnLeft = new GUIButton(new Rectangle(102, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnLeft);
            _btnLeft.AnchorAndAlignToObject(_gBackgroundBox, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(4));
            _btnLeft.Enable(false);
            _winMain.AddControl(_btnLeft);

            _btnRight = new GUIButton(new Rectangle(112, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnRight);
            _btnRight.AnchorAndAlignToObject(_gBackgroundBox, SideEnum.Right, SideEnum.CenterY, GameManager.ScaleIt(4));
            _btnRight.Enable(_liCanBuild.Count > 1);
            _winMain.AddControl(_btnRight);

            _gScroll = new GUIImage(new Rectangle(209, 96, 142, 3), DataManager.HUD_COMPONENTS);
            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_gBackgroundBox, SideEnum.Bottom, GameManager.ScaleIt(4));
            _winMain.AddControl(_gScroll);

            _btnBuild = new GUIButton("Build", BtnBuild);
            _btnBuild.Position(_winMain);
            _btnBuild.AnchorAndAlignToObject(_gScroll, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(43)); 

            _gName = new GUIText("");

            DisplayStructureInfo();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            return GUIUtils.ProcessLeftMouseButton(mouse, _btnLeft, _btnRight, _btnBuild);
        }

        private void DisplayStructureInfo()
        {
            if (_liCanBuild.Count == 0)
            {
                return;
            }

            _btnRight.Enable(_iIndex < _liCanBuild.Count - 1);
            _btnLeft.Enable(_iIndex > 0);

            _winMain.RemoveControl(_gStructure);

            Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(_liCanBuild[_iIndex]);
            _gStructure = new GUISprite(obj.Sprite, true);
            _gStructure.CenterOnObject(_gBackgroundBox);
            _winMain.AddControl(_gStructure);

            GUIUtils.SetObjectScale(_gStructure, obj.Width, obj.Height, 4);
            _gStructure.CenterOnObject(_gBackgroundBox);

            bool sufficientItems = GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, obj.RequiredToMake);

            _btnBuild.Enable(sufficientItems);
            _gStructure.Alpha(sufficientItems ? 1 : 0.3f);

            _gName.SetText(obj.Name());
            _gName.SetColor(sufficientItems ? Color.Black : Color.Red);
            _gName.AnchorAndAlignToObject(_gScroll, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(4));

            GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), _winMain, _gScroll, 2, 22);
        }

        public void BtnLeft()
        {
            if (_iIndex > 0)
            {
                _iIndex--;
                DisplayStructureInfo();
            }
        }
        public void BtnRight()
        {
            if (_iIndex < _liCanBuild.Count)
            {
                _iIndex ++;
                DisplayStructureInfo();
            }
        }

        public void BtnBuild()
        {
            Buildable obj;

            obj = (Buildable)DataManager.CreateWorldObjectByID(_liCanBuild[_iIndex]);
            var requiredToMake = obj.RequiredToMake;

            if (InventoryManager.HasSufficientItems(requiredToMake))
            {
                GameManager.EnterTownModeBuild(false);
                GameManager.PickUpWorldObject(obj);

                GUIManager.CloseMainObject();
                _closeMenu();
            }
        }
    }
}

