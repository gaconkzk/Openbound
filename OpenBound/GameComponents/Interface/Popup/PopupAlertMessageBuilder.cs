﻿/* 
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
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Level.Scene;
using System.Collections.Generic;

namespace OpenBound.GameComponents.Interface.Popup
{
    public enum PopupAlertMessageType
    {
        LeaveGame,
    }

    public class PopupAlertMessageBuilder
    {
        public static PopupMenu BuildPopupAlertMessage(PopupAlertMessageType type)
        {
            PopupMenu popupMenu = null;

            switch (type)
            {
                case (PopupAlertMessageType.LeaveGame):
                    popupMenu = LeaveGame();
                    break;
            }

            PopupHandler.Add(popupMenu);

            return popupMenu;
        }

        public static PopupAlertMessage LeaveGame()
        {
            List<List<SpriteText>> stMatrix;
            List<SpriteText> titleSTList, messageSTList1, messageSTList2;
            titleSTList = new List<SpriteText>();
            titleSTList.Add(new SpriteText(FontTextType.Consolas11, Language.PopupAlertMessageLeaveGameTitle, Color.White, Alignment.Left, layerDepth: DepthParameter.SceneTransitioningEffectBase));

            messageSTList1 = new List<SpriteText>();
            messageSTList1.Add(new SpriteText(FontTextType.Consolas10, Language.PopupAlertMessageLeaveGameMessage11, Color.White, Alignment.Left, layerDepth: DepthParameter.SceneTransitioningEffectBase));
            messageSTList1.Add(new SpriteText(FontTextType.Consolas10, Language.PopupAlertMessageLeaveGameMessage12, Color.LightSalmon, Alignment.Left, layerDepth: DepthParameter.SceneTransitioningEffectBase));
            messageSTList1.Add(new SpriteText(FontTextType.Consolas10, Language.PopupAlertMessageLeaveGameMessage13, Color.White, Alignment.Left, layerDepth: DepthParameter.SceneTransitioningEffectBase));

            messageSTList2 = new List<SpriteText>();
            messageSTList2.Add(new SpriteText(FontTextType.Consolas10, Language.PopupAlertMessageLeaveGameMessage21, Color.White, Alignment.Left, layerDepth: DepthParameter.SceneTransitioningEffectBase));

            stMatrix = new List<List<SpriteText>>();
            stMatrix.Add(messageSTList1);
            stMatrix.Add(messageSTList2);

            CompositeSpriteText title, message;

            title = CompositeSpriteText.CreateCompositeSpriteText(titleSTList, Orientation.Horizontal, Alignment.Left, default);
            message = CompositeSpriteText.CreateCompositeSpriteText(stMatrix, Alignment.Left, Vector2.Zero + new Vector2(0, 20), new Vector2(0, 0));

            PopupAlertMessage popup = new PopupAlertMessage(title, message);
            popup.OnConfirm = (x) => { SceneHandler.Instance.CloseGame(); };
            return popup;
        }
    }
}
