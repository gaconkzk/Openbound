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

using Openbound_Network_Object_Library.Database.Context;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using Openbound_Network_Object_Library.ValidationModel;
using CryptSharp;
using Openbound_Network_Object_Library.Models;

namespace Openbound_Network_Object_Library.Database.Controller
{
    public class PlayerController
    {
        //Create Remove Update Delete
        public void CreatePlayer(Player NewPlayer)
        {
            using (var context = new OpenboundDatabaseContext())
            {
                bool canCreate = context.Players
                    .Where((x) =>
                    x.Email == NewPlayer.Email || x.Nickname == NewPlayer.Nickname
                    ).Count() != 0;

                //email/nick already in use
                if (canCreate) return;

                try
                {
                    context.Players.Add(NewPlayer);
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    //database is offline or fields lenght are invalid
                }
            }
        }

        public Player LoginPlayer(Player filter)
        {
            using (var context = new OpenboundDatabaseContext())
            {
                try
                {
                    filter.Nickname = filter.Nickname.ToLower();

                    Player tmpPlayer = context.Players
                        .Include((p) => p.Guild)
                        .FirstOrDefault((x) => x.Nickname.ToLower() == filter.Nickname);

                    if (tmpPlayer == null) return null;

                    if (!Crypter.CheckPassword(filter.Password, tmpPlayer.Password)) return null;

                    tmpPlayer.Email = null;
                    tmpPlayer.Password = null;

                    if (tmpPlayer.Guild != null)
                        tmpPlayer.Guild.GuildMembers = null;

                    return tmpPlayer;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    //Player not found
                    return null;
                }
            }
        }

        public Dictionary<AvatarCategory, HashSet<int>> RetrivePlayerAvatarList(Player player)
        {
            using (var context =new OpenboundDatabaseContext())
            {
                try
                {
                    Player tmpPlayer = context.Players
                        .Include(x => x.AvatarMetadataList)
                        .FirstOrDefault((x) => x.ID == player.ID);

                    tmpPlayer.LoadOwnedAvatarDictionary();
                    return tmpPlayer.OwnedAvatar;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public Player RetrivePlayer(Player Filter)
        {
            using (var context = new OpenboundDatabaseContext())
            {
                try
                {
                    Player tmpPlayer = context.Players.Where((x) => x.ID == Filter.ID).First();
                    tmpPlayer.Password = "";
                    return tmpPlayer;
                }
                catch (Exception)
                {
                    //Player not found
                    return null;
                }
            }
        }

        //Register a new player (account) and gives feedback if the registration was successful.
        public string RegisterPlayer(Account newAccount)
        {
            string answer = "";
            using (var context = new OpenboundDatabaseContext())
            {
                try
                {
                    Player newPlayer = new Player()
                    {
                        Nickname = newAccount.Nickname,
                        Password = Crypter.Blowfish.Crypt(newAccount.Password),
                        Gender = newAccount.CharacterGender,
                        Email = newAccount.Email,
                        AvatarMetadataList = context.AvatarMetadata.Where((x) => x.ID == 0).ToList(),
                    };

                    List<Player> sameIdPlayer = context.Players.Where(x => x.Nickname.ToLower() == newPlayer.Nickname.ToLower()
                                                                  || x.Email.ToLower() == newPlayer.Email.ToLower()).ToList();

                    if (sameIdPlayer.Count > 0)
                    {
                        if (sameIdPlayer.Any(x => x.Email.ToLower() == newPlayer.Email.ToLower()))
                            answer += "This email is already taken.\n";
                        if (sameIdPlayer.Any(x => x.Nickname.ToLower() == newPlayer.Nickname.ToLower()))
                            answer += "This nickname is already taken.";

                        return answer;
                    }
                    
                    context.Players.Add(newPlayer);
                    context.SaveChanges();
                 
                    return "success";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        public bool PurchaseAvatar(Player player, AvatarMetadata avatarMetadata, bool usingGold)
        {
            try
            {
                OpenboundDatabaseContext odc = new OpenboundDatabaseContext();

                AvatarMetadata databaseAvatar = odc.AvatarMetadata
                    .FirstOrDefault((x) => x.ID == avatarMetadata.ID && x.AvatarCategory == avatarMetadata.AvatarCategory);

                if ( (usingGold && player.Gold >= databaseAvatar.GoldPrice && databaseAvatar.GoldPrice > 0) ||
                    (!usingGold && player.Cash >= databaseAvatar.CashPrice && databaseAvatar.CashPrice > 0))
                {
                    using (var dbContextTransaction = odc.Database.BeginTransaction())
                    {
                        Player p = odc.Players.Find(player.ID);
                        
                        p.AvatarMetadataList.Add(databaseAvatar);

                        if (usingGold)
                            p.Gold -= databaseAvatar.GoldPrice;
                        else
                            p.Cash -= databaseAvatar.CashPrice;
                        
                        odc.SaveChanges();

                        dbContextTransaction.Commit();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Returns true if all player's equipped avatar exists in player's owned avatar list,
        /// otherwise returns false
        /// </summary>
        public bool CheckPlayerAvatarPossessions(Player player)
        {
            OpenboundDatabaseContext odc = new OpenboundDatabaseContext();

            List<int> avL = player.Avatar.ToList();

            //Searches for the 8 equipped avatars in players db avatar list.
            //If the number of matchs is equal to the number of equipped avatars
            //All avatars are owned, therefore returns true

            return odc.Players
               .Include(q => q.AvatarMetadataList)
               .First((x) => x.ID == player.ID)
               .AvatarMetadataList
               .Where(am => avL.Contains(am.ID))
               .Count() == avL.Count;
        }

        public void UpdatePlayerMetadata(Player player)
        {
            OpenboundDatabaseContext odc = new OpenboundDatabaseContext();

            Player p = odc.Players.Find(player.ID);
            p.Avatar = player.Avatar;
            p.Attribute = player.Attribute;
            
            odc.SaveChanges();
        }
    }
}
