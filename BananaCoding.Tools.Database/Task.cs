using System;
using System.Collections.Generic;
using System.Text;
using BananaCoding.CommandLineParser;

namespace BananaCoding.Tools.Database {
    /// <summary>
    /// Commandline tasks
    /// </summary>
    internal class Task {
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

        [CommandLineSwitch("migrate_to", "Migrate database to version. e.g. /mt:10")]
        [CommandLineAlias("mt")]
        public int MigrateTo { get; set; }

        [CommandLineSwitch("no_aspnet_membership", "Ignore install ASP.NET Membership Database (default no_aspnet)")]
        [CommandLineAlias("no_aspnet")]
        public bool NoMembership { get; set; }

        [CommandLineSwitch("aspnet_membership", "Install ASP.NET Membership Database")]
        [CommandLineAlias("aspnet")]
        public bool AspNet { get; set; }

        [CommandLineSwitch("trace", "Turn on invoke/execute tracing, enable full backtrace.")]
        [CommandLineAlias("t")]
        public bool Trace { get; set; }

        [CommandLineSwitch("environment", "Specify the environment to run script.")]
        [CommandLineAlias("env")]
        public DBEnvironments Environment { get; set; }

        [CommandLineSwitch("xml", "Xml output e.g. /xml:C:\\temp\\out.xml.")]
        [CommandLineAlias("xml")]
        public string Xml { get; set; }

        [CommandLineSwitch("fixture", "Include fixture data in \\fixtures folder.")]
        [CommandLineAlias("f")]
        public bool Fixture { get; set; }

        [CommandLineSwitch("fixture_to", "Fixture database with specific file. e.g. /ft:users")]
        [CommandLineAlias("ft")]
        public string FixtureTo { get; set; }

        [CommandLineSwitch("grant_permission", "Specify the user to grant permission to access database. e.g. /grant:\"NT AUTHORITY\\NETWORK SERVICE\"")]
        [CommandLineAlias("grant")]
        public string GrantPermission { get; set; }
    }
}
