using RiverHollow.Characters;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        private static Mob _mob;
        public static Mob CurrentMob { get => _mob; }
        private static List<Monster> _listMonsters;
        public static List<Monster> Monsters { get => _listMonsters; }
        private static List<CombatCharacter> _listParty;
        public static List<CombatCharacter> Party { get => _listParty; }

        public static void NewBattle(Mob m)
        {
            _mob = m;
            _listMonsters = _mob.Monsters;
            _listParty = PlayerManager.GetParty();

            RiverHollow.ChangeGameState(RiverHollow.GameState.Combat);
        }

        public static void EndBattle()
        {
            MapManager.RemoveMob(_mob);
            _mob = null;
            RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
        }

        public static void Kill(CombatCharacter c)
        {
            //ToDo:
        }
    }
}
