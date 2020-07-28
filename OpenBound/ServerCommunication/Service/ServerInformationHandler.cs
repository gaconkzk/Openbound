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

using OpenBound.Common;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Entity.Sync;
using Openbound_Network_Object_Library.Extension;
using System;
using System.Collections.Generic;
using Openbound_Network_Object_Library.Models;

namespace OpenBound.ServerCommunication.Service
{
    class ServerInformationHandler
    {
        //Lobby Server Handlers
        public static void RequestServerList()
        {
            ServerInformationBroker.Instance.LobbyServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.LobbyServerServerListRequest,
                null);
        }

        #region Server List Handlers
        public static void ServerListHandle(string request)
        {
            try
            {
                GameServerInformation si = ObjectWrapper.DeserializeRequest<GameServerInformation>(request);

                lock (GameInformation.Instance.ServerList)
                {
                    if (GameInformation.Instance.ServerList == null)
                        GameInformation.Instance.ServerList = new List<GameServerInformation>();
                    GameInformation.Instance.ServerList.Add(si);
                }
            }
            catch (Exception) { }
        }
        #endregion

        //Game Server Handlers
        #region Connection
        public static void ConnectToGameServer(GameServerInformation serverInformation)
        {
            ServerInformationBroker.Instance.ConnectToGameServer(serverInformation);

            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerPlayerAccessRequest,
                GameInformation.Instance.PlayerInformation);
        }
        #endregion

        #region GameList
        //Room
        public static void CreateRoom(RoomMetadata roomMetadata)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomListCreateRoom,
                roomMetadata
                );
        }

        public static void GameServerRequestRoomList(RoomMetadata roomMetadata)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomListRequestList,
                roomMetadata
                );
        }

        public static void ConnectToRoom(RoomMetadata roomMetadata)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomListRoomEnter,
                roomMetadata
                );
        }

        public static void SendGameListMessage(PlayerMessage message)
        {
#if !DEBUGSCENE
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerChatSendPlayerMessage,
                message);
#endif
        }

        public static void SendChatConnectionRequest(string channelID)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerChatEnter,
                channelID);
        }

        public static void SendChatDisconnectionRequest()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerChatLeave, null);
        }
        #endregion

        #region GameRoom
        public static void ChangePrimaryMobile(MobileType mobileType)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomChangePrimaryMobile,
                mobileType
                );
        }

        public static void ChangeTeam()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomChangeTeam,
                null
                );
        }

        public static void LeaveRoom()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomLeaveRoom,
                null
                );
        }

        public static void ReadyRoom()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomReadyRoom,
                null
                );
        }

        public static void ChangeMap(int mapIndex)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomChangeMap,
                mapIndex
                );
        }
        #endregion

        #region LoadingScreen
        public static void UpdateLoadingScreenPercentage(int loadingPercentage)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomRefreshLoadingPercentage,
                loadingPercentage
                );
        }

        public static void ClientReadyToStartGame()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerRoomStartInGameScene,
                null
                );
        }
        #endregion

        #region InGame
        public static void RequestNextPlayerTurn()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerInGameRequestNextPlayerTurn,
                null
                );
        }

        public static void StartMatch()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
               NetworkObjectParameters.GameServerInGameStartMatch,
               null
               );
        }

        public static void SynchronizeMobileStatus(SyncMobile syncMobile)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerInGameRefreshSyncMobile,
                syncMobile
                );
        }

        public static void SynchronizeItemUsage(SyncMobile syncMobile)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerInGameRequestItemUsage,
                syncMobile
                );
        }

        public static void RequestShot(SyncMobile syncMobile)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
               NetworkObjectParameters.GameServerInGameRequestShot,
               syncMobile
               );
        }

        public static void RequestDeath(SyncMobile syncMobile)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerInGameRequestDeath,
                syncMobile
                );
        }
        #endregion

        public static void AvatarShopBuyAvatarGold(AvatarMetadata avatarMetadata)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerAvatarShopBuyAvatarGold,
                avatarMetadata
                );
        }

        public static void AvatarShopBuyAvatarCash(AvatarMetadata avatarMetadata)
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerAvatarShopBuyAvatarCash,
                avatarMetadata
                );
        }

        public static void AvatarShopUpdatePlayerData()
        {
            ServerInformationBroker.Instance.GameServerServiceProvider.RequestList.Enqueue(
                NetworkObjectParameters.GameServerAvatarShopUpdatePlayerData,
                GameInformation.Instance.PlayerInformation);
        }
    }
}
