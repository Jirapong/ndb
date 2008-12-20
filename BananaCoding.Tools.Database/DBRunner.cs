using System;
using System.Collections.Generic;
using System.Text;
using BananaCoding.CommandLineParser;
using System.Data.SqlClient;

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
                    SqlDBHelper.Migrate(task.Environment);
                }
                else if (task.Membership)
                {
                    SqlDBHelper.InstallMembership(task.Environment);
                }
                else if (task.Version)
                {
                    SqlDBHelper.ViewVersion(task.Environment);
                }
                else if (task.Reset)
                {
                    SqlDBHelper.DeleteDB(task.Environment);
                    SqlDBHelper.CreateDB(task.Environment);
                    SqlDBHelper.Migrate(task.Environment);
                }
                else
                    printUsage(parser);
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
                
                Console.WriteLine(errorSb);
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

                Console.WriteLine(errorSb);
            }
            catch (Exception generalEx)
            {
                StringBuilder errorSb = new StringBuilder();

                errorSb.AppendLine(generalEx.GetType().ToString() + generalEx.Message);
                errorSb.AppendLine(task.Trace ? generalEx.StackTrace : "(See full trace by running ndb.exe with /trace)");

                Console.WriteLine(errorSb);
            }
            // For error handling, were any switches handled?
            string[] unhandled = parser.UnhandledSwitches;
            if (unhandled != null && unhandled.Length > 0)
            {
                Console.WriteLine("\nThe following switches were not handled.");
                foreach (string s in unhandled)
                    Console.WriteLine("  - {0}", s);
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
                    Console.WriteLine("Command : {0} - [{1}]", s.Name, s.Description);
                    Console.Write("Type    : {0} ", s.Type);

                    if (s.IsEnum)
                    {
                        Console.Write("- Enums allowed (");
                        foreach (string e in s.Enumerations)
                            Console.Write("{0} ", e);
                        Console.Write(")");
                    }
                    Console.WriteLine();

                    if (s.Aliases != null)
                    {
                        Console.Write("Aliases : [{0}] - ", s.Aliases.Length);
                        foreach (string alias in s.Aliases)
                            Console.Write(" {0}", alias);
                        Console.WriteLine();
                    }

                    Console.WriteLine("------> Value is : {0} (Without any callbacks {1})\n",
                        s.Value != null ? s.Value : "(Unknown)",
                        s.InternalValue != null ? s.InternalValue : "(Unknown)");
                }
            }
            else
                Console.WriteLine("There are no registered switches.");
        }
    }
}
