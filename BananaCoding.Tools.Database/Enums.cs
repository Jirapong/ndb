using System;
using System.Collections.Generic;
using System.Text;

namespace BananaCoding.Tools.Database
{
    internal enum MigrationKind
    {
        Up,
        Down,
        ToCurrentVersion,
        ToVersion
    }

    public enum DBEnvironments
    {
        Development,
        Production
    }
    
}
