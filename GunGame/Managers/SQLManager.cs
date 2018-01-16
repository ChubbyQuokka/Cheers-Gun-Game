using System;

using MySql.Data.MySqlClient;

using Rocket.Core.Logging;

namespace GunGame.Managers
{
    public static class SQLManager
    {
        public static MySqlConnection Connection;

        static GunGameConfig.MySqlSettings settings;

        public static bool Initialize()
        {
            try
            {
                settings = GunGameConfig.instance.sqlSettings;

                Connection = new MySqlConnection($"SERVER={settings.address};DATABASE={settings.database};UID={settings.user};PASSWORD={settings.pass};PORT={settings.port};");

                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = $"show tables like '{settings.table}'";

                Connection.Open();

                if (cmd.ExecuteScalar() == null)
                {
                    MySqlCommand cmd2 = Connection.CreateCommand();
                    cmd2.CommandText = $"CREATE TABLE `{settings.table}` (`steamid` bigint NOT NULL UNIQUE,`kills` integer NOT NULL,`deaths` integer NOT NULL,`rounds` integer NOT NULL,`first` integer NOT NULL,`second` integer NOT NULL,`third` integer NOT NULL, PRIMARY KEY (`steamid`))";
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

            cmd.CommandText = $"SELECT `kills`,`deaths`,`rounds`,`first`,`second`,`third` FROM `{settings.table}` WHERE `steamid`='{steamId}'";
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
                cmd.CommandText = $"INSERT INTO `{settings.table}` VALUES ('{steamId}','{query.kills}','{query.deaths}','{query.rounds}','{query.first}','{query.second}','{query.third}')";
            }
            else
            {
                cmd.CommandText = $"UPDATE `{settings.table}` SET `kills`='{query.kills}', `deaths`='{query.deaths}', `rounds`='{query.rounds}', `first`='{query.first}', `second`='{query.second}', `third`='{query.third}' WHERE `steamid`='{steamId}'";
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