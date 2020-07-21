
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ConfigFileManager.LoadConfigFile(RequesterApplication.EFMigration);
        }
        
        protected override void Seed(OpenboundDatabaseContext context)
        {
            if (!context.AvatarMetadata.Any())
            {
                SeedAvatarMetadata(context);
                context.SaveChanges();
            }

            if (!context.Players.Any())
            {
                SeedPlayer(context);
                context.SaveChanges();
            }
        }

        private void SeedPlayer(OpenboundDatabaseContext context)
        {
            string defaultPassword = "$2y$12$ZcVa8MvpkEUx5LlQ5BNjNOezed07s8b71I5OcYq5vf1q52tjASjki";
            context.Players.AddOrUpdate(x => x.ID,
                new Player { Nickname = "Winged", Email = "dev00@dev.com", Password = defaultPassword, AvatarMetadataList = context.AvatarMetadata.ToList() },
                new Player { Nickname = "Wicked", Email = "dev01@dev.com", Password = defaultPassword, AvatarMetadataList = context.AvatarMetadata.ToList() },
                new Player { Nickname = "Willow", Email = "dev02@dev.com", Password = defaultPassword, AvatarMetadataList = context.AvatarMetadata.ToList() },
                new Player { Nickname = "Vinny",  Email = "dev03@dev.com", Password = defaultPassword, AvatarMetadataList = context.AvatarMetadata.ToList() },
                new Player { Nickname = "Test",   Email = "test0@dev.com", Password = defaultPassword, AvatarMetadataList = context.AvatarMetadata.ToList() }
            );
        }

        private void SeedAvatarMetadata(OpenboundDatabaseContext context)
        {
            List<AvatarMetadata> list = ObjectWrapper.DeserializeCommentedJSONFile<List<AvatarMetadata>>(
                $"{Directory.GetCurrentDirectory()}/DatabaseSeed/AvatarMetadata.json");

            context.AvatarMetadata.AddOrUpdate(list.ToArray());
        }
    } 
}