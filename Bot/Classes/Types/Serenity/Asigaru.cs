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
            _name = name;
            _id = id;
            _lvl = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(Id), "discord_id", "users"));
            _health = 33;
            _damage = 43;
            _armor = 23;
            _protection = 1.5f;
            _dodge = 0.3f;
            _luck = 0.5f;
            _multiplier = 1.60f;
        }
    }
}
