using Bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Serenity
{
    class Asigaru : Archetype
    {
        readonly Provider provider = new Provider();

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
