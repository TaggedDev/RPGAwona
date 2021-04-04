using Bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Magic
{
    class Alchemist : Archetype
    {
        readonly Provider provider = new Provider();

        private int TakeBonusForLevel(int level, string stat)
        {
            int[] lvlhealth = { 38, 49, 63, 81, 105, 136, 176, 228, 296, 384, 499, 648, 842, 1094, 1422, 1848, 2402, 3122, 4058, 5275 };
            int[] lvldamage = { 34, 44, 57, 74, 96, 124, 161, 209, 271, 352, 457, 594, 772, 1003, 1303, 1693, 2200, 2860, 3718, 4833 };
            int[] lvlarmor = { 42, 54, 70, 91, 118, 153, 198, 257, 334, 434, 564, 733, 952, 1237, 1608, 2090, 2717, 3532, 4591, 5968 };

            if (stat.Equals("health"))
                return lvlhealth[level - 1];
            else if (stat.Equals("damage"))
                return lvldamage[level - 1];
            else if (stat.Equals("armor"))
                return lvlarmor[level - 1];

            return 0;
        }

        public Alchemist(string name, ulong id)
        {
            int level = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(id), "discord_id", "users"));
            _name = name;
            _id = id;
            _lvl = level;
            _health = TakeBonusForLevel(level, "health");
            _damage = TakeBonusForLevel(level, "damage");
            _armor = TakeBonusForLevel(level, "armor");
            _protection = 1.7f;
            _dodge = 0.2f;
            _luck = 0.4f;
            _multiplier = 1.70f;
        }
    }
}
