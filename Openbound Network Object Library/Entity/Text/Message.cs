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


using Microsoft.Xna.Framework;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Extension;
using Openbound_Network_Object_Library.Models;
using System.Collections.Generic;

namespace Openbound_Network_Object_Library.Entity.Text
{
    #region Message Types
    /// <summary>
    /// This kind of messages are always interpreted by the game client in the following pattern:
    /// [<see cref="Player.Nickname"/>]: Text. All text colors are placed automatically,
    /// the nickname color is set by <see cref="Message.TextToColor"/>. If the PlayerTeam is set
    /// the text is painted as accordingly. If not, the default color is white.
    /// </summary>
    public class PlayerMessage
    {
        public Player Player;
        public string Text;
        public PlayerTeam? PlayerTeam;
    }

    /// <summary>
    /// There is no default interpretation for this kind of message. Each line MUST be composed
    /// by a list of custom messages. This element does not support line breaks and all texts
    /// must be preciselly calculated since it overflows the textbox. Check
    /// <see cref="Message.RoomWelcomeMessage"/> for a multi-line example with combined characters
    /// of font-awesome tokens and consolas texts.
    /// </summary>
    public class CustomMessage
    {
        public string Text;
        public uint Token;
        public uint TextColor, TextBorderColor;
        public FontTextType FontTextType;

        public void AppendTokenToText() { Text += (char)Token; }
    }
    #endregion

    public class Message
    {
        #region TextBuilders
        public static string BuildGameServerChatGameList(int id) => NetworkObjectParameters.GameServerChatGameListIdentifier + id.ToString();
        public static string BuildGameServerChatGameRoom(int id) => NetworkObjectParameters.GameServerChatGameRoomIdentifier + id.ToString();

        private static CustomMessage CreateFontAwesomeText(uint token) => new CustomMessage() { Token = token, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.FontAwesome10 };
        private static CustomMessage CreateConsolasText(string text) => new CustomMessage() { Text = text, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.Consolas10 };
        private static CustomMessage CreatePlayerColoredText(string text) => new CustomMessage() { Text = text, TextColor = TextToColor(text).PackedValue, TextBorderColor = Color.Black.PackedValue, FontTextType = FontTextType.Consolas10 };
        #endregion

        #region Font Awesome Texts (Tokens)
        private static CustomMessage FAComputerToken = CreateFontAwesomeText(0xf108);
        private static CustomMessage FAServerToken = CreateFontAwesomeText(0xf233);
        private static CustomMessage FAGamepadToken = CreateFontAwesomeText(0xf11b);
        private static CustomMessage FASadCry = CreateFontAwesomeText(0xf5b3);
        private static CustomMessage FAHeartBroken = CreateFontAwesomeText(0xf7a9);
        #endregion

        #region Consolas Texts
        private static CustomMessage CSpace = CreateConsolasText(" ");

        //Welcome
        //- Channel
        private static CustomMessage CChannelWelcome1 = CreateConsolasText(Language.ChannelWelcomeMessage1);
        private static CustomMessage CChannelWelcome2 = CreateConsolasText(" " + NetworkObjectParameters.GameServerInformation?.ServerName + Language.ChannelWelcomeMessage2);

        //- Room
        private static CustomMessage CRoomWelcome1  = CreateConsolasText(Language.RoomWelcomeMessage1);
        private static CustomMessage CRoomWelcome1E = CreateConsolasText(").");
        private static CustomMessage CRoomWelcome2  = CreateConsolasText(Language.RoomWelcomeMessage2);

        //In Game Messages
        //- Death
        private static CustomMessage IGMDeath1 = CreateConsolasText(Language.DeathMessage1);
        #endregion

        #region 'Pre-baked' Messages
        //WelcomeMessage
        //- Channel
        private static List<CustomMessage> ChannelWelcomeMessage = new List<CustomMessage>() { FAComputerToken, CChannelWelcome1, FAServerToken, CChannelWelcome2 };

        //- Room
        private static List<List<CustomMessage>> RoomWelcomeMessage = new List<List<CustomMessage>>() {
            new List<CustomMessage>() { FAComputerToken, CRoomWelcome1, FAGamepadToken, CSpace, CRoomWelcome1E },
            new List<CustomMessage>() { FAComputerToken, CRoomWelcome2 } };

        //In Game Messages
        //- Death
        private static List<CustomMessage> DeathMessage = new List<CustomMessage>() { FASadCry, CSpace, IGMDeath1, FAHeartBroken };
        #endregion

        #region Customized 'Pre-baked' Message
        //Welcome
        //- Channel
        public static List<CustomMessage> CreateChannelWelcomeMessage(int channel) =>
            new List<CustomMessage>(ChannelWelcomeMessage) { CreateConsolasText(channel + "") };

        //- Room
        public static List<List<CustomMessage>> CreateRoomWelcomeMessage(string roomName)
        {
            List<List<CustomMessage>> cmL = new List<List<CustomMessage>>()
            {
                new List<CustomMessage>(RoomWelcomeMessage[0]),
                new List<CustomMessage>(RoomWelcomeMessage[1]),
            };

            cmL[0].Insert(3, CreateConsolasText(roomName));
            return cmL;
        }

        //In Game Messages
        //- Death
        public static List<CustomMessage> CreateDeathMessage(Player owner)
        {
            List<CustomMessage> cmL = new List<CustomMessage>(DeathMessage);
            cmL.Insert(2, CreatePlayerColoredText(owner.Nickname));
            return cmL;
        }
        #endregion

        public static Color TextToColor(string text)
        {
            uint color = 0x0;

            for (int i = 0; i < text.Length - 1; i++)
            {
                color ^= text[i];
                color = color.RotateLeft(8);
            }

            Color c = new Color(color);
            c.A = (byte)(20 + c.A * 1.2);
            c.R = (byte)(20 + c.R * 1.2);
            c.B = (byte)(20 + c.B * 1.2);
            c.G = (byte)(20 + c.G * 1.2);

            return c;
        }
    }
}
