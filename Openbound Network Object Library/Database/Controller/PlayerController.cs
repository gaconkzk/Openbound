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
                        CharacterGender = newAccount.CharacterGender,
                        Email = newAccount.Email
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
    }
}
