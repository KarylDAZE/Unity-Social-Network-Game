using System.Data;
using MySql.Data.MySqlClient;

namespace MySql
{
    public static class MySqlHelper
    {
        private static MySqlConnection connection = new(MySqlStatement.connectionString);
        public static void Connect()
        {
            try
            {
                connection.Open();
                Console.WriteLine("MySQL数据库连接成功");
                // 示例查询语句
                MySqlCommand createUserInfoTableCommand = new(MySqlStatement.createUserInfoTable, connection);
                using MySqlDataReader reader = createUserInfoTableCommand.ExecuteReader();
                // Optionally read data from the reader if needed
            }
            catch (Exception ex)
            {
                Console.WriteLine("数据库出现异常: " + ex.Message);
            }
        }

        public static DataTable GetTable(string tableName)
        {
            MySqlCommand command = new($"SELECT * FROM {tableName}", connection);
            DataTable dataTable = new();
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                dataTable.Load(reader);
            }
            return dataTable;
        }

        public static DataRow? GetRow(string tableName, int id)
        {
            DataTable dataTable = new();
            MySqlCommand command = new($"SELECT * FROM {tableName} WHERE id = @id", connection);
            command.Parameters.Add(new MySqlParameter("@id", id));
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                dataTable.Load(reader);
            }
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }

        public static void InsertRow(string tableName, Dictionary<string, object> values)
        {
            string columns = string.Join(", ", values.Keys);
            string valuesString = string.Join(", ", values.Keys.Select(v => $"@{v}"));
            MySqlCommand command = new($"INSERT INTO {tableName} ({columns}) VALUES ({valuesString})", connection);
            foreach (var (key, value) in values)
            {
                command.Parameters.Add(new MySqlParameter($"@{key}", value));
            }
            command.ExecuteNonQuery();
        }
    }
}