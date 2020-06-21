using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openbound_Network_Object_Library.Entity.Text
{
    /// <summary>
    /// Player message struct. So far, it is only being used in here. Eventually ill drop it on NetworkObjectLibrary
    /// </summary>
    public struct PlayerMessage
    {
        public Player Player;
        public string Text;
    }

    public struct CustomMessage
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

        private static CustomMessage FAComputerToken = CreateFontAwesomeText(0xf108);
        private static CustomMessage FAServerToken = CreateFontAwesomeText(0xf233);
        private static CustomMessage FAGamepadToken = CreateFontAwesomeText(0xf11b);

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
        private static List<CustomMessage> RoomWelcomeMessage1 =
            new List<CustomMessage>() {
                FAComputerToken,
                CreateConsolasText(Language.RoomWelcomeMessage1),
                FAGamepadToken,
                CreateConsolasText(")."),
            };

        private static List<CustomMessage> RoomWelcomeMessage2 =
            new List<CustomMessage>() { FAComputerToken, CreateConsolasText(Language.RoomWelcomeMessage2) };

        public static List<CustomMessage> CreateRoomWelcomeMessage1(string roomName)
        {
            List<CustomMessage> cmL = new List<CustomMessage>(RoomWelcomeMessage1);
            cmL.Insert(3, CreateConsolasText(roomName));
            return cmL;
        }

        public static List<CustomMessage> CreateRoomWelcomeMessage2()
        {
            return RoomWelcomeMessage2;
        }
        #endregion
    }
}
