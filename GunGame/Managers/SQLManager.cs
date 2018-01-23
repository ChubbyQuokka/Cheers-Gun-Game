using System;

using MySql.Data.MySqlClient;

using Rocket.Core.Logging;

namespace GunGame.Managers
{
    public static class SQLManager
    {
        public static class Constants
        {
            public const string CONNECTION = "SERVER={0}; DATABASE={1}; UID={2};PASSWORD={3};PORT={4};";
            public const string SHOW_TABLES = "SHOW TABLES LIKE {0};";
            public const string CREATE_TABLE = "CREATE TABLE `{0}` (`steamid` bigint NOT NULL UNIQUE,`kills` integer NOT NULL,`deaths` integer NOT NULL,`rounds` integer NOT NULL,`first` integer NOT NULL,`second` integer NOT NULL,`third` integer NOT NULL, PRIMARY KEY (`steamid`));";
            public const string SELECT = "SELECT `kills`,`deaths`,`rounds`,`first`,`second`,`third` FROM `{0}` WHERE `steamid`='{1}'";
            public const string INSERT = "INSERT INTO `{0}` VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}')";
            public const string UPDATE = "UPDATE `{0}` SET `kills`='{1}', `deaths`='{2}', `rounds`='{3}', `first`='{4}', `second`='{5}', `third`='{6}' WHERE `steamid`='{7}'";
        }

        public static MySqlConnection Connection;

        static GunGameConfig.MySqlSettings settings;

        public static bool Initialize()
        {
            try
            {
                settings = GunGameConfig.instance.sqlSettings;
                
                Connection = new MySqlConnection(string.Format(Constants.CONNECTION, settings.address, settings.database, settings.user, settings.pass, settings.port));

                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = string.Format(Constants.SHOW_TABLES, settings.table);

                Connection.Open();

                if (cmd.ExecuteScalar() == null)
                {
                    MySqlCommand cmd2 = Connection.CreateCommand();
                    cmd2.CommandText = string.Format(Constants.CREATE_TABLE, settings.table);
                    cmd2.ExecuteNonQuery();
                }

                Connection.Close();
                return true;
            }
            catch (Exception e)
            {
                Logger.Log(e);
                return false;
            }
        }

        public static PlayerQuery LoadPlayer(ulong steamId)
        {
            PlayerQuery query = new PlayerQuery();

            MySqlCommand cmd = Connection.CreateCommand();
            
            cmd.CommandText = string.Format(Constants.SELECT, settings.table, steamId);
            Connection.Open();
            MySqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                query.kills = (int)dr[0];
                query.deaths = (int)dr[1];
                query.rounds = (int)dr[2];
                query.first = (int)dr[3];
                query.second = (int)dr[4];
                query.third = (int)dr[5];
                query.isFirstQuery = false;
            }
            else
            {
                query.kills = 0;
                query.deaths = 0;
                query.rounds = 0;
                query.first = 0;
                query.second = 0;
                query.third = 0;
                query.isFirstQuery = true;
            }

            Connection.Close();

            return query;
        }

        public static void SavePlayer(ulong steamId, PlayerQuery query)
        {
            MySqlCommand cmd = Connection.CreateCommand();

            if (query.isFirstQuery)
            {
                cmd.CommandText = string.Format(Constants.INSERT, settings.table, steamId, query.kills, query.deaths, query.rounds, query.first, query.second, query.third);
            }
            else
            {
                cmd.CommandText = string.Format(Constants.UPDATE, settings.table, query.kills, query.deaths, query.rounds, query.first, query.second, query.third, steamId);
            }

            Connection.Open();
            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        public struct PlayerQuery
        {
            public int kills;
            public int deaths;
            public int rounds;
            public int first;
            public int second;
            public int third;
            public bool isFirstQuery;
        }
    }
}