using Bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Melee
{
    class Komtur : Archetype
    {
        readonly Provider provider = new Provider();

        public Komtur(string name, ulong id)
        {
            _name = name;
            _id = id;
            _lvl = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(Id), "discord_id", "users"));
            _health = 40;
            _damage = 37;
            _armor = 41;
            _protection = 1.8f;
            _dodge = 0.1f;
            _luck = 0.2f;
            _multiplier = 1.6f;
        }
    }
}
