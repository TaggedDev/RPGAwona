using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types
{
    abstract class Archetype
    {
        protected string _name; // Username#0000
        protected ulong _id; // discord id 
        protected string _race; // current race 
        protected int _health; // health point
        protected int _lvl;
        protected int _defence; // health point
        protected int _damage; // damage parameter
        protected float _luck; // luck parameter
        protected float _multiplier; // luck multiplier
        protected float _dodge; // dodge chance
        protected string _move; // player's choose

        public virtual string Name { get => _name; set => _name = value; } // Username#0000
        public virtual ulong Id { get; set; } // discord id 
        public virtual string Race { get; set; } // current race 
        public virtual int Health { get => _health; set => _health = value; } // health point
        public virtual int Lvl { get => _lvl; set => _lvl = value; }
        public virtual int Defence { get => _defence; set => _defence = value; } // health point
        public virtual int Damage { get => _damage; set => _damage = value; } // damage parameter
        public virtual float Luck { get => _luck; set => _luck = value; } // luck parameter
        public virtual float Multiplier { get => _multiplier; set => _multiplier = value; } // luck multiplier
        public virtual float Dodge { get => _dodge; set => _dodge = value; } // dodge chance
        public virtual string Move { get => _move; set => _move = value; } // player's choose


        /*abstract public bool IsCrit(float chance)
        {
            Random rnd = new Random();
            double result = rnd.NextDouble();
            if (result <= chance)
                return true;
            return false;
        }
        abstract public int Attack(Archetype attackerCharacter, Archetype defenderCharacter)
        {
            float attackerMultiplier, attackerCritChance;
            int attackerDamage, defenderArmor;
            string defenderMove;

            defenderMove = defenderCharacter._move;
            defenderArmor = defenderCharacter._defence;
            attackerDamage = attackerCharacter._damage;
            attackerMultiplier = attackerCharacter._multiplier;
            attackerCritChance = attackerCharacter._luck;

            if (defenderMove.Equals("attack") || defenderMove.Equals("stun"))
            {
                if (IsCrit(attackerCritChance))
                {
                    int damage;
                    damage = Convert.ToInt32(attackerDamage * attackerMultiplier);
                    return damage;
                }
                return attackerDamage - Convert.ToInt32(defenderArmor * .2f);
            }
            else if (defenderMove.Equals("parry"))
            {
                attackerCharacter._health -= Convert.ToInt32(attackerDamage * 0.6f);
                return 0;
            }
            else if (defenderMove.Equals("defence"))
            {
                int hit = Convert.ToInt32(attackerDamage * .2f) - Convert.ToInt32(defenderArmor * .3f);
                attackerCharacter._move = "stun";
                if (hit >= 0)
                    return hit;
                else
                    return 0;
            }
            else
                return -1;
        }*/
    }
}
