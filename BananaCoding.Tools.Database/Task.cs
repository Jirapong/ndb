using System;
using System.Collections.Generic;
using System.Text;
using BananaCoding.CommandLineParser;

namespace BananaCoding.Tools.Database
{
    /// <summary>
    /// Commandline tasks
    /// </summary>
    internal class Task
    {
        public MigrationKind Migration { get; set; }

        [CommandLineSwitch("reset", "Clear and recreates the database for the current /environment")]
        [CommandLineAlias("r")]
        public bool Reset { get; set; }

        [CommandLineSwitch("version", "Retrieves the current schema version number")]
        [CommandLineAlias("v")]
        public bool Version { get; set; }

        [CommandLineSwitch("clear", "Clears all the local databases defined in ndb.exe.config")]
        [CommandLineAlias("cl")]
        public bool ClearDB { get; set; }

        [CommandLineSwitch("create", "Create the database defined in ndb.exe.config for the current /environment")]
        [CommandLineAlias("cr")]
        public bool CreateDB { get; set; }

        [CommandLineSwitch("migrate", "Migrate Database")]
        [CommandLineAlias("m")]
        public bool Migrate { get; set; }

        [CommandLineSwitch("aspnet_membership", "Install ASP.NET Membership Database")]
        [CommandLineAlias("aspnet_membership")]
        public bool Membership { get; set; }

        [CommandLineSwitch("trace", "Turn on invoke/execute tracing, enable full backtrace.")]
        [CommandLineAlias("t")]
        public bool Trace { get; set; }

        [CommandLineSwitch("environment", "Specify the environment to run script.")]
        [CommandLineAlias("env")]
        public DBEnvironments Environment { get; set; }
    }
}
