using Bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Honor
{
    class Honor : Archetype
    {
        readonly Provider provider = new Provider();

        public int TakeBonusForLevel(int level, string stat)
        {
            int[] lvlhealth = { 40, 52, 67, 87, 113, 146, 189, 245, 318, 413, 536, 696, 904, 1175, 1527, 1985, 2580, 3354, 4360, 5668 };
            int[] lvldamage = { 37, 48, 62, 80, 104, 135, 175, 227, 295, 383, 497, 646, 839, 1090, 1417, 1842, 2394, 3112, 4045, 5258 };
            int[] lvlarmor = { 37, 41, 53, 69, 90, 116, 151, 196, 254, 330, 428, 556, 723, 940, 1221, 1588, 2064, 2683, 3488, 4534 };

            if (stat.Equals("health"))
                return lvlhealth[level - 1];
            else if (stat.Equals("damage"))
                return lvldamage[level - 1];
            else if (stat.Equals("armor"))
                return lvlarmor[level - 1];

            return 0;
        }

        public Honor(string name, ulong id)
        {
            int level = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(id), "discord_id", "users"));
            _name = name;
            _id = id;
            _lvl = level;
            _health = TakeBonusForLevel(level, "health");
            _damage = TakeBonusForLevel(level, "damage");
            _armor = TakeBonusForLevel(level, "armor");
            _protection = 1.8f;
            _dodge = 0.1f;
            _luck = 0.2f;
            _multiplier = 1.6f;
        }
    }
}
