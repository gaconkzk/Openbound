/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using Openbound_Network_Object_Library.FileOutput;
using Openbound_Network_Object_Library.Migrations;
using Openbound_Network_Object_Library.Models;

namespace Openbound_Network_Object_Library.Database.Context
{
    public class OpenboundDatabaseContext : DbContext
    {
        public OpenboundDatabaseContext()
            : base($"Data Source={NetworkObjectParameters.DatabaseAddress};Initial Catalog={NetworkObjectParameters.DatabaseName};Persist Security Info=True;User ID={NetworkObjectParameters.DatabaseLogin};Password={NetworkObjectParameters.DatabasePassword};MultipleActiveResultSets=True;PersistSecurityInfo=True")
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<OpenboundDatabaseContext, Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Guild> Guilds { get; set; }

    }
}
