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
            _liRequiredItems = new List<GUIItemBox>();

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
            
            _winMain = SetMainWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(186), GameManager.ScaleIt(152));

            _gBackgroundBox = new GUIImage(GUIUtils.STRUCTURE_BOX);
            _gBackgroundBox.PositionAndMove(_winMain, 59, 12);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.AnchorAndAlignWithSpacing(_gBackgroundBox, SideEnum.Left, SideEnum.CenterY, 4);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.AnchorAndAlignWithSpacing(_gBackgroundBox, SideEnum.Right, SideEnum.CenterY, 4);
            _btnRight.Enable(_liCanBuild.Count > 1);

            _gScroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_gBackgroundBox, SideEnum.Bottom, 4);

            _btnBuild = new GUIButton("Build", BtnBuild);
            _btnBuild.Position(_winMain);
            _btnBuild.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 43); 

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
                _btnBuild.Enable(false);
                return;
            }

            _btnRight.Enable(_iIndex < _liCanBuild.Count - 1);
            _btnLeft.Enable(_iIndex > 0);

            _gStructure?.RemoveSelfFromControl();
            _gName?.RemoveSelfFromControl();

            Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(_liCanBuild[_iIndex]);
            _gStructure = new GUISprite(obj.Sprite, true);

            GUIUtils.SetObjectScale(_gStructure, obj.Width, obj.Height, 4);
            _gStructure.CenterOnObject(_gBackgroundBox);

            bool sufficientItems = GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, obj.RequiredToMake);

            _btnBuild.Enable(sufficientItems);
            _gStructure.Alpha(sufficientItems ? 1 : 0.3f);

            _gName = new GUIText(obj.Name());
            _gName.SetColor(sufficientItems ? Color.Black : Color.Red);
            _gName.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 4);

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

