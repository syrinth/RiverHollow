using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.Combat;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
{
    public class TacticalCombatScreen : GUIScreen
    {
        GUIButton _btnAcceptDefeat;
        GUIActionSelectObject _gActionSelect;
        GUITacticalTurnOrderDisplay _gTurnOrder;
        GUITacticalCombatantInfo _gActorInfoPanel;

        private Dictionary<FloatingText, bool> _diFTextQueue;
        private List<FloatingText> _liFloatingText;

        public TacticalCombatScreen()
        {
            //_sdStamina = new GUIStatDisplay(PlayerManager.GetStamina, Color.Red);
            //AddControl(_sdStamina);
            _diFTextQueue = new Dictionary<FloatingText, bool>();
            _liFloatingText = new List<FloatingText>();

            _gTurnOrder = new GUITacticalTurnOrderDisplay();
            _gTurnOrder.AnchorToScreen(SideEnum.Top);
            AddControl(_gTurnOrder);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            foreach (KeyValuePair<FloatingText, bool> kvp in _diFTextQueue)
            {
                if (kvp.Value) { _liFloatingText.Add(kvp.Key); }
                else { _liFloatingText.Remove(kvp.Key); }
            }
            _diFTextQueue.Clear();
            foreach (FloatingText text in _liFloatingText)
            {
                text.Update(gTime);
            }

            switch (TacticalCombatManager.CurrentPhase)
            {
                case TacticalCombatManager.CmbtPhaseEnum.MainSelection:
                    if (TacticalCombatManager.ActiveCharacter.Paused) { return; }
                    _gActionSelect?.Update(gTime);
                    break;

                case TacticalCombatManager.CmbtPhaseEnum.DisplayDefeat:
                    GUIWindow window = new GUIWindow();
                    window.CenterOnScreen();
                    AddControl(window);

                    _btnAcceptDefeat = new GUIButton("OK", AcceptDefeat);
                    _btnAcceptDefeat.AnchorToInnerSide(window, SideEnum.Bottom);


                    GUIText text = new GUIText(DataManager.GetGameTextEntry("Defeat").FormattedText);
                    text.AnchorToInnerSide(window, SideEnum.Top);
                    break;
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_guiTextWindow != null) { rv = _guiTextWindow.ProcessLeftButtonClick(mouse); }
            if (_btnAcceptDefeat != null) { rv = _btnAcceptDefeat.ProcessLeftButtonClick(mouse); }

            if (!rv)
            {
                if (TacticalCombatManager.AreWeSelectingAnAction())
                {
                    if (_gActionSelect != null)
                    {
                        rv = _gActionSelect.ProcessLeftButtonClick(mouse);
                    }
                }
                else if (TacticalCombatManager.CombatPhaseCheck(TacticalCombatManager.CmbtPhaseEnum.ChooseMoveTarget))
                {
                    TacticalCombatManager.SetMoveTarget();
                    rv = true;
                }
                else if (TacticalCombatManager.CombatPhaseCheck(TacticalCombatManager.CmbtPhaseEnum.ChooseActionTarget))
                {
                    TacticalCombatManager.SelectedAction.UseSkillOnTarget();
                    rv = true;
                }
            }

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            //switch (CombatManager.CurrentPhase)
            //{  
            //    case CombatManager.PhaseEnum.DisplayVictory:
            //        rv = _gPostScreen.ProcessLeftButtonClick(mouse);
            //        break;
            //    case CombatManager.PhaseEnum.Defeat:
            //        GUIManager.BeginFadeOut(true);
            //        BackToMain();
            //        MapManager.CurrentMap = MapManager.Maps["mapHospital"];
            //        PlayerManager.CurrentMap = "mapHospital";
            //        PlayerManager.World.Position = Util.SnapToGrid(MapManager.CurrentMap.DictionaryCharacterLayer["playerSpawn"]);
            //        GUIManager.OpenTextWindow(ObjectManager.DiNPC[7].GetDialogEntry("Healed"), ObjectManager.DiNPC[7]);

            //        foreach (ClassedCombatant c in PlayerManager.GetParty())
            //        {
            //            c.ClearConditions();
            //            c.ModifyHealth((int)(c.MaxHP * 0.10), false);
            //        }

            //        break;
            //}

            return rv;
        }

        //Right clicking will deselect the chosen skill
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;

            if (TacticalCombatManager.CanCancel())
            {
                CancelAction();
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            RHTile hoverTile = MapManager.CurrentMap.GetTileByPixelPosition(GUICursor.GetWorldMousePosition());
            if (hoverTile?.Character != null)
            {
                if (_gActorInfoPanel?.Character != hoverTile.Character)
                {
                    RemoveControl(_gActorInfoPanel);
                    _gActorInfoPanel = null;

                    _gActorInfoPanel = new GUITacticalCombatantInfo(hoverTile.Character);
                    _gActorInfoPanel.AnchorToScreen(SideEnum.BottomRight);
                    AddControl(_gActorInfoPanel);
                }
            }
            else
            {
                RemoveControl(_gActorInfoPanel);
                _gActorInfoPanel = null;
            }

            if (!rv && _gActionSelect != null)
            {
                rv = _gActionSelect.ProcessHover(mouse);
            }

            return rv;
        }

        public void AcceptDefeat()
        {
            TacticalCombatManager.EndCombatDefeat();
        }

        public void CancelAction()
        {
            _gActionSelect.CancelAction();
        }

        /// <summary>
        /// Draws the Combat objects that exist in WorldSpace
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawUpperCombatLayer(SpriteBatch spriteBatch)
        {
            TacticalCombatManager.SelectedAction?.Draw(spriteBatch);
            foreach (FloatingText text in _liFloatingText)
            {
                text.Draw(spriteBatch);
            }
        }

        #region CombatManager Controls
        public void OpenMainSelection()
        {
            if(_gActionSelect == null)
            {
                _gActionSelect = new GUIActionSelectObject();
                AddControl(_gActionSelect);
            }
        }
        public void CloseMainSelection()
        {
            if (_gActionSelect != null)
            {
                RemoveControl(_gActionSelect);
                _gActionSelect = null;
            }
        }

        public void HideMainSelection()
        {
            _gActionSelect?.Show(false);
        }
        #endregion

        #region FloatingText Handling
        /// <summary>
        /// Returns true if there are any Floating Text objects.
        /// </summary>
        /// <returns></returns>
        public bool AreThereFloatingText() { return _liFloatingText.Count > 0; }

        /// <summary>
        /// Adds the FloatingText object to the removal queue
        /// </summary>
        public void RemoveFloatingText(FloatingText fText)
        {
            _diFTextQueue[fText] = false;
        }

        /// <summary>
        /// Adds the FloatingText object to the queue to add
        /// </summary>
        public void AddFloatingText(FloatingText fText)
        {
            _diFTextQueue[fText] = true;
        }

        #endregion
    }
}
