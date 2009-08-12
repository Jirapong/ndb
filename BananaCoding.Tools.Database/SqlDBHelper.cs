using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using System.Web.Management;
using System.Diagnostics;
using BananaCoding.Tools.Database.Utility;

namespace BananaCoding.Tools.Database {
    public class SqlDBHelper {
        private static string GetDatabaseName(DBEnvironments environment) {
            string dbName;
            switch (environment) {
                case DBEnvironments.Development:
                    dbName = Properties.Settings.Default.Database;
                    break;
                case DBEnvironments.Production:
                    dbName = Properties.Settings.Default.Production_Database;
                    break;
                default:
                    dbName = Properties.Settings.Default.Database;
                    break;
            }

            if (string.IsNullOrEmpty(dbName)) {
                throw new ArgumentException("You must supply the file name of a database", dbName);
            }

            return dbName;
        }

        public static void CreateDB(DBEnvironments environment) {
            string dbName = GetDatabaseName(environment);

            if (IsExists(environment, dbName)) {
                Console.WriteLine("{0} already exists", dbName);
                return;
            }

            // Create Connection String from Configuration File
            string sqlConStr = BuildConnectionString(environment, "master");
            Console.WriteLine("in {0}", sqlConStr);

            // CREATE DATABASE in context of master db
            string sqlQuery = string.Format("CREATE DATABASE {0}; \r\n GO", dbName);
            SqlScriptHelper.ExecuteScript(sqlQuery, sqlConStr);

            // Create schema_version table
            sqlConStr = BuildConnectionString(environment, dbName);
            Console.WriteLine("in {0}", sqlConStr);
            SqlScriptHelper.ExecuteScriptFromEmbeddedResource("BananaCoding.Tools.Database.DBScripts.create_schema.sql", sqlConStr);
        }

        public static void DeleteDB(DBEnvironments environment) {
            string dbName = GetDatabaseName(environment);

            if (!IsExists(environment, dbName)) {
                Console.WriteLine("{0} not found", dbName);
                return;
            }

            // Create Connection String from Configuration File
            string sqlConStr = BuildConnectionString(environment, "master");
            Console.WriteLine("in {0}", sqlConStr);

            // Uninstall ASP.NET Membership table
            // No need because we can delete just the database
            // SqlServices.Uninstall(dbName, SqlFeatures.All, sqlConStr);

            // Single user to cut current connections.
            string sqlQuery = string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; \r\n GO", dbName);
            SqlScriptHelper.ExecuteScript(sqlQuery, sqlConStr);

            // DROP DATABASE in context of master db
            sqlQuery = string.Format("DROP DATABASE {0}; \r\n GO", dbName);
            SqlScriptHelper.ExecuteScript(sqlQuery, sqlConStr);

        }

        internal static bool IsExists(DBEnvironments environment, string dbName) {
            if (string.IsNullOrEmpty(dbName)) {
                throw new ArgumentException("You must supply the file name of a database", dbName);
            }

            // Create Connection String from Configuration File
            string sqlConStr = BuildConnectionString(environment, "master");

            using (SqlConnection connection = new SqlConnection(sqlConStr)) {
                string sqlQuery = string.Format("SELECT name FROM sys.databases WHERE name = '{0}'", dbName);

                SqlCommand cmd = new SqlCommand(sqlQuery, connection);

                connection.Open();
                object result = cmd.ExecuteScalar();

                return (result != null && !Convert.IsDBNull(result));
            }
        }

        internal static void InstallMembership(DBEnvironments environment) {
            string dbName = GetDatabaseName(environment);

            if (!IsExists(environment, dbName)) {
                Console.WriteLine("{0} not found", dbName);
                return;
            }

            // Create Connection String from Configuration File
            string sqlConStr = BuildConnectionString(environment, dbName);

            // Install ASP.NET membership tables
            SqlServices.Install(dbName, SqlFeatures.All, sqlConStr);

            var versionName = "000_ASPNET_Membership";
            // Update Schema version
            UpdateVersionSchema(sqlConStr, versionName);

        }

