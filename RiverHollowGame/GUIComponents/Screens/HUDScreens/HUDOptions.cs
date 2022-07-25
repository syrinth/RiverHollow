﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    public class HUDOptions : GUIMainObject
    {
        GUICheck _gHideMiniInventory;
        GUIButton _btnSave;

        GUIText _gSoundSettings;
        GUINumberControl _gVolumeControl;
        GUINumberControl _gEffectControl;
        GUICheck _gMute;

        const int SOUND_VOLUME_SCALAR = 100;

        public HUDOptions()
        {
            _winMain = SetMainWindow();

            _gHideMiniInventory = new GUICheck("Hide Mini Inventory", GameManager.HideMiniInventory);
            _gHideMiniInventory.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 8);

            _gSoundSettings = new GUIText("Sound Settings");
            _gSoundSettings.AnchorAndAlignToObject(_gHideMiniInventory, SideEnum.Bottom, SideEnum.Left, 32);

            _gVolumeControl = new GUINumberControl("Music", SoundManager.MusicVolume * SOUND_VOLUME_SCALAR, UpdateMusicVolume);
            _gVolumeControl.AnchorAndAlignToObject(_gSoundSettings, SideEnum.Bottom, SideEnum.Left);
            _gVolumeControl.MoveBy(new Vector2(32, 0));

            _gEffectControl = new GUINumberControl("Effects", SoundManager.EffectVolume * SOUND_VOLUME_SCALAR, UpdateEffectsVolume);
            _gEffectControl.AnchorAndAlignToObject(_gVolumeControl, SideEnum.Bottom, SideEnum.Left);

            _gMute = new GUICheck("Mute All", SoundManager.IsMuted, ProcessMuteAll);
            _gMute.AnchorAndAlignToObject(_gEffectControl, SideEnum.Bottom, SideEnum.Left);

            _btnSave = new GUIButton("Save", BtnSave);
            _btnSave.AnchorToInnerSide(_winMain, SideEnum.BottomRight);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
        }

        public void ProcessMuteAll()
        {
            if (SoundManager.IsMuted)
            {
                SoundManager.UnmuteAllSound();
                _gVolumeControl.RefreshValue(SoundManager.MusicVolume * SOUND_VOLUME_SCALAR);
                _gEffectControl.RefreshValue(SoundManager.EffectVolume * SOUND_VOLUME_SCALAR);
            }
            else
            {
                SoundManager.MuteAllSound();
                _gVolumeControl.RefreshValue(SoundManager.MusicVolume * SOUND_VOLUME_SCALAR);
                _gEffectControl.RefreshValue(SoundManager.EffectVolume * SOUND_VOLUME_SCALAR);
            }
        }
        public void UpdateMusicVolume()
        {
            SoundManager.SetMusicVolume((float)_gVolumeControl.Value / 100.0f);
        }

        public void UpdateEffectsVolume()
        {
            SoundManager.SetEffectVolume((float)_gEffectControl.Value / 100.0f);
        }
        public void BtnSave()
        {
            GameManager.HideMiniInventory = _gHideMiniInventory.Checked();
            GUIManager.CloseMainObject();
        }

        private class GUINumberControl : GUIObject
        {
            float _fValChange = 10;
            float _fMin;
            float _fMax;
            float _fValue;
            public float Value => _fValue;

            GUIText _gText;
            GUIText _gValue;
            GUIButton _btnLeft;
            GUIButton _btnRight;

            public delegate void ActionDelegate();
            ActionDelegate _del;

            public GUINumberControl(string text, float baseValue, ActionDelegate del, int min = 0, int max = 100)
            {
                _del = del;
                _fMin = min;
                _fMax = max;
                _gText = new GUIText("XXXXXXXXXX");

                _btnLeft = new GUIButton(new Rectangle(272, 112, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, BtnLeftClick);
                _btnLeft.AnchorAndAlignToObject(_gText, SideEnum.Right, SideEnum.CenterY, 12);

                _gValue = new GUIText("000");
                _gValue.AnchorAndAlignToObject(_btnLeft, SideEnum.Right, SideEnum.CenterY, 12);

                _btnRight = new GUIButton(new Rectangle(256, 112, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, BtnRightClick);
                _btnRight.AnchorAndAlignToObject(_gValue, SideEnum.Right, SideEnum.CenterY, 12);

                _fValue = baseValue;
                UpdateValue();

                _gText.SetText(text);

                AddControl(_gText);
                AddControl(_btnLeft);
                AddControl(_gValue);
                AddControl(_btnRight);

                Height = _btnLeft.Height;
                Width = _btnRight.Right - _gText.Left;
            }

            public void BtnLeftClick()
            {
                if (_fValue - _fValChange >= _fMin)
                {
                    _fValue -= _fValChange;
                    UpdateValue();

                    _del();
                }
            }
            public void BtnRightClick()
            {
                if (_fValue + _fValChange <= _fMax)
                {
                    _fValue += _fValChange;
                    UpdateValue();

                    _del();
                }
            }
            public void RefreshValue(float value)
            {
                _fValue = value;
                UpdateValue();
            }
            private void UpdateValue()
            {
                _gValue.SetText((int)_fValue);
                _gValue.AnchorAndAlignToObject(_btnRight, SideEnum.Left, SideEnum.CenterY, 12);
            }
        }
    }
}
