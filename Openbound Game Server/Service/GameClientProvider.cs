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

using Openbound_Game_Server.Common;
using Openbound_Game_Server.Server;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using Openbound_Network_Object_Library.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Openbound_Network_Object_Library.Models;
using Openbound_Network_Object_Library.Entity.Text;

namespace Openbound_Game_Server.Service
{
    class GameClientProvider
    {
        #region Connection
        public static bool GameServerPlayerAccessRequest(string param, Dictionary<int, object> paramDictionary, ConcurrentQueue<byte[]> provider)
        {
            try
            {
                Player player = ObjectWrapper.DeserializeRequest<Player>(param);
                lock (NetworkObjectParameters.GameServerInformation)
                {
                    if (NetworkObjectParameters.GameServerInformation.ConnectedClients + 1 > NetworkObjectParameters.GameServerInformation.ConnectedClientCapacity)
                        return false;

                    lock (GameServerObjects.Instance.PlayerHashtable)
                    {
                        if (GameServerObjects.Instance.PlayerHashtable.ContainsKey(player.ID))
                            return false;
                        else
                        {
                            PlayerSession pS = new PlayerSession() { Player = player, ProviderQueue = provider };
                            paramDictionary.Add(NetworkObjectParameters.PlayerSession, pS);
                            GameServerObjects.Instance.PlayerHashtable.Add(player.ID, pS);
                            NetworkObjectParameters.GameServerInformation.ConnectedClients++;
                            GameServerObjects.lobbyServerCSP.RequestList.Enqueue(NetworkObjectParameters.GameServerRegisterRequest, NetworkObjectParameters.GameServerInformation);
                        }
                    }
                }

                return true;
            }
            catch (Exception) { }

            return false;
        }
        #endregion

        #region Server Information Request
        public static bool GameServerSearchPlayer(string param)
        {
            try
            {
                Player player = ObjectWrapper.DeserializeRequest<Player>(param);
                lock (GameServerObjects.Instance.PlayerHashtable)
                {
                    return GameServerObjects.Instance.PlayerHashtable.ContainsKey(player.ID);
                }
            }
            catch (Exception) { }

            return false;
        }
        #endregion

        #region Game List / Requests 
        public static RoomMetadata GameServerRoomListCreateRoom(string param, PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = ObjectWrapper.DeserializeRequest<RoomMetadata>(param);

                lock (GameServerObjects.Instance.RoomMetadataSortedList)
                {
                    //Look for the first available room id;
                    room.ID = 1;
                    while (GameServerObjects.Instance.RoomMetadataSortedList.ContainsKey(room.ID)) room.ID++;

                    //Remove the player that was already addded in the serialization proccess
                    room.RemovePlayer(playerSession.Player);
                    room.AddA(playerSession.Player);

                    //Insert the room to the match metadata list;
                    GameServerObjects.Instance.RoomMetadataSortedList.Add(room.ID, room);
                }

                playerSession.Player.PlayerNavigation = PlayerNavigation.InGameRoom;
                playerSession.Player.PlayerRoomStatus = PlayerRoomStatus.Master;
                playerSession.RoomMetadata = room;
                playerSession.RoomMetadata.RoomOwner = playerSession.Player;

                Console.WriteLine($"- {room.RoomOwner.Nickname} created a room ({room.ID} - {room.Name})");

                return room;
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }

            return null;
        }

