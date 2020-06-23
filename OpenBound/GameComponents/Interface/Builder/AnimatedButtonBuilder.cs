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
using OpenBound.Common;
using OpenBound.GameComponents.Interface.Interactive;
using OpenBound.GameComponents.Interface.Text;
using Openbound_Network_Object_Library.Entity.Text;
using System;

namespace OpenBound.GameComponents.Interface.Builder
{
    public class AnimatedButtonBuilder
    {
        public static AnimatedButton BuildButton(AnimatedButtonType type, Vector2 position, Action<object> onClick = default)
        {
            string buttonText = "";

            //Animated Buttons
            if (type == AnimatedButtonType.ExitDoor)
                buttonText = Parameter.ButtonTextExitText;
            else if (type == AnimatedButtonType.Buddy)
                buttonText = Parameter.ButtonTextBuddyList;
            else if (type == AnimatedButtonType.CashCharge)
                buttonText = Parameter.ButtonTextCashCharge;
            else if (type == AnimatedButtonType.ReportPlayer)
                buttonText = Parameter.ButtonTextReportPlayer;
            else if (type == AnimatedButtonType.AvatarShop)
                buttonText = Parameter.ButtonTextAvatarShop;
            else if (type == AnimatedButtonType.Options)
                buttonText = Parameter.ButtonTextOptions;
            else if (type == AnimatedButtonType.MyInfo)
                buttonText = Parameter.ButtonTextMyInfo;
            else if (type == AnimatedButtonType.MuteList)
                buttonText = Parameter.ButtonTextMuteList;
            //Room Filtering - Gamemode
            else if (type == AnimatedButtonType.Solo)
                buttonText = Parameter.ButtonTextRoomFilteringSolo;
            else if (type == AnimatedButtonType.Tag)
                buttonText = Parameter.ButtonTextRoomFilteringTag;
            else if (type == AnimatedButtonType.Score)
                buttonText = Parameter.ButtonTextRoomFilteringScore;
            else if (type == AnimatedButtonType.Jewel)
                buttonText = Parameter.ButtonTextRoomFilteringJewel;
            //Room Filtering - Type
            else if (type == AnimatedButtonType.ViewAll)
                buttonText = Parameter.ButtonTextRoomFilteringViewAll;
            else if (type == AnimatedButtonType.ViewWaiting)
                buttonText = Parameter.ButtonTextRoomFilteringViewWaiting;
            else if (type == AnimatedButtonType.ViewFriend)
                buttonText = Parameter.ButtonTextRoomFilteringViewFriend;
            //Room Options
            else if (type == AnimatedButtonType.GoTo)
                buttonText = Parameter.ButtonTextGoTo;
            else if (type == AnimatedButtonType.Create)
                buttonText = Parameter.ButtonTextCreate;
            else if (type == AnimatedButtonType.QuickJoin)
                buttonText = Parameter.ButtonTextQuickJoin;
            //Room Buttons
            else if (type == AnimatedButtonType.Ready)
                buttonText = Parameter.ButtonTextInRoomReady;
            else if (type == AnimatedButtonType.MatchSettings)
                buttonText = Parameter.ButtonTextInRoomMatchSettings;
            else if (type == AnimatedButtonType.ChangeMobile)
                buttonText = Parameter.ButtonTextInRoomChangeMobile;
            else if (type == AnimatedButtonType.ChangeItem)
                buttonText = Parameter.ButtonTextInRoomChangeItem;
            else if (type == AnimatedButtonType.ChangeTeam)
                buttonText = Parameter.ButtonTextInRoomChangeTeam;

            return new AnimatedButton(type, position, onClick,
                new SpriteText(Parameter.AnimatedButtonFont,
                    buttonText, Color.White, Alignment.Center,
                    layerDepth: DepthParameter.InterfaceButtonText));
        }
    }
}
