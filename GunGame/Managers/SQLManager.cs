using MySql.Data.MySqlClient;

namespace GunGame.Managers
{
    public static class SQLManager
    {
        public static MySqlConnection Connection;

        public static void Initialize()
        {
            Connection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", GunGameConfig.instance.sqlSettings.address, GunGameConfig.instance.sqlSettings.database, GunGameConfig.instance.sqlSettings.user, GunGameConfig.instance.sqlSettings.pass, GunGameConfig.instance.sqlSettings.port));

            MySqlCommand cmd = Connection.CreateCommand();

            cmd.CommandText = "show tables like 'gungame'";

            Connection.Open();

            if (cmd.ExecuteScalar() == null) {
                MySqlCommand cmd2 = Connection.CreateCommand();
                cmd2.CommandText = "CREATE TABLE `gungame` (`steamid` bigint NOT NULL UNIQUE,`kills` integer NOT NULL,`deaths` integer NOT NULL,`rounds` integer NOT NULL,`first` integer NOT NULL,`second` integer NOT NULL,`third` integer NOT NULL, PRIMARY KEY (`steamid`))";
                cmd2.ExecuteNonQuery();
            }

            Connection.Close();

        }

        public static PlayerQuery LoadPlayer(ulong steamId)
        {
            PlayerQuery query = new PlayerQuery();

            MySqlCommand cmd = Connection.CreateCommand();

            cmd.CommandText = string.Format("SELECT `kills`,`deaths`,`rounds`,`first`,`second`,`third` FROM `gungame` WHERE `steamid`='{0}'", steamId);

            Connection.Open();
            MySqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read()) {
                query.kills = (int)dr[0];
                query.deaths = (int)dr[1];
                query.rounds = (int)dr[2];
                query.first = (int)dr[3];
                query.second = (int)dr[4];
                query.third = (int)dr[5];
                query.isFirstQuery = false;
            } else {
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

            if (query.isFirstQuery) {
                cmd.CommandText = string.Format("INSERT INTO `gungame` VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", steamId, query.kills, query.deaths, query.rounds, query.first, query.second, query.third);
            } else {
                cmd.CommandText = string.Format("UPDATE `gungame` SET `kills`='{0}', `deaths`='{1}', `rounds`='{2}', `first`='{3}', `second`='{4}', `third`='{5}' WHERE `steamid`='{6}'", query.kills, query.deaths, query.rounds, query.first, query.second, query.third, steamId);
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