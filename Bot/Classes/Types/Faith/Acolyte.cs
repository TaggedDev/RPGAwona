using System;
using Bot.Modules;
using Microsoft.Data.Sqlite;

namespace Bot.Types
{
    class Acolyte : Archetype
    {
        readonly Provider provider = new Provider();

        public Acolyte(string name, ulong id)
        {
            _name = name;
            _id = id;
            _lvl = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(Id), "discord_id", "users"));
            _health = 38;
            _damage = 34;
            _armor = 42;
            _protection = 1.6f;
            _dodge = 0.6f;
            _luck = 0.2f;
            _multiplier = 1.6f;
        }
    }
}
