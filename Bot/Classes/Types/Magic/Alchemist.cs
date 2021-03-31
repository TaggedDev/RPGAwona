using Bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Magic
{
    class Alchemist : Archetype
    {
        readonly Provider provider = new Provider();

        public Alchemist(string name, ulong id)
        {
            _name = name;
            _id = id;
            _lvl = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(Id), "discord_id", "users"));
            _health = 34;
            _damage = 45;
            _armor = 22;
            _protection = 1.7f;
            _dodge = 0.2f;
            _luck = 0.4f;
            _multiplier = 1.70f;
        }
    }
}
