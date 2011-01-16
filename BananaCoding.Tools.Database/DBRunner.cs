using System;
using System.Collections.Generic;
using System.Text;
using BananaCoding.CommandLineParser;
using System.Data.SqlClient;
using System.IO;

namespace BananaCoding.Tools.Database
{
    public class DBRunner
    {
        public DBRunner()
        {

        }

        internal void Run(string[] args)
        {
            Task task = new Task();

            Parser parser = new Parser(System.Environment.CommandLine, task);

            // Add a switches with lots of aliases for the first name, "help" and "a".
            parser.AddSwitch(new string[] { "help", @"\?" }, "show help");

            // Parse the command line.
            parser.Parse();

            try
            {
                if (!string.IsNullOrEmpty(task.Xml))
                {
                    SqlDBHelper.Logger = new StreamWriter(task.Xml);
                    SqlDBHelper.WriteXmlHeader();
                }

                if (task.CreateDB)
                {
                    SqlDBHelper.CreateDB(task.Environment);
                }
                else if (task.ClearDB)
                {
                    SqlDBHelper.DeleteDB(task.Environment);
                }
                else if (task.Migrate)
                {
                    SqlDBHelper.Migrate(task);
                }
                else if (task.Version)
                {
                    SqlDBHelper.ViewVersion(task.Environment);
                }
                else if (task.Reset)
                {
                    SqlDBHelper.DeleteDB(task.Environment);
                    SqlDBHelper.CreateDB(task.Environment);
                    SqlDBHelper.Migrate(task);
                }
                else if (task.Fixture)
                {
                    SqlDBHelper.LoadFixtures(task);
                }
                else if (!string.IsNullOrEmpty(task.GrantPermission))
                {
                    SqlDBHelper.GrantPermission(task.Environment, task.GrantPermission);
                }
                else
                    printUsage(parser);
            }
            catch (System.UnauthorizedAccessException uaex)
            {
                StringBuilder errorSb = new StringBuilder();
                errorSb.AppendLine(uaex.Message);
                errorSb.AppendLine(task.Trace ? uaex.StackTrace : "(See full trace by running ndb.exe with /trace)");
                errorSb.AppendLine("Try: attrib.exe -R migrate");
                errorSb.AppendLine("Try: attrib.exe -R fixtures");

                SqlDBHelper.MessageOut(errorSb.ToString());
            }
            catch (Microsoft.SqlServer.Management.Common.ExecutionFailureException smoEx)
            {
                StringBuilder errorSb = new StringBuilder();
                if (smoEx.InnerException != null)
                {
                    errorSb.AppendLine(smoEx.InnerException.HelpLink + smoEx.InnerException.Message);
                    errorSb.AppendLine(task.Trace ? smoEx.InnerException.StackTrace : "(See full trace by running ndb.exe with /trace)");
                }
                else
                {
                    errorSb.AppendLine(smoEx.Message);
                    errorSb.AppendLine(task.Trace ? smoEx.StackTrace : "(See full trace by running ndb.exe with /trace)");
                }

                SqlDBHelper.MessageOut(errorSb.ToString());
            }
            catch (SqlException dbEx)
            {
                StringBuilder errorSb = new StringBuilder();

                errorSb.AppendLine(dbEx.Message);

                if (task.Trace)
                {
                    foreach (SqlError err in dbEx.Errors)
                    {
                        errorSb.AppendLine(err.ToString());
                    }
                }
                else
                    errorSb.AppendLine("(See full trace by running ndb.exe with /trace)");

                SqlDBHelper.MessageOut(errorSb.ToString());
            }
            catch (Exception generalEx)
            {
                StringBuilder errorSb = new StringBuilder();

                errorSb.AppendLine(generalEx.GetType().ToString() + generalEx.Message);
                errorSb.AppendLine(task.Trace ? generalEx.StackTrace : "(See full trace by running ndb.exe with /trace)");

                SqlDBHelper.MessageOut(errorSb.ToString());
            }
            finally
            {
                if (!string.IsNullOrEmpty(task.Xml))
                {
                    SqlDBHelper.WriteXmlFooter();
                    SqlDBHelper.Logger.Flush();
                    SqlDBHelper.Logger.Close();
                }
            }

            // For error handling, were any switches handled?
            string[] unhandled = parser.UnhandledSwitches;
            if (unhandled != null && unhandled.Length > 0)
            {
                SqlDBHelper.MessageOut("\nThe following switches were not handled.");
                foreach (string s in unhandled)
                    SqlDBHelper.MessageOut(string.Format("  - {0}", s));
            }
        }

        private void printUsage(Parser parser)
        {
            Parser.SwitchInfo[] si = parser.Switches;
            if (si != null)
            {
                Console.WriteLine("There are {0} registered switches:", si.Length);
                foreach (Parser.SwitchInfo s in si)
                {
                    StringBuilder sb = new StringBuilder();
                    if (s.IsEnum)
                    {
                        sb.Append(":{");
                        sb.Append(string.Join(", ", s.Enumerations));
                        sb.Append("}");
                    }

                    Console.WriteLine("ndb.exe /{0}{3} or /{1} [{2:60}]", s.Name, s.Aliases[0], s.Description, sb);
                }
            }
            else
                Console.WriteLine("There are no registered switches. run ndb.exe to view all tasks.");
        }
    }
}