        internal static void Migrate(Task task) {
            DBEnvironments environment = task.Environment;
            int migrateTo = task.MigrateTo;
            bool noASPNET = task.NoMembership;

            string dbName = GetDatabaseName(environment);

            if (!IsExists(environment, dbName)) {
                Console.WriteLine("{0} not found", dbName);
                return;
            }

            // Create Connection String from Configuration File
            string sqlConStr = BuildConnectionString(environment, dbName);
            Console.WriteLine("in {0}", sqlConStr);

            // Update Schema version
            int? latestVersion = GetLatestSchemaVersion(sqlConStr);

            // Migrate 000 first
            if (!latestVersion.HasValue && !noASPNET) InstallMembership(environment);

            // Get all migration files
            var sw = new Stopwatch();
            string[] files = Directory.GetFiles(@"migrate", "*.sql");
            Array.Sort<string>(files);
            foreach (var sqlscript in files) {
                // ignore vim backup file
                if (sqlscript.EndsWith("~")) continue;

                string versionName = Path.GetFileNameWithoutExtension(sqlscript);
                int versionNumber = GetVersionNumber(versionName);
                if (latestVersion.HasValue && versionNumber <= latestVersion) continue;
                if (migrateTo != 0 && versionNumber > migrateTo) break;

                using (TransactionScope scope = new TransactionScope()) {
                    Console.WriteLine(@"Run script ------ {0} --------------------------", versionName);

                    sw.Reset();
                    sw.Start();

                    // Run Sql from file
                    SqlScriptHelper.ExecuteScriptFile(sqlscript, sqlConStr);

                    // Update Schema version
                    UpdateVersionSchema(sqlConStr, versionName);

                    sw.Stop();
                    Console.WriteLine(@"Run script ------ {0} ({1}ms) ------ successfully.", versionName, sw.ElapsedMilliseconds);

                    scope.Complete();
                }
            }
        }

        internal static void ViewVersion(DBEnvironments environment) {
            string dbName = GetDatabaseName(environment);

            if (!IsExists(environment, dbName)) {
                Console.WriteLine("{0} not found", dbName);
                return;
            }

            // Create Connection String from Configuration File
            string sqlConStr = BuildConnectionString(environment, dbName);
            Console.WriteLine("in {0}", sqlConStr);

            string versionName = GetLatestSchemaVersionName(sqlConStr);
            if (!string.IsNullOrEmpty(versionName))
                Console.WriteLine("Current Version: {0}", versionName);
            else
                Console.WriteLine("Database is empty");
        }

        private static string GetLatestSchemaVersionName(string sqlConStr) {
            using (SqlConnection conn = new SqlConnection(sqlConStr)) {
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 version FROM schema_version ORDER BY version DESC", conn);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (!Convert.IsDBNull(result) && result != null)
                    return result.ToString();
            }
            return string.Empty;
        }

        private static int? GetLatestSchemaVersion(string sqlConStr) {
            string versionName = GetLatestSchemaVersionName(sqlConStr);

            if (string.IsNullOrEmpty(versionName))
                return null;

            return GetVersionNumber(versionName);
        }

        private static void UpdateVersionSchema(string sqlConStr, string versionName) {
            using (SqlConnection conn = new SqlConnection(sqlConStr)) {
                SqlCommand cmd = new SqlCommand("INSERT INTO schema_version(version) VALUES(@Version);", conn);

                cmd.Parameters.AddWithValue("@Version", versionName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static int GetVersionNumber(string fullname) {
            int version = 0;

            if (int.TryParse(fullname.Split('_')[0], out version))
                return version;
            else
                throw new InvalidOperationException(string.Format("Cannot read version from a given name '{0}'", fullname));
        }

        private static string BuildConnectionString(DBEnvironments environment, string dbName) {
            SqlConnectionStringBuilder sqlConBuilder = new SqlConnectionStringBuilder();
            sqlConBuilder.DataSource = (environment == DBEnvironments.Development) ? Properties.Settings.Default.Server : Properties.Settings.Default.Production_Server;
            sqlConBuilder.InitialCatalog = dbName;
            sqlConBuilder.Enlist = false;
            sqlConBuilder.ConnectTimeout = 900;
            if (string.IsNullOrEmpty(Properties.Settings.Default.UserName)) {
                sqlConBuilder.IntegratedSecurity = true;
            } else {
                sqlConBuilder.IntegratedSecurity = false;
                sqlConBuilder.UserID = (environment == DBEnvironments.Development) ? Properties.Settings.Default.UserName : Properties.Settings.Default.Production_UserName;
                sqlConBuilder.Password = (environment == DBEnvironments.Development) ? Properties.Settings.Default.Password : Properties.Settings.Default.Production_Password;
            }
            return sqlConBuilder.ConnectionString;
        }


    }
}
