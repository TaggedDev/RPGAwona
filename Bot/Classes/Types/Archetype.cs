using Bot.Modules;
using Microsoft.Data.Sqlite;
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
        protected int _armor; // health point
        protected int _damage; // damage parameter
        protected float _luck; // luck parameter
        protected float _multiplier; // luck multiplier
        protected float _protection;
        protected float _dodge; // dodge chance
        protected string _move; // player's choose

        public virtual string Name { get => _name; set => _name = value; } // Username#0000
        public virtual ulong Id { get => _id; set => _id = value; } // discord id 
        public virtual string Race { get => _race; set => _race = value; } // current race 
        public virtual int Health { get => _health; set => _health = value; } // health point
        public virtual int Lvl { get => _lvl; set => _lvl = value; }
        public virtual int Armor { get => _armor; set => _armor = value; } // health point
        public virtual int Damage { get => _damage; set => _damage = value; } // damage parameter
        public virtual float Luck { get => _luck; set => _luck = value; } // luck parameter
        public virtual float Multiplier { get => _multiplier; set => _multiplier = value; } // luck multiplier
        public virtual float Protection { get => _protection; set => _protection = value; } // luck multiplier
        public virtual float Dodge { get => _dodge; set => _dodge = value; } // dodge chance
        public virtual string Move { get => _move; set => _move = value; } // player's choose

        public void ExecuteSQL(string cmd)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand
                {
                    Connection = connection,
                    CommandText = cmd
                };
                command.ExecuteNonQuery();
            }
        }

        public object GetFieldSQL(ulong id, string field, string table)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = $"SELECT * FROM {table}";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader.GetValue(0));
                            if (getId.Equals(Convert.ToString(id)))
                                return reader[field];
                        }
                }
            }
            return null;
        }

        public virtual int Attack(string enemyAction, Archetype enemy)
        {
            int result;

            Random rnd = new Random();
            float rand = Convert.ToSingle(rnd.NextDouble());

            if (enemyAction.Equals("Attack"))
                return -2;
            if (enemyAction.Equals("Defend"))
                return -1;

            if (enemyAction.Equals("Ability") || enemyAction.Equals("Sleep"))
            { 
                if (rand > enemy.Dodge)
                {
                    rand = Convert.ToSingle(rnd.NextDouble());

                    if (rand > Luck)
                        result = Convert.ToInt32(Damage + Damage * Multiplier);
                    else
                        result = Damage;

                    result = Convert.ToInt32(result/enemy.Protection - enemy.Armor);
                    if (result < 0) return -3;

                    return result;

                }
                else
                    return -4;
            }
            return -6;
           
            
        }

        public virtual int Shield(string enemyAction, Archetype enemy)
        {
            int result;

            Random rnd = new Random();
            float rand = Convert.ToSingle(rnd.NextDouble());

            if (enemyAction.Equals("Defend"))
                return -2;
            if (enemyAction.Equals("Ability"))
                return -1;

            if (enemyAction.Equals("Attack") || enemyAction.Equals("Sleep"))
            {
                if (rand > enemy.Dodge)
                {
                    rand = Convert.ToSingle(rnd.NextDouble());

                    if (rand > Luck)
                        result = Convert.ToInt32(Damage + Damage * Multiplier);
                    else
                        result = Damage;

                    result = Convert.ToInt32(result / enemy.Protection - enemy.Armor);
                    if (result < 0) return -3;

                    return result;

                }
                else
                    return -4;
            }
            return -6;
        }

        public virtual int Ability(string enemyAction, Archetype enemy)
        {
            int result;

            Random rnd = new Random();
            float rand = Convert.ToSingle(rnd.NextDouble());

            if (enemyAction.Equals("Ability"))
                return -2;
            if (enemyAction.Equals("Attack"))
                return -1;

            if (enemyAction.Equals("Defend") || enemyAction.Equals("Sleep"))
            {
                if (rand > enemy.Dodge)
                {
                    rand = Convert.ToSingle(rnd.NextDouble());

                    if (rand > Luck)
                        result = Convert.ToInt32(Damage + Damage * Multiplier);
                    else
                        result = Damage;

                    result = Convert.ToInt32(result / enemy.Protection - enemy.Armor);
                    if (result < 0) return -3;

                    return result;

                }
                else
                    return -4;
            }
            return -5;
        }

        public virtual int Sleep()
        {
            return -6;
        }

        public virtual int Action(string thisAction, string enemyAction, Archetype enemy)
        {
            int res;
            if (thisAction.Equals("Defend"))
                res = Shield(enemyAction, enemy);
            else if (thisAction.Equals("Ability"))
                res = Ability(enemyAction, enemy);
            else if (thisAction.Equals("Attack"))
                res = Attack(enemyAction, enemy);
            else
                res = Sleep();

            if (res < 0)
                return 0;
            return res;
        }
    }
}
