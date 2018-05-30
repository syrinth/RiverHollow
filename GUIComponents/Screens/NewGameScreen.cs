﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
{
    class NewGameScreen : GUIScreen
    {
        List<Color> _liColors = new List<Color>() { Color.Red, Color.Blue, Color.Violet };
        int _iHairColorIndex;
        int _iHairTypeIndex;
        int _iHairTypeMax = GameContentManager.GetTexture(@"Textures\texPlayerHair").Height / 32;
        enum SelectionEnum { None, Name, Manor };
        SelectionEnum _selection;
        const int BTN_HEIGHT = 32;
        const int BTN_WIDTH= 128;
        GUIWindow _window;
        GUIButton _btnOK;
        GUIButton _btnCancel;
        GUITextInputWindow _nameWindow;
        GUITextInputWindow _manorWindow;

        List<GUIObject> _liClasses;
        ClassSelectionBox _selectedClass;
        PlayerDisplayBox _playerDisplayBox;

        GUIText _gTextHairColor, _gTextHairType;
        GUIImage _giNextHairColor, _giNextHairType;

        public NewGameScreen()
        {
            _iHairColorIndex = 0;
            int startX = ((RiverHollow.ScreenWidth - RiverHollow.ScreenHeight) / 2) - GUIWindow.BrownWin.Edge;

            _window = new GUIWindow(new Vector2(startX, 0), GUIWindow.BrownWin, RiverHollow.ScreenHeight, RiverHollow.ScreenHeight);
            Controls.Add(_window);

            _btnCancel = new GUIButton("Cancel", BTN_WIDTH, BTN_HEIGHT);
            _btnCancel.AnchorToInnerSide(_window, SideEnum.BottomRight, 0);
            
            _btnOK = new GUIButton("OK", BTN_WIDTH, BTN_HEIGHT);
            _window.Controls.Add(_btnOK);
            _btnOK.AnchorAndAlignToObject(_btnCancel, SideEnum.Left, SideEnum.Top, 0);
            
            _manorWindow = new GUITextInputWindow("Manor Name:", SideEnum.Left);
            _manorWindow.AnchorToInnerSide(_window, SideEnum.TopRight);
            
            _nameWindow = new GUITextInputWindow("Character Name:", SideEnum.Left);
            _nameWindow.AnchorAndAlignToObject(_manorWindow, SideEnum.Bottom, SideEnum.Right );

            _liClasses = new List<GUIObject>();
            for (int i = 1; i <= ObjectManager.GetWorkerNum(); i++) {
                ClassSelectionBox w = new ClassSelectionBox(Vector2.Zero, ObjectManager.GetWorker(i));
                _liClasses.Add(w);
                _window.Controls.Add(w);
                Controls.Add(w);
            }
            _selectedClass = (ClassSelectionBox)_liClasses[0];
            _selectedClass.PlayAnimation("WalkDown");

            _playerDisplayBox = new PlayerDisplayBox();
            _playerDisplayBox.AnchorToInnerSide(_window, SideEnum.TopLeft);

            _gTextHairColor = new GUIText("Hair Color");
            _gTextHairColor.AnchorAndAlignToObject(_playerDisplayBox, SideEnum.Bottom, SideEnum.Left);

            _giNextHairColor = new GUIImage(Vector2.Zero, new Rectangle(288, 96, 32, 32), 32, 32, GameContentManager.GetTexture(@"Textures\Dialog"));
            _giNextHairColor.AnchorAndAlignToObject(_gTextHairColor, SideEnum.Right, SideEnum.Bottom, 10);

            _gTextHairType = new GUIText("Hair Type");
            _gTextHairType.AnchorAndAlignToObject(_gTextHairColor, SideEnum.Bottom, SideEnum.Left);

            _giNextHairType = new GUIImage(Vector2.Zero, new Rectangle(288, 96, 32, 32), 32, 32, GameContentManager.GetTexture(@"Textures\Dialog"));
            _giNextHairType.AnchorAndAlignToObject(_gTextHairType, SideEnum.Right, SideEnum.Bottom, 10);

            GUIObject.CreateSpacedRow(ref _liClasses, _window.Height / 2, _window.Position().X, _window.Width, 20);

            Controls.Add(_btnCancel);
            Controls.Add(_btnOK);
            Controls.Add(_manorWindow);
            Controls.Add(_nameWindow);
            Controls.Add(_gTextHairColor);
            Controls.Add(_giNextHairColor);
            Controls.Add(_gTextHairType);
            Controls.Add(_giNextHairType);

            _selection = SelectionEnum.None;
        }

        public override void Update(GameTime gameTime)
        {
            if(_selection == SelectionEnum.Name) { _nameWindow.Update(gameTime); }
            else if (_selection == SelectionEnum.Manor) { _manorWindow.Update(gameTime); }

            foreach (GUIObject o in _liClasses)
            {
                ((ClassSelectionBox)o).Update(gameTime);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                PlayerManager.World.SetScale();
                RiverHollow.NewGame();
                PlayerManager.SetClass(_selectedClass.ClassID);
                PlayerManager.SetName(_nameWindow.GetText());
                GameManager.DontReadInput();
                rv = true;
            }
            if (_btnCancel.Contains(mouse))
            {
                GUIManager.SetScreen(new IntroMenuScreen());
                GameManager.DontReadInput();
                rv = true;
            }
            if (_giNextHairColor.Contains(mouse))
            {
                if (_iHairColorIndex < _liColors.Count - 1) { _iHairColorIndex++; }
                else { _iHairColorIndex = 0; }
                PlayerManager.World.SetHairColor(_liColors[_iHairColorIndex]);
            }
            if (_giNextHairType.Contains(mouse))
            {
                if (_iHairTypeIndex < _iHairTypeMax - 1) { _iHairTypeIndex++; }
                else { _iHairTypeIndex = 0; }

                _playerDisplayBox.SyncHair(_iHairTypeIndex);
            }

            

            if (_nameWindow.Contains(mouse)) { _selection = SelectionEnum.Name; }
            else if (_manorWindow.Contains(mouse)) { _selection = SelectionEnum.Manor; }
            else { _selection = SelectionEnum.None;}

            foreach(GUIObject o in _liClasses)
            {
                if (o.Contains(mouse))
                {
                    ClassSelectionBox csb = ((ClassSelectionBox)o);
                    if (_selectedClass != csb)
                    {
                        csb.PlayAnimation("WalkDown");
                        _selectedClass.PlayAnimation("Idle");
                        _selectedClass = csb;
                    }
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnOK.IsMouseHovering = _btnOK.Contains(mouse);
            _btnCancel.IsMouseHovering = _btnCancel.Contains(mouse);
            return rv;
        }

        public class PlayerDisplayBox : GUIWindow
        {
            GUISprite _bodySprite;
            public GUISprite BodySprite => _bodySprite;
            GUISprite _eyeSprite;
            public GUISprite EyeSprite => _eyeSprite;
            GUISprite _hairSprite;
            public GUISprite HairSprite => _hairSprite;
            GUISprite _armSprite;
            public GUISprite ArmSprite => _armSprite;

            public PlayerDisplayBox()
            {
                _bodySprite = new GUISprite(PlayerManager.World.BodySprite);
                _eyeSprite = new GUISprite(PlayerManager.World.EyeSprite);
                _hairSprite = new GUISprite(PlayerManager.World.HairSprite);
                _armSprite = new GUISprite(PlayerManager.World.ArmSprite);

                _bodySprite.SetScale((int)GameManager.Scale);
                _eyeSprite.SetScale((int)GameManager.Scale);
                _hairSprite.SetScale((int)GameManager.Scale);
                _armSprite.SetScale((int)GameManager.Scale);

                _winData = GUIWindow.RedWin;
                
                PositionSprites();
            }

            public override void Update(GameTime gameTime)
            {
                _bodySprite.Update(gameTime);
                _eyeSprite.Update(gameTime);
                _hairSprite.Update(gameTime);
                _armSprite.Update(gameTime);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                _bodySprite.Draw(spriteBatch);
                _eyeSprite.Draw(spriteBatch);
                _hairSprite.Draw(spriteBatch);
                _armSprite.Draw(spriteBatch);
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                PositionSprites();
            }

            public void PlayAnimation(string animation)
            {
                _bodySprite.PlayAnimation(animation);
                _eyeSprite.PlayAnimation(animation);
                _hairSprite.PlayAnimation(animation);
                _armSprite.PlayAnimation(animation);
            }

            public void PositionSprites()
            {
                if (_bodySprite != null)
                {
                    _bodySprite.CenterOnWindow(this);
                    _bodySprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    Width = _bodySprite.Width + _bodySprite.Width / 3;
                    Height = _bodySprite.Height + 2 * _bodySprite.Height / 4;

                }
                if (_eyeSprite != null)
                {
                    _eyeSprite.CenterOnWindow(this);
                    _eyeSprite.AnchorToInnerSide(this, SideEnum.Bottom);
                }
                if (_hairSprite != null)
                {
                    _hairSprite.CenterOnWindow(this);
                    _hairSprite.AnchorToInnerSide(this, SideEnum.Bottom);
                }
                if (_armSprite != null)
                {
                    _armSprite.CenterOnWindow(this);
                    _armSprite.AnchorToInnerSide(this, SideEnum.Bottom);
                }
            }

            public void SyncHair(int index)
            {
                PlayerManager.World.SetHairType(index);

                _hairSprite.SetSprite(PlayerManager.World.HairSprite);
                _hairSprite.SetScale((int)GameManager.Scale);
                PositionSprites();
            }
        }

        public class ClassSelectionBox : GUIWindow
        {
            GUISprite _sprite;
            public GUISprite Sprite => _sprite;

            int _iClassID;
            public int ClassID => _iClassID;

            public ClassSelectionBox(Vector2 p, WorldAdventurer w)
            {
                _sprite = new GUISprite(w.BodySprite);
                _sprite.SetScale((int)GameManager.Scale);
                _iClassID = w.AdventurerID;
                Position(p);
                _winData = GUIWindow.RedWin;
                Width = _sprite.Width + _sprite.Width / 3;
                Height = _sprite.Height + 2 * _sprite.Height / 4;
                _sprite.CenterOnWindow(this);
                _sprite.AnchorToInnerSide(this, SideEnum.Bottom);
            }

            public override void Update(GameTime gameTime)
            {
                _sprite.Update(gameTime);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                _sprite.Draw(spriteBatch);
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                if (_sprite != null) {
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);
                }
            }

            public void PlayAnimation(string animation)
            {
                _sprite.PlayAnimation(animation);
            }
        }
    }
}
