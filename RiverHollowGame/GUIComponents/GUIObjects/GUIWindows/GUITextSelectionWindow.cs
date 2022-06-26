using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        string _sStatement;
        protected Point _poiMouse = Point.Zero;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected Dictionary<int, SelectionData> _diOptions;

        public GUITextSelectionWindow(TextEntry selectionText, bool open = true)
        {
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
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            _giSelection = new GUIImage(new Rectangle(288, 96, 8, 9), GameManager.ScaleIt(8), GameManager.ScaleIt(9), DataManager.DIALOGUE_TEXTURE);
            _giSelection.AnchorAndAlignToObject(_giText, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_giSelection);

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
                case TextEntrySelectionEnum.MerchantTalk:
                    AddMerchantTalkOptions();
                    break;
                case TextEntrySelectionEnum.YesNo:
                    AddYesNoOptions();
                    break;
                case TextEntrySelectionEnum.Shop:
                    AddShopOptions();
                    break;
                case TextEntrySelectionEnum.Party:
                    AddPartyOptions();
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
            if (GameManager.CurrentNPC != null && CurrentNPC.IsActorType(WorldActorTypeEnum.Villager))
            {
                Villager v = (Villager)CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };

                if (v.CanGiveGift) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Gift")); }
                if (v.Combatant && v.GetFriendshipLevel() >= 2) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Party")); }
                if (_textEntry.HasTag("ShipGoods")) { liCommands.Add(DataManager.GetGameTextEntry("Selection_ShipGoods")); }
                if (v.CanBeMarried)
                {
                    if (v.AvailableToDate()) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Date")); }
                    else if (v.AvailableToMarry() && InventoryManager.HasItemInPlayerInventory(int.Parse(DataManager.Config[16]["ItemID"]), 1)) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Propose")); }
                }
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        /// <summary>
        /// Adds the appropriate Merchant talk options
        /// </summary>
        private void AddMerchantTalkOptions()
        {
            if (GameManager.CurrentNPC != null)
            {
                TalkingActor act = CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };
                liCommands.Add(DataManager.GetGameTextEntry("Selection_Buy"));
                liCommands.Add(DataManager.GetGameTextEntry("Selection_Requests"));
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        /// <summary>
        /// Adds the TextEntry options for a Shop
        /// </summary>
        private void AddShopOptions()
        {
            if (GameManager.CurrentNPC != null)
            {
                TalkingActor act = CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };

                if (act.CanGiveGift) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Gift")); }
                liCommands.Add(DataManager.GetGameTextEntry("Selection_Buy"));
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        /// <summary>
        /// Constructs new TextEntry objects for each character in the party
        /// with an Option_# verb attached
        /// </summary>
        private void AddPartyOptions()
        {
            List<TextEntry> liCommands = new List<TextEntry>();

            int i = 0;
            foreach(ClassedCombatant act in PlayerManager.GetParty())
            {
                string verb = "Option_" + i++;
                Dictionary<string, string> stringData = new Dictionary<string, string> { ["Text"] = act.Name(), ["TextVerb"] = verb };

                liCommands.Add(new TextEntry(verb, stringData));
            }

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
                if (InputManager.CheckPressedKey(Keys.W) || InputManager.CheckPressedKey(Keys.Up))
                {
                    if (_iKeySelection - 1 >= 0)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                        _iKeySelection--;
                    }
                }
                else if (InputManager.CheckPressedKey(Keys.S) || InputManager.CheckPressedKey(Keys.Down))
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
                    if (_poiMouse != GUICursor.Position.ToPoint() && Contains(GUICursor.Position.ToPoint()))
                    {
                        _poiMouse = GUICursor.Position.ToPoint();
                        if (_iKeySelection - 1 >= 0 && GUICursor.Position.Y < _giSelection.Position().Y)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                            _iKeySelection--;
                        }
                        else if (_iKeySelection + 1 < _diOptions.Count && GUICursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.Bottom);
                            _iKeySelection++;
                        }
                    }
                }

                if (InputManager.CheckPressedKey(Keys.Enter))
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

            TalkingActor act = GameManager.CurrentNPC;
            switch (chosenAction.TextVerb)
            {
                case TextEntryVerbEnum.Buy:
                    act?.OpenShop();
                    break;
                case TextEntryVerbEnum.Date:
                    if (act != null)
                    {
                        ((Villager)act).RelationshipState = RelationShipStatusEnum.Dating;
                        nextText = act.GetDialogEntry("DateYes");
                    }
                    break;
                case TextEntryVerbEnum.Gift:
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(DisplayTypeEnum.Gift));
                    break;
                case TextEntryVerbEnum.No:
                    switch (_textEntry.GameTrigger)
                    {
                        case TextEntryTriggerEnum.Donate:
                            ((Villager)GameManager.CurrentNPC).FriendshipPoints += 1000;
                            break;
                        case TextEntryTriggerEnum.ConfirmGift:
                        case TextEntryTriggerEnum.ConfirmPurchase:
                        case TextEntryTriggerEnum.UseItem:
                            GameManager.SetSelectedItem(null);
                            break;
                    }

                    //Just pop this here for now
                    if (act != null && act.IsActorType(WorldActorTypeEnum.ShippingGremlin))
                    {
                        act.PlayAnimation(AnimationEnum.Action2);
                    }
                    break;
                case TextEntryVerbEnum.Party:
                    nextText = act?.JoinParty();
                    break;
                case TextEntryVerbEnum.Propose:
                    if (act != null && act.GetFriendshipLevel() >= 8)
                    {
                        Villager v = ((Villager)act);
                        v.RelationshipState = RelationShipStatusEnum.Engaged;

                        nextText = act.GetDialogEntry("MarriageYes");
                    }
                    else { nextText = act?.GetDialogEntry("MarriageNo"); }
                    break;
                case TextEntryVerbEnum.ShipGoods:
                    ((ShippingGremlin)GameManager.CurrentNPC).OpenShipping();
                    break;
                case TextEntryVerbEnum.ShowRequests:
                    nextText = act?.OpenRequests();
                    break;
                case TextEntryVerbEnum.Talk:
                    nextText = act?.GetDailyDialogue();
                    break;
                case TextEntryVerbEnum.Yes:
                    switch (_textEntry.GameTrigger)
                    {
                        case TextEntryTriggerEnum.GetBaby:
                            PlayerManager.DetermineBabyAcquisition();
                            break;
                        case TextEntryTriggerEnum.ConfirmGift:
                            nextText = act?.Gift(GameManager.CurrentItem);
                            GUIManager.CloseMainObject();
                            GameManager.SetSelectedItem(null);
                            break;
                        case TextEntryTriggerEnum.ConfirmPurchase:
                            MapManager.CurrentMap.TheShop.Purchase(GameManager.CurrentItem);
                            break;
                        case TextEntryTriggerEnum.Donate:
                            ((Villager)GameManager.CurrentNPC).FriendshipPoints += 40;
                            break;
                        case TextEntryTriggerEnum.EndDay:
                            Vector2 pos = PlayerManager.PlayerActor.CollisionBox.Center.ToVector2();
                            PlayerManager.SetPath(TravelManager.FindPathToLocation(ref pos, MapManager.CurrentMap.DictionaryCharacterLayer["PlayerSpawn"]));
                            GUIManager.SetScreen(new DayEndScreen());
                            break;
                        case TextEntryTriggerEnum.Exit:
                            break;
                        case TextEntryTriggerEnum.PetFollow:
                            PlayerManager.PlayerActor.SetPet((Pet)GameManager.CurrentNPC);
                            break;
                        case TextEntryTriggerEnum.PetUnfollow:
                            PlayerManager.PlayerActor.SetPet(null);
                            break;
                        case TextEntryTriggerEnum.UseItem:
                            GameManager.CurrentItem.UseItem(chosenAction.TextVerb);
                            GameManager.SetSelectedItem(null);
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

        protected void AssignToColumn()
        {
            for (int i = 0; i < _diOptions.Count; i++)
            {
                GUIText gText = _diOptions[i].GText;
                if (i == 0) { gText.AnchorAndAlignToObject(_giSelection, SideEnum.Right, SideEnum.Bottom); }
                else { gText.AnchorAndAlignToObject(_diOptions[i - 1].GText, SideEnum.Bottom, SideEnum.Left); }
                AddControl(gText);
            }
        }

        protected class SelectionData
        {
            GUIText _gText;
            public GUIText GText => _gText;

            public string Text => _gText.Text;

            public TextEntry SelectionEntry { get; private set; }

            public SelectionData(TextEntry textEntry, string fontName = DataManager.FONT_NEW)
            {
                SelectionEntry = textEntry;
                _gText = new GUIText(textEntry.GetFormattedText(), true, fontName);
            }
        }

        public override bool IsSelectionBox() { return true; }
    }
}
