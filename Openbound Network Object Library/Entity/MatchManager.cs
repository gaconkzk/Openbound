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

using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity.Sync;
using System.Collections.Generic;
using System.Linq;
using Openbound_Network_Object_Library.Models;

namespace Openbound_Network_Object_Library.Entity
{
    public class MatchManager
    {
        public List<SyncMobile> SyncMobileList;

        private RoomMetadata roomMetadata;
        public MatchMetadata MatchMetadata;

        public List<Player> MatchUnion => roomMetadata.PlayerList;

        public SyncMobile CurrentTurnOwner => SyncMobileList.First((x) => x.IsAlive);


        public MatchManager() { }
        public MatchManager(RoomMetadata roomMetadata)
        {
            this.roomMetadata = roomMetadata;

            Queue<Player> sortedTeam1 = new Queue<Player>(roomMetadata.TeamASafe.OrderBy((x) => NetworkObjectParameters.Random.NextDouble()));
            Queue<Player> sortedTeam2 = new Queue<Player>(roomMetadata.TeamBSafe.OrderBy((x) => NetworkObjectParameters.Random.NextDouble()));

            //Randomizing Player Turn Order
            if (NetworkObjectParameters.Random.Next(2) == 0)
            {
                var tmp = sortedTeam1;
                sortedTeam1 = sortedTeam2;
                sortedTeam2 = tmp;
            }

            SyncMobileList = new List<SyncMobile>();

            do
            {
                if (sortedTeam1.Count > 0)
                {
                    Player pA = sortedTeam1.Dequeue();
                    SyncMobileList.Add(new SyncMobile(pA, roomMetadata.SpawnPositions[pA.ID]));
                }

                if (sortedTeam2.Count > 0)
                {
                    Player pB = sortedTeam2.Dequeue();
                    SyncMobileList.Add(new SyncMobile(pB, roomMetadata.SpawnPositions[pB.ID]));
                }
            } while (sortedTeam1.Count + sortedTeam2.Count != 0);

            MatchMetadata = new MatchMetadata(roomMetadata.Map);
            MatchMetadata.CurrentTurnOwner = CurrentTurnOwner;
        }

        public void ComputePlayerAction(SyncMobile filter)
        {
            SyncMobile sMob = SyncMobileList.Find((x) => x.Owner.ID == filter.Owner.ID);
            
            //Clearing the action list to wipe the "Charging" action from the shooting player
            sMob.SynchronizableActionList.Clear();

            filter.Delay = sMob.Delay += sMob.MobileMetadata.GetDelay(sMob.SelectedShotType);

            //Mobile / Delay
            SyncMobileList = SyncMobileList.OrderBy((x) => x.Delay).ToList();

            //Turn pass
            MatchMetadata.PassTurn(roomMetadata.OriginalTeamSize);
            MatchMetadata.CurrentTurnOwner = CurrentTurnOwner;

            //SS Lock (if necessary)
            if (filter.SelectedShotType == ShotType.SS /* && filter.SSLockRemainingTurns == 0 */)
                filter.SSLockRemainingTurns += NetworkObjectParameters.SSCooldownTimer + 1;
        }
    }
}
