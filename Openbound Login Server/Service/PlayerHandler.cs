﻿/* 
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
using Openbound_Network_Object_Library.Database.Controller;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Extension;
using Openbound_Network_Object_Library.TCP.ServiceProvider;
using System;
using System.Threading;
using Openbound_Network_Object_Library.Models;
using Openbound_Network_Object_Library.ValidationModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Core;

namespace Openbound_Login_Server.Service
{
    class PlayerHandler
    {
        public static void StartListeningPlayerLoginAttempt(ConcurrentQueue<byte[]> provider, string param)
        {
            try
            {
                PlayerController pc = new PlayerController();

                Player deserializedPlayer = ObjectWrapper.DeserializeRequest<Player>(param);
                Player player = pc.LoginPlayer(deserializedPlayer);

                Player answer = null;

                if (player != null)
                {
                    ClientServiceProvider csp = new ClientServiceProvider(
                        NetworkObjectParameters.LobbyServerInformation.ServerLocalAddress,
                        NetworkObjectParameters.LobbyServerInformation.ServerPort,
                        NetworkObjectParameters.LobbyServerBufferSize,
                        NetworkObjectParameters.LobbyServerBufferSize,
                        (_provider, _request) =>
                        {
                            answer = ObjectWrapper.DeserializeRequest<Player>(_request[1]);
                        });
                    csp.StartOperation();
                    csp.RequestList.Enqueue(NetworkObjectParameters.LobbyServerPlayerUIDRequest, player);

                    while (answer == null)
                        Thread.Sleep(100);

                    csp.StopOperation();

                    deserializedPlayer.Nickname = answer.Nickname;
                }

                Console.WriteLine($"- Request for: {deserializedPlayer.Nickname} - {deserializedPlayer.Password} - Player " + ((answer == null) ? "not " : "") + "found");

                if (answer == null)
                {
                    provider.Enqueue(NetworkObjectParameters.LoginServerLoginAttemptRequest, null);
                    return;
                }

                // Preparing Player Information Stream
                Dictionary<AvatarCategory, HashSet<int>> ownedAvatars = pc.RetrivePlayerAvatarList(player);

                provider.Enqueue(NetworkObjectParameters.LoginServerLoginAttemptRequest, player);

                foreach (KeyValuePair<AvatarCategory, HashSet<int>> kvp in ownedAvatars)
                {
                    //For safety reasons, the maximum integer stream size must be 300 integers.
                    const int maximumSize = 300;
                    for(int i = 0; i < kvp.Value.Count; i += maximumSize)
                    {
                        provider.Enqueue(NetworkObjectParameters.LoginServerLoginAttemptRequest, (int)kvp.Key, kvp.Value.Skip(i).Take(maximumSize).ToList());
                    }
                }

                //Sends the "EOF" to client
                provider.Enqueue(NetworkObjectParameters.LoginServerLoginAttemptRequest, 0, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                throw ex;
            }
        }

        public static string RegistrationAttempt(string param)
        {
            try
            {
                Account deserializedAccount = ObjectWrapper.DeserializeRequest<Account>(param);
                string registrationResult = new PlayerController().RegisterPlayer(deserializedAccount);

                return registrationResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                throw ex;
            }
        }
    }
}
