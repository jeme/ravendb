﻿using System.Collections.Generic;

namespace Raven.Server.SqlMigration.Model
{
    public class MigrationSettings
    {
        /* TODO
         public bool BinaryToAttachment { get; set; } = true;
            */
        public List<RootCollection> Collections { get; set; }
        public int BatchSize { get; set; } = 1000;
    }
}