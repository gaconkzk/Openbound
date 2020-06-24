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

using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.ServerCommunication.Service;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using Openbound_Network_Object_Library.TCP.ServiceProvider;
using System;
using System.Collections.Generic;
using Openbound_Network_Object_Library.Models;
using Openbound_Network_Object_Library.Entity.Text;

namespace OpenBound.ServerCommunication
{
    public class ServerInformationBroker
    {
        public Dictionary<int, Action<object>> ActionCallbackDictionary;
        private static ServerInformationBroker instance;
        public static ServerInformationBroker Instance
        {
            get
            {
                if (instance == null) instance = new ServerInformationBroker();
                return instance;
            }
        }

        public ClientServiceProvider LobbyServerServiceProvider { get; private set; }
        public ClientServiceProvider GameServerServiceProvider { get; private set; }

        private ServerInformationBroker()
        {
            ActionCallbackDictionary = new Dictionary<int, Action<object>>();

            LobbyServerServiceProvider = new ClientServiceProvider(
                NetworkObjectParameters.LobbyServerInformation.ServerLocalAddress,
                NetworkObjectParameters.LobbyServerInformation.ServerPort,
                NetworkObjectParameters.LobbyServerBufferSize,
                NetworkObjectParameters.LobbyServerBufferSize,
                LobbyServerConsumerAction);

            ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerRoomRefreshMetadata, UpdateRoomMetadataAsyncCallback);

            LobbyServerServiceProvider.StartOperation();
        }

        #region Basic Cross-Screen-Services
        private static void UpdateRoomMetadataAsyncCallback(object answer)
        {
            RoomMetadata room = (RoomMetadata)answer;

            lock (GameInformation.Instance.RoomMetadata)
            {
                GameInformation.Instance.RoomMetadata = room;
            }
        }
        #endregion

        public void ConnectToGameServer(GameServerInformation serverInformation)
        {
            GameServerServiceProvider = new ClientServiceProvider(
                serverInformation.ServerPublicAddress, serverInformation.ServerPort,
                NetworkObjectParameters.LobbyServerBufferSize, NetworkObjectParameters.LobbyServerBufferSize,
                GameServerConsumerAction);

            GameServerServiceProvider.StartOperation();
        }

        public void DisconnectFromGameServer()
        {
            Instance.GameServerServiceProvider.StopOperation();
        }

        public void StopServices()
        {
            LobbyServerServiceProvider.StopOperation();

            if (GameServerServiceProvider != null)
                GameServerServiceProvider.StopOperation();
        }

        // HUBS
        public void LobbyServerConsumerAction(ClientServiceProvider csp, string[] request)
        {
            int service = int.Parse(request[0]);

            switch (service)
            {
                case NetworkObjectParameters.LobbyServerServerListRequest:
                    ServerInformationHandler.ServerListHandle(request[1]);
                    break;
            }
        }

        public void GameServerConsumerAction(ClientServiceProvider csp, string[] request)
        {
            int service = int.Parse(request[0]);
            object answer;

#if DEBUG
            Console.WriteLine(service + "|" + request[1]);
#endif

            switch (service)
            {
                case NetworkObjectParameters.GameServerPlayerAccessRequest:
                    answer = ObjectWrapper.DeserializeRequest<bool>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerPlayerAccessRequest](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomListCreateRoom:
                    answer = ObjectWrapper.DeserializeRequest<RoomMetadata>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomListCreateRoom](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomListRequestList:
                    answer = ObjectWrapper.DeserializeRequest<RoomMetadata>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomListRequestList](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomListRoomEnter:
                    answer = ObjectWrapper.DeserializeRequest<RoomMetadata>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomListRoomEnter](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomRefreshMetadata:
                    answer = ObjectWrapper.DeserializeRequest<RoomMetadata>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomRefreshMetadata](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomLeaveRoom:
                    answer = ObjectWrapper.DeserializeRequest<bool>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomLeaveRoom](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomReadyRoom:
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomReadyRoom](null);
                    break;
                case NetworkObjectParameters.GameServerRoomRefreshLoadingPercentage:
                    answer = ObjectWrapper.DeserializeRequest<KeyValuePair<int, int>>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomRefreshLoadingPercentage](answer);
                    break;
                case NetworkObjectParameters.GameServerRoomStartInGameScene:
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerRoomStartInGameScene](null);
                    break;
                case NetworkObjectParameters.GameServerInGameStartMatch:
                    answer = ObjectWrapper.DeserializeRequest<List<SyncMobile>>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameStartMatch](answer);
                    break;
                case NetworkObjectParameters.GameServerInGameRefreshSyncMobile:
                    answer = ObjectWrapper.DeserializeRequest<List<SyncMobile>>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameRefreshSyncMobile](answer);
                    break;
                case NetworkObjectParameters.GameServerInGameRequestNextPlayerTurn:
                    answer = ObjectWrapper.DeserializeRequest<MatchMetadata>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameRequestNextPlayerTurn](answer);
                    break;
                case NetworkObjectParameters.GameServerInGameRequestShot:
                    answer = ObjectWrapper.DeserializeRequest<SyncMobile>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameRequestShot](answer);
                    break;
                case NetworkObjectParameters.GameServerInGameRequestDeath:
                    answer = ObjectWrapper.DeserializeRequest<SyncMobile>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameRequestDeath](answer);
                    break;
                case NetworkObjectParameters.GameServerInGameRequestGameEnd:
                    answer = ObjectWrapper.DeserializeRequest<PlayerTeam>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameRequestGameEnd](answer);
                    break;
                case NetworkObjectParameters.GameServerInGameRequestDisconnect:
                    answer = ObjectWrapper.DeserializeRequest<SyncMobile>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerInGameRequestDisconnect](answer);
                    break;

                //Chat
                case NetworkObjectParameters.GameServerChatJoinChannel:
                    answer = ObjectWrapper.DeserializeRequest<int>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerChatJoinChannel](answer);
                    break;
                case NetworkObjectParameters.GameServerChatEnter:
                    answer = ObjectWrapper.DeserializeRequest<Player>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerChatEnter](answer);
                    break;
                case NetworkObjectParameters.GameServerChatLeave:
                    answer = ObjectWrapper.DeserializeRequest<Player>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerChatLeave](answer);
                    break;
                case NetworkObjectParameters.GameServerChatSendPlayerMessage:
                    answer = ObjectWrapper.DeserializeRequest<PlayerMessage>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerChatSendPlayerMessage](answer);
                    break;
                case NetworkObjectParameters.GameServerChatSendSystemMessage:
                    answer = ObjectWrapper.DeserializeRequest<List<CustomMessage>>(request[1]);
                    ActionCallbackDictionary[NetworkObjectParameters.GameServerChatSendSystemMessage](answer);
                    break;

            }
        }
    }
}
