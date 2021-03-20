using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Classes.Items
{
    class Potion : Item
    {
        private int _effect;
        public Potion(string name, int requiredLevel, int price, int effect)
        {
            _name = name;
            _type = "зелье";
            _requiredLevel = requiredLevel;
            _price = price;
            _effect = effect;
            _isEquiped = false;
        }
    }
}
