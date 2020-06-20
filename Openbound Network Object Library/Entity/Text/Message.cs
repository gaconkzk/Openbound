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

        public static CustomMessage CreateFontAwesomeText(uint token) => new CustomMessage() { Token = token, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.FontAwesome10 };
        public static CustomMessage CreateConsolasText(string text) => new CustomMessage() { Text = text, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.Consolas10 };

        public static CustomMessage FAComputerToken = CreateFontAwesomeText(0xf108);
        public static CustomMessage FAServerToken = CreateFontAwesomeText(0xf233);
        public static CustomMessage FAGamepadToken = CreateFontAwesomeText(0xf11b);

        #region Channel Welcome Message
        public static List<CustomMessage> ChannelWelcomeMessage =
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
        public static List<CustomMessage> RoomWelcomeMessage =
            new List<CustomMessage>() {
                FAComputerToken,
                CreateConsolasText(Language.RoomWelcomeMessage1),
                FAGamepadToken,
                //Room name goes here,
                CreateConsolasText(Language.RoomWelcomeMessage2),
            };

        public static List<CustomMessage> CreateRoomWelcomeMessage(string roomName)
        {
            List<CustomMessage> mList = new List<CustomMessage>(RoomWelcomeMessage);
            mList.Insert(3, CreateConsolasText(roomName));
            return mList;
        }
        #endregion
    }
}
