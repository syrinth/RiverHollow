using Adventure.Characters;
using System.Collections.Generic;

namespace Adventure.Game_Managers
{
    public static class CombatManager
    {
        private static Mob _m;
        private static List<Monster> _listMonsters;
        public static List<Monster> Monsters { get => _listMonsters; }
        private static List<CombatCharacter> _listParty;
        public static List<CombatCharacter> Party { get => _listParty; }

        public static void NewBattle(Mob m)
        {
            _m = m;
            _listMonsters = _m.Monsters;
            _listParty = PlayerManager.GetParty();

            RiverHollow.ChangeGameState(RiverHollow.GameState.Combat);
        }

        public static void EndBattle()
        {
            MapManager.RemoveMob(_m);
            _m = null;
            RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
        }

        public static void Kill(CombatCharacter c)
        {
            //ToDo:
        }
    }
}
