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
        protected int _defence; // health point
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

        public virtual int Shield(int damage, string enemyAction, Archetype enemy)
        {
            if (enemyAction.Equals("Attack"))
            {
                ulong p1id, p2id;

                Provider provider = new Provider();
                p1id = Convert.ToUInt64(provider.GetFieldAwonaByID("player1id", Convert.ToString(Id), "player1id", "duel"));
                p2id = Convert.ToUInt64(provider.GetFieldAwonaByID("player2id", Convert.ToString(enemy.Id), "player2id", "duel"));
                if (p1id == 0)
                {
                    p1id = Convert.ToUInt64(provider.GetFieldAwonaByID("player2id", Convert.ToString(Id), "player2id", "duel"));
                    p2id = Convert.ToUInt64(provider.GetFieldAwonaByID("player1id", Convert.ToString(enemy.Id), "player1id", "duel"));
                }

                int damagepoint = Convert.ToInt32(Math.Floor(damage - (damage * Protection) - (Defence / 25)));

                if (enemy.Id == p1id)
                    ExecuteSQL($"UPDATE duel SET player2health = {Health - damagepoint} WHERE player1id = {enemy.Id}");
                else if (enemy.Id == p2id)
                    ExecuteSQL($"UPDATE duel SET player1health = {Health - damagepoint} WHERE player2id = {enemy.Id}");
            }
            return 0;
        }

        public virtual int Parry(int damage, string enemyAction, Archetype enemy)
        {

            if (enemyAction.Equals("Attack"))
            {
                ulong p1id, p2id;

                Provider provider = new Provider();
                p1id = Convert.ToUInt64(provider.GetFieldAwonaByID("player1id", Convert.ToString(Id), "player1id", "duel"));
                p2id = Convert.ToUInt64(provider.GetFieldAwonaByID("player2id", Convert.ToString(enemy.Id), "player2id", "duel"));
                if (p1id == 0)
                {
                    p1id = Convert.ToUInt64(provider.GetFieldAwonaByID("player2id", Convert.ToString(Id), "player2id", "duel"));
                    p2id = Convert.ToUInt64(provider.GetFieldAwonaByID("player1id", Convert.ToString(enemy.Id), "player1id", "duel"));
                }

                int damagepoint = Convert.ToInt32(Math.Floor(damage * Protection + damage * Defence / 25));
                // Depends on what player is attacking, take % of enemy damage in
                if (enemy.Id == p1id)
                    ExecuteSQL($"UPDATE duel SET player2health = {Health - damagepoint} WHERE player1id = {enemy.Id}");
                else if (enemy.Id == p2id)
                    ExecuteSQL($"UPDATE duel SET player1health = {Health - damagepoint} WHERE player2id = {enemy.Id}");

                // But anyway return the damage that author would do to enemy
                return damage;
            }
            
            // If enemy action is not attack return 0 because parry works so
            return 0;
        }

        public virtual int Sleep()
        {
            return 0;
        }

        public virtual int Action(string thisAction, string enemyAction, Archetype enemy)
        {
            int damage, res;
            damage = Damage;
            Console.WriteLine($"Look at {thisAction}");
            if (thisAction.Equals("Defend"))
                res = Shield(damage, enemyAction, enemy);
            else if (thisAction.Equals("Parry"))
                res = Parry(damage, enemyAction, enemy);
            else if (thisAction.Equals("Attack"))
                res = Attack(enemy);
            else
                res = Sleep();

            return res;
        }
    }
}
