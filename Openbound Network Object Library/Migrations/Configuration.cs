
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Database.Context;
using Openbound_Network_Object_Library.FileOutput;
using Openbound_Network_Object_Library.Models;

namespace Openbound_Network_Object_Library.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<OpenboundDatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            ConfigFileManager.LoadConfigFile(RequesterApplication.EFMigration);
        }

        
        protected override void Seed(OpenboundDatabaseContext context)
        {
            if (!context.Players.Any()) SeedPlayers(context);
        }

        private void SeedPlayers(OpenboundDatabaseContext context)
        {
            string defaultPassword = "$2y$12$ZcVa8MvpkEUx5LlQ5BNjNOezed07s8b71I5OcYq5vf1q52tjASjki";
            context.Players.AddOrUpdate(x => x.ID,
                new Player { Nickname = "Winged", Email = "dev00@dev.com", Password = defaultPassword },
                new Player { Nickname = "Wicked", Email = "dev01@dev.com", Password = defaultPassword },
                new Player { Nickname = "Willow", Email = "dev02@dev.com", Password = defaultPassword },
                new Player { Nickname = "Vinny", Email = "dev03@dev.com", Password = defaultPassword },
                new Player { Nickname = "Test", Email = "test00@dev.com", Password = defaultPassword}
            );
        }
    } 
}