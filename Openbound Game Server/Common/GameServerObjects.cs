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
using Openbound_Network_Object_Library.TCP.ServiceProvider;
using System.Collections;
using System.Collections.Generic;

namespace Openbound_Game_Server.Common
{
    public class GameServerObjects
    {
        private static GameServerObjects instance { get; set; }
        public static GameServerObjects Instance
        {
            get
            {
                if (instance == null) instance = new GameServerObjects();
                return instance;
            }
        }

        public static ServerServiceProvider serverServiceProvider;
        public static ClientServiceProvider lobbyServerCSP;

        public Hashtable PlayerHashtable { get; set; }
        public SortedList<int, RoomMetadata> RoomMetadataSortedList { get; set; }

        private GameServerObjects()
        {
            PlayerHashtable = new Hashtable();
            RoomMetadataSortedList = new SortedList<int, RoomMetadata>();
        }
    }
}
