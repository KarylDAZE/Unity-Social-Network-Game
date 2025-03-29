namespace MySql
{
    public static class MySqlStatement
    {
        public const string connectionString = "server=localhost;user=root;password=Karyl0902;database=users_db;";
        public const string createUserInfoTable = @"CREATE TABLE IF NOT EXISTS user_info(
            id int NOT NULL PRIMARY KEY AUTO_INCREMENT COMMENT 'Primary Key',
            create_time DATETIME,
            username VARCHAR(255) NOT NULL,
            password VARCHAR(255) NOT NULL,
            avatar_base64 MEDIUMTEXT
        ) COMMENT ''";

        public const string createFriendTable = @"CREATE TABLE IF NOT EXISTS friend(
            id INT NOT NULL PRIMARY KEY AUTO_INCREMENT COMMENT 'Primary Key',
            user_id1 INT NOT NULL COMMENT '发起好友请求的用户ID',
            user_id2 INT NOT NULL COMMENT '被请求的用户ID',
            status TINYINT NOT NULL DEFAULT 0 COMMENT '好友请求状态，0-待确认，1-已成为好友',
            create_time DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '记录创建时间',
            UNIQUE KEY unique_friend (user_id1, user_id2)
        ) COMMENT '好友关系表'";

        public const string createChatTable = @"CREATE TABLE IF NOT EXISTS chat(
            id INT NOT NULL PRIMARY KEY AUTO_INCREMENT COMMENT 'Primary Key',
            user_id1 INT NOT NULL COMMENT '发送者ID',
            user_id2 INT NOT NULL COMMENT '接收者ID',
            content TEXT NOT NULL COMMENT '消息内容',
            create_time DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '消息发送时间'
        ) COMMENT '聊天记录表'";

        public const string createScoreTable = @"CREATE TABLE IF NOT EXISTS score(
            id INT NOT NULL PRIMARY KEY AUTO_INCREMENT COMMENT 'Primary Key',
            user_id INT NOT NULL COMMENT '用户ID',
            score INT NOT NULL COMMENT '分数',
            create_time DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '记录创建时间'
        ) COMMENT '分数记录表'";
    }
}