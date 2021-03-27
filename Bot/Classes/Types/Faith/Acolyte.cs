using System;
using Microsoft.Data.Sqlite;

namespace Bot.Types
{
    class Acolyte : Archetype
    {
        public Acolyte(string name, ulong id, int health, int defence, int damage, float luck, float dodge)
        {
            _name = name;
            _id = id;
            _health = health + 250;
            _damage = damage + 20;
            _defence = defence + 10;
            _luck = luck + 0.12f;
            _dodge = dodge + 0.05f;
        }
        public Acolyte(string name, ulong id)
        {
            _name = name;
            _id = id;
            _health = 250;
            _protection = .4f;
            _multiplier = 1.45f;
            _defence = 100;
            _damage = 100;
            _luck = 0.12f;
            _dodge = 0.05f;
        }
    }
}
