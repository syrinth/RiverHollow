using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class NPCToken : Item
    {
        NPCTokenTypeEnum _eTokenType;
        int _iNPCID;
        public NPCToken(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");
            Util.AssignValue(ref _eTokenType, "Subtype", stringData);
            Util.AssignValue(ref _iNPCID, "NPC_ID", stringData);

            _bStacks = false;
        }

        public override bool AddToInventoryTrigger()
        {
            if (_eTokenType == NPCTokenTypeEnum.Mount)
            {
                Mount act = DataManager.CreateMount(_iNPCID);
                PlayerManager.AddMount(act);
                act.SpawnInHome();
            }
            else if (_eTokenType == NPCTokenTypeEnum.Pet)
            {
                Pet act = DataManager.CreatePet(_iNPCID);
                PlayerManager.AddPet(act);
                act.SpawnNearPlayer();
                if (PlayerManager.PlayerActor.ActivePet == null)
                {
                    PlayerManager.PlayerActor.SetPet(act);
                }
            }

            return true;
        }
    }
}
