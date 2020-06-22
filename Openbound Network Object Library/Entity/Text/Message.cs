using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Extension;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openbound_Network_Object_Library.Entity.Text
{
    /// <summary>
    /// Player message class. So far, it is only being used in here. Eventually ill drop it on NetworkObjectLibrary
    /// </summary>
    public class PlayerMessage
    {
        public Player Player;
        public string Text;
        public PlayerTeam? PlayerTeam;
    }

    public class CustomMessage
    {
        public string Text;
        public uint Token;
        public uint TextColor, TextBorderColor;
        public FontTextType FontTextType;

        public void AppendTokenToText() { Text += (char)Token; }
    }

    public class Message
    {
        public static string BuildGameServerChatGameList(int id) => NetworkObjectParameters.GameServerChatGameListIdentifier + id.ToString();
        public static string BuildGameServerChatGameRoom(int id) => NetworkObjectParameters.GameServerChatGameRoomIdentifier + id.ToString();

        private static CustomMessage CreateFontAwesomeText(uint token) => new CustomMessage() { Token = token, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.FontAwesome10 };
        private static CustomMessage CreateConsolasText(string text) => new CustomMessage() { Text = text, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.Consolas10 };
        private static CustomMessage CreatePlayerColoredText(string text) => new CustomMessage() { Text = text, TextColor = TextToColor(text).PackedValue, TextBorderColor = Color.Black.PackedValue, FontTextType = FontTextType.Consolas10 };

        private static CustomMessage FAComputerToken = CreateFontAwesomeText(0xf108);
        private static CustomMessage FAServerToken = CreateFontAwesomeText(0xf233);
        private static CustomMessage FAGamepadToken = CreateFontAwesomeText(0xf11b);
        private static CustomMessage FASadCry = CreateFontAwesomeText(0xf5b3);
        private static CustomMessage FAHeartBroken = CreateFontAwesomeText(0xf7a9);

        private static CustomMessage CSpace = CreateConsolasText(" ");

        #region Channel Welcome Message
        private static List<CustomMessage> ChannelWelcomeMessage =
            new List<CustomMessage>() {
                FAComputerToken,
                CreateConsolasText(Language.ChannelWelcomeMessage1),
                FAServerToken,
                CreateConsolasText(" " + NetworkObjectParameters.GameServerInformation?.ServerName + Language.ChannelWelcomeMessage2),
            };

        public static List<CustomMessage> CreateChannelWelcomeMessage(int channel)
        {
            return new List<CustomMessage>(ChannelWelcomeMessage) { CreateConsolasText(channel + "") };
        }
        #endregion

        #region Rooom Welcome Message
        private static List<List<CustomMessage>> RoomWelcomeMessage =
            new List<List<CustomMessage>>() {
                new List<CustomMessage>()
                {
                    FAComputerToken,
                    CreateConsolasText(Language.RoomWelcomeMessage1),
                    FAGamepadToken,
                    CSpace,
                    CreateConsolasText(")."),
                },
                new List<CustomMessage>()
                {
                    FAComputerToken,
                    CreateConsolasText(Language.RoomWelcomeMessage2)
                }
            };

        public static List<List<CustomMessage>> CreateRoomWelcomeMessage(string roomName)
        {
            List<List<CustomMessage>> cmL = new List<List<CustomMessage>>()
            {
                new List<CustomMessage>(RoomWelcomeMessage[0]),
                new List<CustomMessage>(RoomWelcomeMessage[1]),
            };

            cmL[0].Insert(4, CreateConsolasText(roomName));
            return cmL;
        }
        #endregion

        #region Death Message

        private static List<CustomMessage> DeathMessage =
            new List<CustomMessage>() {
                FASadCry,
                CSpace,
                CreateConsolasText(Language.DeathMessage1),
                FAHeartBroken,
            };

        public static List<CustomMessage> BuildDeathMessage(Player owner)
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
