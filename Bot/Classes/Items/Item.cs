using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Classes
{
    abstract class Item 
    {
        protected string _name;
        protected string _type;
        protected int _requiredLevel;
        protected int _price;
        protected int _slot;
        protected bool _isEquiped;
    }
}
