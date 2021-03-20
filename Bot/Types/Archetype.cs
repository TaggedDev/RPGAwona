using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Types
{
    abstract class Archetype
    {
        protected string _name { get; set; } // Username#0000
        protected int _id { get; set;  } // discord id 
        protected string _race { get; } // current race 
        protected int _health { get; set; } // health point
        protected int _lvl { get; set; }
        protected int _defence { get; set; } // health point
        protected int _damage { get; set; } // damage parameter
        protected float _luck { get; set; } // luck parameter
        protected float _multiplier { get; set; } // luck multiplier
        protected float _dodge { get; set; } // dodge chance
        protected string _move { get; set; } // player's choose

        public bool IsCrit(float chance)
        {
            Random rnd = new Random();
            double result = rnd.NextDouble();
            if (result <= chance)
                return true;
            return false;
        }
        public int Attack(Archetype attackerCharacter, Archetype defenderCharacter)
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
        }
    }
}
