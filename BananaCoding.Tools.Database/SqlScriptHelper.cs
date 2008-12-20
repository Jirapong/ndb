using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

///
/// History: received copy from Brian Noyes
/// 1) 09/14/2008 - Modify to support SQLite (JN)
/// 2) 12/17/2008 - Implement SQL Sdk to execute the script file to support multiple GO statement
///

namespace BananaCoding.Tools.Database
{
    public class SqlScriptHelper
    {
        public static void ExecuteScript(string script, string connectionString)
        {
            StringReader reader = new StringReader(script);
            ExecuteScriptStatements(reader, connectionString);
        }

        public static void ExecuteScriptFile(string scriptFileName, string connectionString)
        {
            if (scriptFileName == null || scriptFileName == string.Empty)
            {
                throw new ArgumentException("You must supply the file name of a file containing SQL Script", scriptFileName);
            }
            FileStream scriptStream = new FileStream(scriptFileName, FileMode.Open);
            ExecuteScriptStream(scriptStream, connectionString);
        }

        public static void ExecuteScriptFromEmbeddedResource(string embeddedResourceName, string connectionString)
        {
            if (embeddedResourceName == null || embeddedResourceName == string.Empty)
            {
                throw new ArgumentException("embeddedResourceName must be a fully qualified name of an embedded resource file containing SQL Script", embeddedResourceName);
            }
            Assembly assem = Assembly.GetCallingAssembly();
            Stream scriptStream = assem.GetManifestResourceStream(embeddedResourceName);
            if (scriptStream == null)
            {
                throw new ArgumentException(string.Format("No embedded resource named {0} found.", embeddedResourceName), embeddedResourceName);
            }
            ExecuteScriptStream(scriptStream, connectionString);
        }

        private static void ExecuteScriptStream(Stream scriptStream, string connectionString)
        {
            if (scriptStream != null)
            {
                StreamReader scriptReader = new StreamReader(scriptStream);
                ExecuteScriptStatements(scriptReader, connectionString);
            }
        }

        private static void ExecuteScriptStatements(TextReader scriptReader, string connectionString)
        {
            string script = scriptReader.ReadToEnd();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                Server server = new Server(new ServerConnection(conn));
                server.ConnectionContext.ExecuteNonQuery(script);
                //server.ConnectionContext.InfoMessage += new SqlInfoMessageEventHandler(ConnectionContext_InfoMessage);
            }
        }

        //static void ConnectionContext_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        //{
        //    Console.WriteLine(e.Message);
        //}

        private static void ExecuteScriptStatementsByLine(TextReader scriptReader, string connectionString)
        {
            string scriptLine = string.Empty;
            StringBuilder scriptBuffer = new StringBuilder();
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                while ((scriptLine = scriptReader.ReadLine()) != null)
                {
                    if (scriptLine.ToLower().Trim() == "go")
                    {
                        ExecuteScriptBlock(scriptBuffer.ToString(), connection);
                        scriptBuffer = new StringBuilder();
                    }
                    else
                    {
                        scriptBuffer.Append(scriptLine.Trim());
                    }
                }

            }
            finally
            {
                connection.Close();
            }
        }

        private static void ExecuteScriptBlock(string scriptBlock, SqlConnection conn)
        {
            if (scriptBlock != null && scriptBlock != string.Empty)
            {
                SqlCommand cmd = new SqlCommand(scriptBlock, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
