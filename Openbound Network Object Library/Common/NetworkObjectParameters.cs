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
using Microsoft.Xna.Framework;
using Openbound_Network_Object_Library.Models;

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
        public const int LoginServerLoginAttemptRequest    = 0x0008;
        public const int LoginServerAccountCreationRequest = 0x0009;

        //Error
        public const int ServerProcessingError = 0x0000;

        //Lobby Server
        public const int LobbyServerPlayerUIDRequest  = 0x0001;
        public const int LobbyServerLoginRequest      = 0x0005;
        public const int LobbyServerServerListRequest = 0x0007;
        public const int LobbyServerAvatarMetadata    = 0x003e;

        //Game Server
        public const int GameServerRegisterRequest     = 0x0020;
        public const int GameServerMetadataRequest     = 0x0021;
        public const int GameServerPlayerAccessRequest = 0x0022;
        public const int GameServerSearchPlayer        = 0x0023;

        //Game Server - Room
        public const int GameServerRoomListCreateRoom           = 0x0024;
        public const int GameServerRoomListRequestList          = 0x0025;
        public const int GameServerRoomListRoomEnter            = 0x0026;
        public const int GameServerRoomRefreshMetadata          = 0x0027;
        public const int GameServerRoomLeaveRoom                = 0x0028;
        public const int GameServerRoomReadyRoom                = 0x0029;
        public const int GameServerRoomRefreshLoadingPercentage = 0x002a;
        public const int GameServerRoomStartInGameScene         = 0x002b;
        public const int GameServerRoomChangePrimaryMobile      = 0x0034;
        public const int GameServerRoomChangeTeam               = 0x0035;
        public const int GameServerRoomChangeMap                = 0x0036;

        //Game Server - InGame
        public const int GameServerInGameStartMatch            = 0x002c;
        public const int GameServerInGameRefreshSyncMobile     = 0x002d;
        public const int GameServerInGameRequestNextPlayerTurn = 0x002e;
        public const int GameServerInGameRequestShot           = 0x0030;
        public const int GameServerInGameRequestDeath          = 0x0031;
        public const int GameServerInGameRequestGameEnd        = 0x0032;
        public const int GameServerInGameRequestDisconnect     = 0x0033;

        //Messaging / Chat Requests
        public const int GameServerChatEnter             = 0x0037;
        public const int GameServerChatLeave             = 0x0038;
        public const int GameServerChatSendPlayerMessage = 0x0039;
        public const int GameServerChatSendSystemMessage = 0x003a;
        public const int GameServerChatJoinChannel       = 0x003b;

        //Avatar Shop - Buy Avatar
        public const int GameServerAvatarShopBuyAvatarGold      = 0x003f;
        public const int GameServerAvatarShopBuyAvatarCash      = 0x0040;
        public const int GameServerAvatarShopUpdatePlayerData = 0x0041;

        //Chat - Game Server
        public const int  GameServerChatChannelsMaximumNumber  = 8;
        public const char GameServerChatGameListIdentifier     = 'G';
        public const char GameServerChatGameRoomIdentifier     = 'R';

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
        public static List<MobileType> ImplementedMobileList = new List<MobileType>() { MobileType.Armor, MobileType.Bigfoot, MobileType.Dragon, MobileType.Ice, MobileType.Knight, MobileType.Mage, MobileType.Random, MobileType.RaonLauncher, MobileType.Trico, MobileType.Turtle };
        public static List<WeatherType> ActiveWeatherEffectList = new List<WeatherType>() { WeatherType.Force, WeatherType.Tornado, WeatherType.Electricity, WeatherType.Weakness, WeatherType.Mirror, WeatherType.Random, WeatherType.Thor };
        public static List<WeatherType> RandomizableWeatherEffectList = new List<WeatherType>() { WeatherType.Force, WeatherType.Tornado, WeatherType.Electricity, WeatherType.Weakness, WeatherType.Mirror };

        //Game Constants - Weather
        public const int WeatherMinimumWindForce = 0;
        public const int WeatherMaximumWindForce = 35;
        public const int WeatherWindForceDisturbance = 4;
        public const int WeatherWindAngleDisturbance = 6;
        public const double WeatherWindAngleDisturbanceChance = 0.5;

        //Game Constants - Weather - Thor
        //Thor pos calculation: MinimumOffset + Random(0, 1) * MaximumOffset
        public const float WeatherThorMinimumOffsetX = 0.1f;
        public const float WeatherThorMaximumOffsetX = 0.8f;

        public const float WeatherThorMinimumOffsetY = 0.3f;
        public const float WeatherThorMaximumOffsetY = 0.6f;

        //Map Address
        public const int ChangeMapLeft = -2;
        public const int ChangeMapRight = -1;

        //Game Messages
        public static uint ServerMessageColor       = Color.DarkOrange.PackedValue;
        public static uint ServerMessageBorderColor = Color.Black.PackedValue;

        //Player Status
        public const int PlayerAttributePerLevel = 10;
        public const int PlayerAttributeMaximumPerLevel = 180;
        public const int PlayerAttributeMaximumPerCategory = 50;

        public static Dictionary<int, PlayerRank> PlayerRankExperienceTable
            = new Dictionary<int, PlayerRank>()
        {
            {     0, PlayerRank.Chick },        //  100
            {   100, PlayerRank.WoodHammer1 },  //  200
            {   300, PlayerRank.WoodHammer2 },  //  200
            {   500, PlayerRank.StoneHammer1 }, //  300
            {   800, PlayerRank.StoneHammer2 }, //  300
            {  1100, PlayerRank.Axe1 },         //  400
            {  1500, PlayerRank.Axe2 },         //  400
            {  1900, PlayerRank.SilverAxe1 },   //  500
            {  2400, PlayerRank.SilverAxe2 },   //  500
            {  2900, PlayerRank.GoldenAxe1 },   //  600
            {  3500, PlayerRank.GoldenAxe2 },   //  600
            {  4100, PlayerRank.DAxe1 },        //  700
            {  4800, PlayerRank.DAxe2 },        //  700
            {  5500, PlayerRank.DSilverAxe1 },  //  800
            {  6300, PlayerRank.DSilverAxe2 },  //  800
            {  7100, PlayerRank.DGoldenAxe1 },  //  900
            {  8000, PlayerRank.DGoldenAxe2 },  //  900
            {  8900, PlayerRank.Staff1 },       // 1000
            {  9900, PlayerRank.Staff2 },       // 1000
            { 10900, PlayerRank.Staff3 },       // 1000
            { 11900, PlayerRank.Staff4 },       // 1000
            { 13000, PlayerRank.Dragon1 },      // 1100
            { 14100, PlayerRank.Dragon2 },      // 1100
            { 15200, PlayerRank.Dragon3 },      // 1100
        };

        public static Dictionary<int, PlayerRank> ExtraPlayerRankExperienceTable
            = new Dictionary<int, PlayerRank>()
        {
            { -1, PlayerRank.Champion1     }, // 2nd team in championship
            { -2, PlayerRank.WorldChampion }, // winner team in championship
            { -3, PlayerRank.Vip           }, // to be decided
            { -4, PlayerRank.GM            }  // game master
        };

        //Item balancing

        public const float InGameItemEnergyUp1Value       = 12f;
        public const float InGameItemEnergyUp1ExtraValue  =  3f;

        public const float InGameItemEnergyUp2Value       = 30f;
        public const float InGameItemEnergyUp2ExtraValue  =  3f;

        public const float InGameItemBloodValue           = 33f;
        public const float InGameItemBloodExtraValue      =  8f;

        public const float InGameItemPowerUpValue         = 30f;

        public const float InGameItemBungeShotValue       = 25f;

        public const int InGameItemDualCost         = 600;
        public const int InGameItemDualPlusCost     = 300;
        public const int InGameItemEnergyUp2Cost    = 300;
        public const int InGameItemTeamTeleportCost = 50;
        public const int InGameItemTeleportCost     = 150;
        public const int InGameItemThunder          = 200;

        public const int InGameItemEnergyUp1Cost    = 100;
        public const int InGameItemBloodCost        =   0;
        public const int InGameItemChangeWindCost   = 120;
        public const int InGameItemBungeShotCost    =  80;
        public const int InGameItemPowerUpCost      = 150;


    }
}
