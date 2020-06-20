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

    public class Message {

        public static CustomMessage CreateFontAwesomeText(uint token) => new CustomMessage() { Token = token, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.FontAwesome10 };
        public static CustomMessage CreateConsolasText(string text) => new CustomMessage() { Text = text, TextColor = NetworkObjectParameters.ServerMessageColor, TextBorderColor = NetworkObjectParameters.ServerMessageBorderColor, FontTextType = FontTextType.Consolas10 };

        public static List<CustomMessage> WelcomeMessage =
            new List<CustomMessage>() {
                CreateFontAwesomeText(0xf108),
                CreateConsolasText(" " + Language.CreateWelcomeMessage1 + " ("),
                CreateFontAwesomeText(0xf233),
                CreateConsolasText(" " + NetworkObjectParameters.GameServerInformation.ServerName + ")"),
            };

        public static List<CustomMessage> CreateWelcomeMessage(int channel) =>
            new List<CustomMessage>(WelcomeMessage) { CreateConsolasText(Language.CreateWelcomeMessage2 + channel) };
    }
}
