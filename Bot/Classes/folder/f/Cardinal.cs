using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Faith
{
    class Cardinal : Archetype
    {


        public Cardinal(string name, ulong id, string race, int health, int damage, float luck, float dodge)
        {
            _name = name;
            _id = id;
            _health = health + 250;
            _damage = damage + 20;
            _luck = luck + 0.12f;
            _dodge = dodge + 0.05f;
        }
    }
}
