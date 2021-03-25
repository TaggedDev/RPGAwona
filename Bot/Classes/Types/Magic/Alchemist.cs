using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types.Magic
{
    class Alchemist : Archetype
    {
        public Alchemist(string name, ulong id, int health, int damage, float luck, float dodge)
        {
            _name = name;
            _id = id;
            _health = health + 775;
            _damage = damage + 60;
            _luck = luck + 0.21f;
            _dodge = dodge + 0.17f;
        }

        public Alchemist(string name, ulong id)
        {
            _name = name;
            _id = id;
            _health = 775;
            _damage = 60;
            _luck = 0.21f;
            _dodge = 0.17f;
        }
    }
}
