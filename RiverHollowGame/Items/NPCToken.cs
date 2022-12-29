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
        private NPCTokenTypeEnum TokenType => DataManager.GetEnumByIDKey<NPCTokenTypeEnum>(ID, "Subtype", DataType.Item);
        private int NPCID => DataManager.GetIntByIDKey(ID, "NPC_ID", DataType.Item);

        public NPCToken(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");
        }

        public override bool AddToInventoryTrigger()
        {
            if (TokenType == NPCTokenTypeEnum.Mount)
            {
                Mount act = DataManager.CreateMount(NPCID);
                PlayerManager.AddMount(act);
                act.SpawnInHome();
            }
            else if (TokenType == NPCTokenTypeEnum.Pet)
            {
                Pet act = DataManager.CreatePet(NPCID);
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
