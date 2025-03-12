namespace MySql
{
    public static class MySqlStatement
    {
        public const string connectionString = "server=localhost;user=root;password=Karyl0902;database=users_db;";
        public const string createUserInfoTable = @"CREATE TABLE IF NOT EXISTS user_info(
            id int NOT NULL PRIMARY KEY AUTO_INCREMENT COMMENT 'Primary Key',
            create_time DATETIME,
            username VARCHAR(255),
            password VARCHAR(255),
            avatar_base64 MEDIUMTEXT
        ) COMMENT ''";
    }
}