using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Faith
{
    class Cardinal : Archetype
    {
        public Cardinal(string name, int id, string race, int health, int damage, float luck, float dodge)
        {
            _name = name;
            _id = id;
            _health = health + 775;
            _damage = damage + 60;
            _luck = luck + 0.21f;
            _dodge = dodge + 0.17f;
        }
    }
}
