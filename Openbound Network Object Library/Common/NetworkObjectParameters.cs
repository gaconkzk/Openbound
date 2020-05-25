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

using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;

namespace Openbound_Network_Object_Library.Common
{
    public class NetworkObjectParameters
    {
        //IsOnline expiration date
        public static readonly int GameServerLastOnlineExpirationInSeconds = 30;
        public static readonly int GameServerLastOnlineRefreshingInSeconds = (int)(GameServerLastOnlineExpirationInSeconds * 0.8);

        //Request Buffer Size
        public const int LoginServerBufferSize = 4096;
        public const int LobbyServerBufferSize = 4096;
        public const int GameServerBufferSize  = 4096;
        //public static readonly int 

        //Login Server
        public const int LoginServerLoginAttemptRequest = 0x0008;
        public const int LoginServerAccountCreationRequest = 0x0009;

        //Lobby Server
        public const int LobbyServerPlayerUIDRequest = 0x0001;
        public const int LobbyLoginRequest = 0x0005;
        public const int LobbyServerServerListRequest = 0x0007;

        //Game Server
        public const int GameServerRegisterRequest = 0x0020;
        public const int GameServerMetadataRequest = 0x0021;
        public const int GameServerPlayerAccessRequest = 0x0022;
        public const int GameServerSearchPlayer = 0x0023;

        //Game Server - Room
        public const int GameServerRoomListCreateRoom = 0x0024;
        public const int GameServerRoomListRequestList = 0x0025;
        public const int GameServerRoomListRoomEnter = 0x0026;
        public const int GameServerRoomRefreshMetadata = 0x0027;
        public const int GameServerRoomLeaveRoom = 0x0028;
        public const int GameServerRoomReadyRoom = 0x0029;
        public const int GameServerRoomRefreshLoadingPercentage = 0x002a;
        public const int GameServerRoomStartInGameScene = 0x002b;
        public const int GameServerRoomChangePrimaryMobile = 0x0034;
        public const int GameServerRoomChangeTeam = 0x0035;
        public const int GameServerRoomChangeMap = 0x0036;

        //Game Server - InGame
        public const int GameServerInGameStartMatch = 0x002c;
        public const int GameServerInGameRefreshSyncMobile = 0x002d;
        public const int GameServerInGameRequestNextPlayerTurn = 0x002e;
        public const int GameServerInGameRequestShot = 0x0030;
        public const int GameServerInGameRequestDeath = 0x0031;
        public const int GameServerInGameRequestGameEnd = 0x0032;
        public const int GameServerInGameRequestDisconnect = 0x0033;

        //Error
        public const int ServerProcessingError = 0x0000;

        //Server Information
        public static ServerInformation LoginServerInformation;
        public static ServerInformation LobbyServerInformation;
        public static GameServerInformation GameServerInformation;

        //Database Information
        public static string DatabaseAddress;
        public static string DatabaseName;
        public static string DatabaseLogin;
        public static string DatabasePassword;

        //Server Registration Information
        public static readonly int SecurityTokenExpirationInMinutes = 1;
        public static List<string> GameServerRequestIPWhitelist;

        //Game Entity Identification
        public const int PlayerSession = 0x01;

        //Server Default Behaviour
        public const int LoginServerDefaultPort = 8022;
        public const int LobbyServerDefaultPort = 8023;
        public const int GameServerDefaultStartingPort = 8024;

        public static int ResilientMultiClientTCPListenerAuthBufferSize { get; internal set; }

        public const int RandomSeed = 10;
        public static readonly Random Random = new Random(RandomSeed);

        //Game Constants
        public static List<MobileType> ImplementedMobileList = new List<MobileType>() { MobileType.Armor, MobileType.Bigfoot, MobileType.Dragon, MobileType.Ice, MobileType.Knight, MobileType.Mage, MobileType.Random, MobileType.Turtle };

        //Game Constants - Weather
        public const int WeatherMinimumWindForce = 0;
        public const int WeatherMaximumWindForce = 35;
        public const int WeatherWindForceDisturbance = 4;
        public const int WeatherWindAngleDisturbance = 6;
        public const double WeatherWindAngleDisturbanceChance = 0.5;

        //Map Address
        public const int ChangeMapLeft = -2;
        public const int ChangeMapRight = -1;
    }
}
