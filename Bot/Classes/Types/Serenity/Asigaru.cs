using Bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Serenity
{
    class Asigaru : Archetype
    {
        readonly Provider provider = new Provider();

        private int TakeBonusForLevel(int level, string stat)
        {
            //int[] stats_sum = { 18, 21, 25, 30, 36, 43, 51, 61, 73, 87, 104, 124, 148, 177, 212, 254, 304, 364, 436, 523 };
            int[] lvlhealth = { 33, 42, 54, 70, 91, 118, 153, 198, 257, 334, 434, 564, 733, 952, 1237, 1608, 2090, 2717, 3532, 4591 };
            int[] lvldamage = { 43, 55, 71, 92, 119, 154, 200, 260, 338, 439, 570, 741, 963, 1251, 1626, 2113, 2746, 3569, 4639, 6030 };
            int[] lvlarmor = { 23, 29, 37, 48, 62, 80, 104, 135, 175, 227, 295, 383, 497, 646, 839, 1090, 1417, 1842, 2394, 3112 };

            if (stat.Equals("health"))
                return lvlhealth[level - 1];
            else if (stat.Equals("damage"))
                return lvldamage[level - 1];
            else if (stat.Equals("armor"))
                return lvlarmor[level - 1];

            return 0;
        }

        public Asigaru(string name, ulong id)
        {
            int level = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(id), "discord_id", "users")); 
            _name = name;
            _id = id;
            _lvl = level;
            _health = TakeBonusForLevel(level, "health");
            _damage = TakeBonusForLevel(level, "damage");
            _armor = TakeBonusForLevel(level, "armor");
            _protection = 1.5f;
            _dodge = 0.3f;
            _luck = 0.5f;
            _multiplier = 1.60f;
        }
    }
}
