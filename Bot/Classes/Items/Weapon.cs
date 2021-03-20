using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Classes.Items
{
    class Weapon : Item
    {
        private int _damage;
        private int _critChance;

        public Weapon(string name, string type, int level, int damage, int critchance, int price, int slot)
        {
            _name = name;
            _type = type;
            _damage = damage;
            _critChance = critchance;
            _price = price;
            _slot = slot;
        }
    }
}
