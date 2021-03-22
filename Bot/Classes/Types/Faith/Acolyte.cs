using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types
{
    class Acolyte : Archetype
    {
        /*public override string name { get => _name; set => _name = value; } // Username#0000
        public override ulong id { get => _id; set => _id = value; } // discord id 
        public override string race { get => _race; set => _race = value; } // current race 
        public override int health { get => _health; set => _health = value; } // health point
        public override int lvl { get => _lvl; set => _lvl = value; }
        public override int defence { get => _defence; set => _defence = value; } // health point
        public override int damage { get => _damage; set => _damage = value; } // damage parameter
        public override float luck { get; set; } // luck parameter
        public override float multiplier { get; set; } // luck multiplier
        public override float dodge { get; set; } // dodge chance
        public override string move { get; set; } // player's choose*/

        public Acolyte(string name, ulong id, int health, int damage, float luck, float dodge)
        {
            _name = name;
            _id = id;
            _health = health + 250;
            _damage = damage + 20;
            _luck = luck + 0.12f;
            _dodge = dodge + 0.05f;
        }
        public Acolyte(string name, ulong id)
        {
            _name = name;
            _id = id;
            _health = 250;
            _damage = 20;
            _luck = 0.12f;
            _dodge = 0.05f;
        }

    }
}
