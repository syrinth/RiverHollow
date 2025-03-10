﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        protected const int SELECTION_MAX_ROWS = 6;

        string _sStatement;
        protected Point _poiMouse = Point.Zero;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected Dictionary<int, SelectionData> _diOptions;

        public GUITextSelectionWindow(TextEntry selectionText, bool open = true)
        {
            _iMaxRows = SELECTION_MAX_ROWS;

            _giSelection = new GUIImage(GUIUtils.BTN_RIGHT_SMALL);
            AddControl(_giSelection);
            _textEntry = selectionText;
            _textEntry.HandlePreWindowActions();
            _diOptions = new Dictionary<int, SelectionData>();
            ConfigureHeight();
            _iKeySelection = 0;
            ConstructSelectionMenu();

            PostParse();

            Setup(open);
        }

        public void PostParse()
        {
            SyncText(_sStatement, true);
            _gText.AnchorToInnerSide(this, SideEnum.TopLeft, BORDER_EDGE);
            _giSelection.AnchorAndAlignWithSpacing(_gText, SideEnum.Bottom, SideEnum.Left, BORDER_EDGE);
            
            AssignToColumn();
        }

        #region Selection Menu Creation
        /// <summary>
        /// Constructs the appropriate Selection Menu based on the TextEntry's selection type
        /// </summary>
        private void ConstructSelectionMenu()
        {
            _sStatement = _textEntry.GetFormattedText();
            switch (_textEntry.SelectionType)
            {
                case TextEntrySelectionEnum.VillageTalk:
                    AddVillagerTalkOptions();
                    break;
                case TextEntrySelectionEnum.YesNo:
                    AddYesNoOptions();
                    break;
                case TextEntrySelectionEnum.Bed:
                    AddBedOptions();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Adds the appropriate Villager talk options
        /// </summary>
        private void AddVillagerTalkOptions()
        {
            if (GameManager.CurrentNPC != null && CurrentNPC.IsActorType(ActorTypeEnum.Villager))
            {
                Villager v = (Villager)CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };

                if (v.CanGiveGift) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Gift")); }
                if (v.CanBeMarried)
                {
                    if (v.AvailableToDate()) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Date")); }
                    else if (v.AvailableToMarry() && InventoryManager.HasItemInPlayerInventory(int.Parse(DataManager.Config[16]["ItemID"]), 1)) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Propose")); }
                }
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        private void AddBedOptions()
        {
            List<TextEntry> liCommands = new List<TextEntry>
            {
                DataManager.GetGameTextEntry("Selection_Sleep"),
                DataManager.GetGameTextEntry("Selection_NeverMind")
            };

            AddOptions(liCommands);
        }

        /// <summary>
        /// Adds the Yes and No TextEntry objects
        /// </summary>
        private void AddYesNoOptions()
        {
            List<TextEntry> liCommands = new List<TextEntry> {
                DataManager.GetGameTextEntry("Selection_Yes"),
                DataManager.GetGameTextEntry("Selection_No")
            };

            AddOptions(liCommands);
        }

        /// <summary>
        /// Adds the given list of TextEntries to the _diOptions dictionary
        /// </summary>
        /// <param name="liCommands"></param>
        private void AddOptions(List<TextEntry> liCommands)
        {
            int key = 0;
            foreach (TextEntry s in liCommands)
            {
                _diOptions.Add(key++, new SelectionData(s));
            }
        }
        #endregion

        public override void Update(GameTime gTime)
        {
            if (_bOpening)
            {
                HandleOpening(gTime);
            }
            else
            {
                if (InputManager.CheckForInitialKeyDown(Keys.W) || InputManager.CheckForInitialKeyDown(Keys.Up))
                {
                    if (_iKeySelection - 1 >= 0)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                        _iKeySelection--;
                    }
                }
                else if (InputManager.CheckForInitialKeyDown(Keys.S) || InputManager.CheckForInitialKeyDown(Keys.Down))
                {
                    if (_iKeySelection + 1 < _diOptions.Count)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.Bottom);
                        _iKeySelection++;
                    }
                }
                else
                {
                    //Until fixed for specific motion
                    if (_poiMouse != GUICursor.Position && Contains(GUICursor.Position))
                    {
                        _poiMouse = GUICursor.Position;
                        if (_iKeySelection - 1 >= 0 && GUICursor.Position.Y < _giSelection.Position().Y)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.CenterY);
                            _iKeySelection--;
                        }
                        else if (_iKeySelection + 1 < _diOptions.Count && GUICursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.CenterY);
                            _iKeySelection++;
                        }
                    }
                }

                if (InputManager.CheckForInitialKeyDown(Keys.Enter))
                {
                    SelectAction();
                }
            }
        }

        /// <summary>
        /// Triggered when the user selects an option in the GUITextSelectionWindow
        /// 
        /// This method analyzes the chosen TextEntry object and determines what actions to take
        /// as a result of the chosenAction and the current TextEntry
        /// </summary>
        protected void SelectAction()
        {
            TextEntry chosenAction = _diOptions[_iKeySelection].SelectionEntry;
            TextEntry nextText = null;

            TalkingActor npc = GameManager.CurrentNPC;
            switch (chosenAction.TextVerb)
            {
                case TextEntryVerbEnum.EndDay:
                    Point pos = PlayerManager.PlayerActor.CollisionCenter;
                    PlayerManager.SetPath(TravelManager.FindPathToLocation(ref pos, MapManager.CurrentMap.GetCharacterObject("PlayerSpawn").Location));
                    GUIManager.SetScreen(new DayEndScreen());
                    break;
                case TextEntryVerbEnum.Date:
                    if (npc != null)
                    {
                        ((Villager)npc).RelationshipState = RelationShipStatusEnum.Dating;
                        nextText = npc.GetDialogEntry("DateYes");
                    }
                    break;
                case TextEntryVerbEnum.Gift:
                    GUIManager.OpenMainObject(new HUDGiftWindow((Villager)npc));
                    break;
                case TextEntryVerbEnum.No:
                    switch (_textEntry.GameTrigger)
                    {
                        case TextEntryTriggerEnum.Donate:
                            ((Villager)GameManager.CurrentNPC).FriendshipPoints += 1000;
                            break;
                        case TextEntryTriggerEnum.ConfirmPurchase:
                            break;
                    }
                    break;
                case TextEntryVerbEnum.Party:
                    nextText = npc?.JoinParty();
                    break;
                case TextEntryVerbEnum.Propose:
                    if (npc != null && npc.GetFriendshipLevel() >= 8)
                    {
                        Villager v = ((Villager)npc);
                        v.RelationshipState = RelationShipStatusEnum.Engaged;

                        nextText = npc.GetDialogEntry("MarriageYes");
                    }
                    else { nextText = npc?.GetDialogEntry("MarriageNo"); }
                    break;
                case TextEntryVerbEnum.Talk:
                    nextText = npc?.GetDailyDialogue();
                    break;
                case TextEntryVerbEnum.Yes:
                    switch (_textEntry.GameTrigger)
                    {
                        case TextEntryTriggerEnum.GetBaby:
                            PlayerManager.DetermineBabyAcquisition();
                            break;
                        case TextEntryTriggerEnum.ConfirmPurchase:
                            MapManager.CurrentMap.TheShop.Purchase(GameManager.CurrentMerchandise);
                            break;
                        case TextEntryTriggerEnum.Donate:
                            ((Villager)GameManager.CurrentNPC).FriendshipPoints += 40;
                            break;
                        case TextEntryTriggerEnum.PetFollow:
                            PlayerManager.PlayerActor.SetPet((Pet)GameManager.CurrentNPC);
                            break;
                        case TextEntryTriggerEnum.PetUnfollow:
                            PlayerManager.PlayerActor.SetPet(null);
                            break;

                    }
                    break;
            }

            if (nextText == null) { GUIManager.CloseTextWindow(); }
            else { GUIManager.SetWindowText(nextText, true); }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                SelectAction();
                rv = true;
            }
            return rv;
        }

        internal void Clear()
        {
            _iKeySelection = 0;
            
            foreach (SelectionData g in _diOptions.Values)
            {
                if (Controls.Contains(g.GText))
                {
                    RemoveControl(g.GText);
                }
            }

            _diOptions.Clear();
        }

        protected override void SyncObjects()
        {
            base.SyncObjects();
            AssignToColumn();
            if (_diOptions.Count > 0)
            {
                _giSelection.AnchorAndAlignWithSpacing(_diOptions[0].GText, SideEnum.Left, SideEnum.CenterY, BORDER_EDGE);
            }
        }

        protected void AssignToColumn()
        {
            for (int i = 0; i < _diOptions.Count; i++)
            {
                GUIText gText = _diOptions[i].GText;
                if (i == 0)
                {
                    gText.AnchorAndAlign(_gText, SideEnum.Bottom, SideEnum.Left);
                    gText.MoveBy(_giSelection.Width + (GameManager.CurrentScale * 2), 0);
                }
                else { gText.AnchorAndAlign(_diOptions[i - 1].GText, SideEnum.Bottom, SideEnum.Left); }
                AddControl(gText);
            }
        }

        protected class SelectionData
        {
            readonly GUIText _gText;
            public GUIText GText => _gText;

            public TextEntry SelectionEntry { get; private set; }

            public SelectionData(TextEntry textEntry, string fontName = DataManager.FONT_MAIN)
            {
                SelectionEntry = textEntry;
                _gText = new GUIText(textEntry.GetFormattedText(), true, fontName);
            }
        }

        public override bool IsSelectionBox() { return true; }
    }
}
