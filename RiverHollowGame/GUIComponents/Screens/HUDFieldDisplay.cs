using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    internal class HUDFieldDisplay : GUIMainObject
    {
        const int MAX_DISPLAY = 4;

        Field _objField;
        string[] _arrCropList;

        GUIImage _gSelection;
        GUIText _gName;
        GUIText _gTime;
        GUIImage _gScroll;

        readonly List<GUIItemBoxHover> _liItemDisplay;

        public HUDFieldDisplay(Field targetField)
        {
            _liItemDisplay = new List<GUIItemBoxHover>();

            _objField = targetField;

            _arrCropList = _objField.GetStringParamsByIDKey("ItemID");

            _winMain = SetMainWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(114), GameManager.ScaleIt(78));
            _gSelection = new GUIImage(GUIUtils.SELECT_CORNER);
            _gSelection.Show(false);

            for (int i = 0; i < MAX_DISPLAY; i++)
            {
                Item newItem = null;
                if (_arrCropList.Length > i)
                {
                    var cropData = Util.FindIntArguments(_arrCropList[i]);
                    newItem = DataManager.GetItem(cropData[0]);
                }

                var displayWindow = new GUIItemBoxHover(newItem, ItemBoxDraw.Never, UpdateInfo);
                if (newItem == null)
                {
                    displayWindow.Enable(false);
                }

                _liItemDisplay.Add(displayWindow);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), _winMain, new Point(14, 14), MAX_DISPLAY, 2, 2);

            _gScroll = new GUIImage(GUIUtils.HUD_SCROLL_S);
            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_liItemDisplay[0], SideEnum.Bottom, 6);

            _gName = new GUIText("");
            _gTime = new GUIText("");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gSelection?.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            for (int i = 0; i < _liItemDisplay.Count; i++)
            {
                if (_liItemDisplay[i].Contains(mouse))
                {
                    for (int j = 0; j < _arrCropList.Length; j++)
                    {
                        var cropData = Util.FindIntArguments(_arrCropList[j]);

                        if (cropData[0] == _liItemDisplay[i].BoxItem.ID)
                        {
                            rv = true;
                            _objField.SetSeedID(cropData[1]);
                            GUIManager.CloseMainObject();
                            break;
                        }
                    }
                }
            }

            return rv;
        }

        private void UpdateInfo(GUIItemBoxHover obj)
        {
            if (obj.BoxItem != null)
            {
                _gSelection.CenterOnObject(obj);
                _gSelection.Show(true);

                for (int i = 0; i < _arrCropList.Length; i++)
                {
                    var cropData = Util.FindIntArguments(_arrCropList[i]);

                    if (cropData[0] == obj.BoxItem.ID)
                    {
                        var plantID = cropData[1];
                        string strTime = DataManager.GetStringByIDKey(plantID, "Time", DataType.WorldObject);
                        var intTimeSegments = Util.FindIntParams(strTime);

                        int totalTime = 0;
                        intTimeSegments.ForEach(x => totalTime += x);

                        int objID = obj.BoxItem.ID;
                        Item crop = DataManager.GetItem(cropData[0]);

                        _gName.SetText(crop.Name());
                        _gName.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 4);

                        _gTime.SetText(string.Format("{0} days", totalTime));
                        _gTime.AnchorAndAlignWithSpacing(_gName, SideEnum.Bottom, SideEnum.CenterX, 4);

                        break;
                    }
                }
            }
        }
    }
}
