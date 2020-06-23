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

using System.Globalization;

namespace OpenBound.Common
{
    public class Language
    {
        //Interface Components - Game Modes
        public const string GameModeSolo  = "Solo";
        public const string GameModeScore = "Score";
        public const string GameModeTag   = "Tag";
        public const string GameModeJewel = "Jewel";

        //Interface - Messages
        public const string HUDTextAllText  = "All";
        public const string HUDTextTeamText = "Team";

        //Interface - Weather - Weather Names
        public const string WeatherForce       = "Force";
        public const string WeatherTornado     = "Tornado";
        public const string WeatherElectricity = "Electricity";
        public const string WeatherWeakness    = "Weakness";
        public const string WeatherMirror      = "Mirror";
        public const string WeatherRandom      = "Random";

        //Popup - Create Room
        public static string PopupCreateRoomOneVersusOneText     = "Room Size: 2 players, 1 on each side.";
        public static string PopupCreateRoomTwoVersusTwoText     = "Room Size: 4 players, 2 on each side.";
        public static string PopupCreateRoomThreeVersusThreeText = "Room Size: 6 players, 3 on each side.";
        public static string PopupCreateRoomFourVersusFourText   = "Room Size: 8 players, 4 on each side.";

        public static string PopupCreateRoomScoreText = "The first team to destroy the remaining\n" +
                                                        "number of vehicles shown on the scoreboard\n" +
                                                        "wins.";
        public static string PopupCreateRoomSoloText  = "The last team standing in the arena wins.";
        public static string PopupCreateRoomTagText   = "Each player has two vehicles, the second\n" +
                                                        "mobile have half of its original HP. The\n" +
                                                        "last team standing in the arena\n" +
                                                        "wins.";
        public static string PopupCreateRoomJewelText = "Each team must destroy the Jewels, little\n" +
                                                        "mechanical creatures, in order to claim it's\n" +
                                                        "points. On the end of the match\n" +
                                                        "the team with the biggest score\n" +
                                                        "wins.";

        public static string PopupCreateRoomTitle    = "Room Title: ";
        public static string PopupCreateRoomPassword = "Room Password: ";

        //Popup - Game Options

        public const string PopupGameOptionsGameplay                = "Gameplay";
        public const string PopupGameOptionsGameplayAimingMode      = "Aiming Mode";
        public const string PopupGameOptionsGameplayAimingModeDrag  = "Drag";
        public const string PopupGameOptionsGameplayAimingModeSlice = "Slice";

        public const string PopupGameOptionsGameplayScrollSpeed = "Scroll Speed";
        public const string PopupGameOptionsGameplayMouseSpeed  = "Mouse Speed";

        public const string PopupGameOptionsSound    = "Sound";
        public const string PopupGameOptionsSoundBGM = "BGM";
        public const string PopupGameOptionsSoundSFX = "SFX";

        public const string PopupGameOptionsMisc              = "Miscelaneous";
        public const string PopupGameOptionsMiscBackground    = "Background";
        public const string PopupGameOptionsMiscBackgroundOn  = "On";
        public const string PopupGameOptionsMiscBackgroundOff = "Off";

        public const string PopupGameOptionsMiscInterface          = "Interface";
        public const string PopupGameOptionsMiscInterfaceClassic   = "Classic";
        public const string PopupGameOptionsMiscInterfaceTHBlue    = "TH Blue";
        public const string PopupGameOptionsMiscInterfaceTHWhite   = "TH White";
        public const string PopupGameOptionsMiscInterfaceOpenbound = "Openbound";

        //Popup - AlertMessage - Leave Game

        public const string PopupAlertMessageLeaveGameTitle     = "You want to close the game?";
        public const string PopupAlertMessageLeaveGameMessage11 = "Your current leave percentage is ";
        public static string PopupAlertMessageLeaveGameMessage12 => $"{string.Format(CultureInfo.InvariantCulture, "{0:0.00}", GameInformation.Instance.PlayerInformation.LeavePercentage)}";
        public static string PopupAlertMessageLeaveGameMessage13 =  " %";
        public const string  PopupAlertMessageLeaveGameMessage21 =  "If you leave this number is going to increase.\nPlayers with higher leave percentages are less likely\nto be trusted by others as a good player and you may\nalso not be able to play in some servers.\nYou still want to proceed?";

        public static string PopupCreateRoomNamePlaceholder => $"{GameInformation.Instance.PlayerInformation.Nickname}'s Room";

        //public static string GamemodeSolo = "Solo";
    }
}
