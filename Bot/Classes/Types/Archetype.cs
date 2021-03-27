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
        protected int _defence; // health point
        protected int _damage; // damage parameter
        protected float _luck; // luck parameter
        protected float _multiplier; // luck multiplier
        protected float _protection;
        protected float _dodge; // dodge chance
        protected string _move; // player's choose

        public virtual string Name { get => _name; set => _name = value; } // Username#0000
        public virtual ulong Id { get; set; } // discord id 
        public virtual string Race { get => _race; set => _race = value; } // current race 
        public virtual int Health { get => _health; set => _health = value; } // health point
        public virtual int Lvl { get => _lvl; set => _lvl = value; }
        public virtual int Defence { get => _defence; set => _defence = value; } // health point
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
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = cmd;
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

        public virtual int Attack(Archetype enemy)
        {
            int damage, enemyDefence;
            float critChance;

            Random rnd = new Random();
            double rand = rnd.NextDouble();

            damage = Damage;
            enemyDefence = enemy.Defence;
            critChance = Luck;

            if (critChance > rand)
                damage *= Convert.ToInt32(Math.Floor(Multiplier));

            int result = damage - Convert.ToInt32(Math.Floor(enemyDefence * .15f));
            return result;
        }

        public virtual int Shield(int damage, Archetype enemy)
        {
            float enemyProtection = enemy.Protection;
            ulong p1id, p2id;

            p1id = Convert.ToUInt64(GetFieldSQL(Id, "id", "duel"));
            p2id = Convert.ToUInt64(GetFieldSQL(enemy.Id, "id", "duel"));
            int attack = Convert.ToInt32(Math.Floor(damage - damage * enemyProtection));

            if (enemy.Id == p1id)
                ExecuteSQL($"UPDATE duel SET player2health = {Health - attack}");
            else if (enemy.Id == p2id)
                ExecuteSQL($"UPDATE duel SET player1health = {Health - attack}");

            return 0;
        }

        public virtual int Parry(int damage, Archetype enemy)
        {
            string enemyAction;
            int playerHealth, attack;
            float critChance;
            ulong p1id, p2id;
            Random rnd = new Random();
            double rand = rnd.NextDouble();

            critChance = Luck / 1.27f;

            playerHealth = Health;
            playerHealth -= Convert.ToInt32(Math.Floor(damage * 0.65));

            attack = damage;

            if (critChance > rand)
                attack *= Convert.ToInt32(Math.Floor(Multiplier));

            p1id = Convert.ToUInt64(GetFieldSQL(Id, "id", "duel"));
            p2id = Convert.ToUInt64(GetFieldSQL(enemy.Id, "id", "duel"));

            // !!! IMPORTANT !!!
            enemyAction = "attack";
            if (enemyAction.Equals("attack"))
            {
                int enemyHealth = enemy.Health - attack;
                // Depends on what player is attacking, take % of enemy damage in
                if (enemy.Id == p1id)
                    ExecuteSQL($"UPDATE duel SET player2health = {enemyHealth}");
                else if (enemy.Id == p2id)
                    ExecuteSQL($"UPDATE duel SET player1health = {enemyHealth}");
                // But anyway return the damage that author would do to enemy
                return attack;
            }
            // If enemy action is not attack return 0 because parry works so
            return 0;
        }

        public virtual int Sleep()
        {
            return 0;
        }

        public virtual int Action(string action, Archetype enemy)
        {
            int damage, res;
            damage = Damage;
            if (action.Equals("Defense"))
                res = Shield(damage, enemy);
            else if (action.Equals("Parry"))
                res = Parry(damage, enemy);
            else if (action.Equals("Attack"))
                res = Attack(enemy);
            else
                res = Sleep();

            return res;
        }
    }
}