        public static void GameServerRoomListRequestList(string param, ConcurrentQueue<byte[]> provider)
        {
            try
            {
                RoomMetadata filter = ObjectWrapper.DeserializeRequest<RoomMetadata>(param);

                lock (GameServerObjects.Instance.RoomMetadataSortedList)
                {
                    var query = GameServerObjects.Instance.RoomMetadataSortedList.AsEnumerable();

                    if (filter.GameMode != GameMode.Any)
                        query = query.Where((x) => x.Value.GameMode == filter.GameMode);

                    if (!filter.IsPlaying)
                        query = query.Where((x) => !x.Value.IsPlaying);

                    //The last filter should be by page
                    query = query.Skip(9 * filter.PageNumber).Take(10);

                    query.ToList().ForEach((room) =>
                    {
                        provider.Enqueue(NetworkObjectParameters.GameServerRoomListRequestList, room.Value);
                    });
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }

        public static RoomMetadata GameServerRoomListRoomEnter(string param, PlayerSession playerSession)
        {
            try
            {
                RoomMetadata filter = ObjectWrapper.DeserializeRequest<RoomMetadata>(param);

                lock (GameServerObjects.Instance.RoomMetadataSortedList)
                {
                    RoomMetadata room = GameServerObjects.Instance.RoomMetadataSortedList[filter.ID];

                    lock (room)
                    {
                        List<Player> roomUnion = room.PlayerList;

                        //if the room is full, refuse returning null
                        if (room.NumberOfPlayers == (int)room.Size) return null;

                        //insert the player on the lowest numbered team
                        if (room.TeamASafe.Count() <= room.TeamBSafe.Count())
                            room.AddA(playerSession.Player);
                        else
                            room.AddB(playerSession.Player);

                        playerSession.Player.PlayerLoadingStatus = PlayerRoomStatus.NotReady;
                        playerSession.Player.PlayerRoomStatus = PlayerRoomStatus.NotReady;
                        playerSession.Player.PlayerNavigation = PlayerNavigation.InGameRoom;

                        playerSession.RoomMetadata = room;

                        //send an update for each member of the match with the current metadata
                        ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, roomUnion);
                    }

                    Console.WriteLine($" - {playerSession.Player.Nickname} joined the room ({room.ID} - {room.Name})");

                    return room;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }

            //case things goes bad, return null to the request user
            return null;
        }
        #endregion

        #region Game Room / Requests
        public static bool GameServerRoomLeaveRoom(PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;
                Player player = playerSession.Player;

                lock (GameServerObjects.Instance.RoomMetadataSortedList)
                {
                    //remove the player from each possible team
                    lock (room)
                    {
                        room.RemovePlayer(player);

                        playerSession.RoomMetadata = null;
                        playerSession.Player.PlayerNavigation = PlayerNavigation.InGameMenus;

                        Console.WriteLine($" - {player.Nickname} left the room ({room.ID} - {room.Name})");

                        if (room.NumberOfPlayers == 0)
                        {
                            GameServerObjects.Instance.RoomMetadataSortedList.Remove(room.ID);
                            Console.WriteLine($" - A room was destroyed ({room.ID} - {room.Name})");
                            return true;
                        }

                        List<Player> roomUnion = room.PlayerList;

                        if (player.PlayerRoomStatus == PlayerRoomStatus.Master)
                        {
                            Player newMaster = roomUnion.First();

                            newMaster.PlayerRoomStatus = PlayerRoomStatus.Master;
                            room.RoomOwner = newMaster;
                        }

                        //send an update for each member of the match with the current metadata
                        ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, roomUnion);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }

            return false;
        }

        public static void GameServerRoomChangePrimaryMobile(string param, PlayerSession playerSession)
        {
            try
            {
                MobileType mobileType = ObjectWrapper.DeserializeRequest<MobileType>(param);

                RoomMetadata room = playerSession.RoomMetadata;

                lock (room)
                {
                    playerSession.Player.PrimaryMobile = mobileType;
                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, room.PlayerList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static void GameServerRoomReadyRoom(PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;
                Player player = playerSession.Player;

                lock (room)
                {
                    List<Player> roomUnion = room.PlayerList;

                    if (player.ID == room.RoomOwner.ID)
                    {
                        int readyPlayers = roomUnion.Where((x) => x.PlayerRoomStatus == PlayerRoomStatus.Ready).Count();

                        if (/*readyPlayers + 1 == room.NumberOfPlayers &&
                            room.TeamA.Count() == room.TeamB.Count()*/true)
                        {
                            Console.WriteLine($" - Room ({room.ID} - {room.Name}) has started!");

                            //Sets state of each player to be used in the "Loading Complete" method, also resets the room status to "not ready" in case the game goes back to the room screen.
                            roomUnion.ForEach((x) =>
                            {
                                x.PlayerLoadingStatus = PlayerRoomStatus.NotReady;
                                x.PlayerRoomStatus = PlayerRoomStatus.NotReady;
                                x.PlayerNavigation = PlayerNavigation.InLoadingScreen;
                            });

                            //Resets the RoomOwner status to master.
                            room.RoomOwner.PlayerRoomStatus = PlayerRoomStatus.Master;

                            //Prepare match
                            room.StartMatch();

                            ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, roomUnion);
                            ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomReadyRoom, null, roomUnion);
                        }
                    }
                    else
                    {
                        player.PlayerRoomStatus = (player.PlayerRoomStatus == PlayerRoomStatus.NotReady) ?
                            PlayerRoomStatus.Ready : PlayerRoomStatus.NotReady;
                    }

                    Console.WriteLine($" - {player.Nickname} on ({room.ID} - {room.Name}) is {player.PlayerRoomStatus}");

                    //send an update for each member of the match with the current metadata
                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, roomUnion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        internal static void GameServerRoomChangeTeam(PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;
                Player player = playerSession.Player;

                lock (room)
                {
                    if (room.TeamA.Contains(player) && room.TeamB.Count < 4)
                    {
                        room.RemovePlayer(player);
                        room.AddB(player);
                    }
                    else if (room.TeamB.Contains(player) && room.TeamA.Count < 4)
                    {
                        room.RemovePlayer(player);
                        room.AddA(player);
                    }

                    //send an update for each member of the match with the current metadata
                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, room.PlayerList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        internal static void GameServerRoomChangeMap(string param, PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;
                Player player = playerSession.Player;

                lock (room)
                {
                    if (room.RoomOwner != player) return;

                    int mapIndex = ObjectWrapper.DeserializeRequest<int>(param);

                    if (mapIndex == NetworkObjectParameters.ChangeMapLeft) room.Map = Map.GetPreviousMap(room.Map);
                    else if (mapIndex == NetworkObjectParameters.ChangeMapRight) room.Map = Map.GetNextMap(room.Map);
                    else room.Map = Map.GetMap(mapIndex);

                    //send an update for each member of the match with the current metadata
                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshMetadata, room, room.PlayerList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static void GameServerRoomRefreshLoadingPercentage(string param, PlayerSession playerSession)
        {
            try
            {
                int loadingPercentage = int.Parse(param);
                playerSession.Player.LoadingScreenPercentage = loadingPercentage;

                Console.WriteLine($"{playerSession.Player.ID} - {playerSession.Player.Nickname} is {loadingPercentage}%");

                ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomRefreshLoadingPercentage, new KeyValuePair<int, int>(playerSession.Player.ID, loadingPercentage), playerSession.RoomMetadata.PlayerList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static void GameServerRoomStartInGameScene(PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;

                playerSession.Player.PlayerLoadingStatus = PlayerRoomStatus.Ready;

                CheckStartInGameSceneCondition(room);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static void GameServerRoomRequestDisconnect(PlayerSession playerSession)
        {
            RoomMetadata room = playerSession.RoomMetadata;

            lock (room)
                room.RemovePlayer(playerSession.Player);

            CheckStartInGameSceneCondition(room);
        }

        public static void CheckStartInGameSceneCondition(RoomMetadata matchMetadata)
        {
            lock (matchMetadata)
            {
                List<Player> roomUnion = matchMetadata.PlayerList;

                if (roomUnion.Any((x) => x.PlayerLoadingStatus != PlayerRoomStatus.Ready)) return;

                ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerRoomStartInGameScene, null, roomUnion);
            }
        }
        #endregion

        #region InGame / Requests
        public static void GameServerInGameStartMatch(PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;

                playerSession.Player.PlayerLoadingStatus = PlayerRoomStatus.NotReady;
                playerSession.Player.PlayerNavigation = PlayerNavigation.InGame;

                lock (room)
                {
                    List<Player> roomUnion = room.PlayerList;
                    if (roomUnion.Any((x) => x.PlayerNavigation != PlayerNavigation.InGame)) return;

                    MatchManager mm = new MatchManager(room);

                    RegisterIntoPlayerSession(roomUnion, (playerE) => { playerE.MatchManager = mm; });

                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerInGameStartMatch, mm.SyncMobileList, roomUnion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static void GameServerInGameRefreshSyncMobile(string param, PlayerSession playerSession)
        {
            try
            {
                SyncMobile filter = ObjectWrapper.DeserializeRequest<SyncMobile>(param);

                MatchManager mm = playerSession.MatchManager;
                Player player = playerSession.Player;

                lock (mm)
                {
                    SyncMobile sm = mm.SyncMobileList.ToList().Find((x) => x.Owner.ID == player.ID);
                    sm.Update(filter);
                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerInGameRefreshSyncMobile, mm.SyncMobileList, mm.MatchUnion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static MatchMetadata GameServerInGameRequestNextPlayerTurn(PlayerSession playerSession)
        {
            try
            {
                RoomMetadata room = playerSession.RoomMetadata;

                if (room.VictoriousTeam == null)
                    return playerSession.MatchManager.MatchMetadata;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }

            return null;
        }

        public static void GameServerInGameRequestShot(string param, PlayerSession playerSession)
        {
            try
            {
                SyncMobile filter = ObjectWrapper.DeserializeRequest<SyncMobile>(param);
                MatchManager mm = playerSession.MatchManager;

                lock (mm)
                {
                    mm.ComputePlayerAction(filter);
                }

                ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerInGameRequestShot, filter, mm.MatchUnion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public static void GameServerInGameRequestDeath(string param, PlayerSession playerSession)
        {
            try
            {
                SyncMobile filter = ObjectWrapper.DeserializeRequest<SyncMobile>(param);
                MatchManager mm = playerSession.MatchManager;

                lock (mm)
                {
                    SyncMobile sm = mm.SyncMobileList.Find((x) => x.Owner.ID == filter.Owner.ID);

                    if (sm == null) return;

                    filter.IsAlive = false;
                    sm.Update(filter);

                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerInGameRequestDeath, filter, mm.MatchUnion);
                    CheckWinConditions(mm, playerSession);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: When GameServerInGameRequestDeath {ex.Message}");
            }
        }

        public static void GameServerInGameRequestDisconnect(PlayerSession playerSession)
        {
            try
            {
                MatchManager mm = playerSession.MatchManager;
                if (playerSession.MatchManager == null) return;

                lock (mm)
                {
                    SyncMobile sm = mm.SyncMobileList.Find((x) => x.Owner.ID == playerSession.Player.ID);

                    if (sm == null) return;

                    sm.IsAlive = false;

                    ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerInGameRequestDisconnect, sm, mm.MatchUnion);
                    CheckWinConditions(mm, playerSession);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: When GameServerInGameRequestDisconnect {ex.Message}");
            }
        }

        public static void CheckWinConditions(MatchManager matchManager, PlayerSession playerSession)
        {
            List<SyncMobile> aliveMobList = matchManager.SyncMobileList.Where((x) => x.IsAlive).ToList();

            PlayerTeam? victoriousTeam = null;

            //Win by K.O.
            if (!aliveMobList.Exists((x) => x.Owner.PlayerTeam == PlayerTeam.Red)) victoriousTeam = PlayerTeam.Blue;
            else if (!aliveMobList.Exists((x) => x.Owner.PlayerTeam == PlayerTeam.Blue)) victoriousTeam = PlayerTeam.Red;

            if (victoriousTeam == null) return;

            RoomMetadata room = playerSession.RoomMetadata;

            lock (room)
            {
                room.VictoriousTeam = victoriousTeam;
                room.IsPlaying = false;
                room.PlayerList.ForEach((x) => x.PlayerNavigation = PlayerNavigation.InGameRoom);
                /*save data on database*/
            }

            ServerwideBroadcastToPlayer(NetworkObjectParameters.GameServerInGameRequestGameEnd, victoriousTeam, matchManager.MatchUnion);
        }
        #endregion

        #region Messaging / Room List Chat Requests
        public static int GameServerChatRoomListEnter(string param, PlayerSession playerSession)
        {
            try
            {
                //Parse the player selected channel to find out its index
                int channelID = 0;
                int.TryParse(param, out channelID);

                lock (GameServerObjects.Instance.ChatDictionary)
                {
                    //find the first non-full channel IF the player hasn't selected a specific channel
                    if (channelID == 0)
                    {
                        channelID = GameServerObjects.Instance.ChatDictionary.Keys.First((x) => GameServerObjects.Instance.ChatDictionary[x].Count < NetworkObjectParameters.GameServerMaximumChatChannelCapacity);
                    }
                    else
                    {
                        //if the selected channel is already full, returns channel connection error
                        return NetworkObjectParameters.ServerProcessingError;
                    }

                    //if player is connected to any channel, disconnect from the selected channel
                    if (playerSession.CurrentConnectedChat != 0)
                        GameServerObjects.Instance.ChatDictionary[playerSession.CurrentConnectedChat].Add(playerSession.Player);

                    //Connect to new channel
                    GameServerObjects.Instance.ChatDictionary[channelID].Add(playerSession.Player);

                    //Sends welcome message to player
                    playerSession.ProviderQueue.Enqueue(NetworkObjectParameters.GameServerChatGameListSendSystemMessage, Message.CreateWelcomeMessage(channelID));
                }

                //Updates the current connected channel id
                playerSession.CurrentConnectedChat = channelID;

                return channelID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: When GameServerChatRoomListEnter {ex.Message}");
            }

            return NetworkObjectParameters.ServerProcessingError;
        }

        public static void GameServerChatRoomListLeave(PlayerSession playerSession)
        {
            try
            {
                lock (GameServerObjects.Instance.ChatDictionary)
                {
                    //if player is connected to any channel, disconnect from the selected channel
                    if (playerSession.CurrentConnectedChat != 0)
                        GameServerObjects.Instance.ChatDictionary[playerSession.CurrentConnectedChat].Remove(playerSession.Player);
                }

                //Updates the current connected channel id
                playerSession.CurrentConnectedChat = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: When GameServerChatRoomListLeave {ex.Message}");
            }
        }

        public static void GameServerChatRoomSendMessage(string param, PlayerSession playerSession)
        {
            try
            {
                PlayerMessage pm = ObjectWrapper.DeserializeRequest<PlayerMessage>(param);

                lock (GameServerObjects.Instance.ChatDictionary)
                {
                    RestrictBroadcastToPlayer(
                        NetworkObjectParameters.GameServerChatGameListSendPlayerMessage, pm,
                        GameServerObjects.Instance.ChatDictionary[playerSession.CurrentConnectedChat]);
                }   
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: When GameServerChatRoomSendMessage {ex.Message}");
            }
        }
        #endregion

        #region Helping Functions
        private static void RegisterIntoPlayerSession(List<Player> playerList, Action<PlayerSession> action)
        {
            lock (GameServerObjects.Instance.PlayerHashtable)
            {
                playerList.ForEach((user) =>
                {
                    PlayerSession tmpSession = (PlayerSession)GameServerObjects.Instance.PlayerHashtable[user.ID];
                    action(tmpSession);
                });
            }
        }

        private static void ServerwideBroadcastToPlayer(int service, object message, List<Player> players)
        {
            lock (GameServerObjects.Instance.PlayerHashtable)
            {
                foreach (Player p in players)
                {
                    if (!GameServerObjects.Instance.PlayerHashtable.ContainsKey(p.ID)) continue;
                    ((PlayerSession)GameServerObjects.Instance.PlayerHashtable[p.ID]).ProviderQueue.Enqueue(service, message);
                }
            }
        }

        private static void RestrictBroadcastToPlayer(int service, object message, HashSet<Player> players)
        {
            foreach (Player p in players)
            {
                if (!GameServerObjects.Instance.PlayerHashtable.ContainsKey(p.ID)) continue;
                ((PlayerSession)GameServerObjects.Instance.PlayerHashtable[p.ID]).ProviderQueue.Enqueue(service, message);
            }
        }
        #endregion

        #region DEBUG
        private static void DebugMethod(PlayerSession playerSession)
        {
            if (!playerSession.RoomMetadata.PlayerList.Contains(playerSession.RoomMetadata.RoomOwner))
                Console.WriteLine("\n\n NOT FOUND HIM! OWNER \n\n");

            if (!playerSession.RoomMetadata.PlayerList.Contains(playerSession.Player))
                Console.WriteLine("\n\n NOT FOUND HIM! PLAYER \n\n");
        }
        #endregion

    }
}
