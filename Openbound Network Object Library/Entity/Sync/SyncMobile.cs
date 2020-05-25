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

using System.Collections.Generic;
using Openbound_Network_Object_Library.Models;

namespace Openbound_Network_Object_Library.Entity.Sync
{
    public enum Facing
    {
        Left = -1,
        Right = 1,
    }

    public class SyncMobile
    {
        public Player Owner;
        public MobileMetadata MobileMetadata;
        public int[] Position;
        public int CrosshairAngle;
        public ShotType SelectedShotType;
        public Facing Facing;
        public SyncProjectile SyncProjectile;
        public int Delay;
        public bool IsAlive;

        public List<SynchronizableAction> SynchronizableActionList;

        public SyncMobile()
        {
            SynchronizableActionList = new List<SynchronizableAction>();
            Facing = Facing.Left;
            IsAlive = true;
        }

        public SyncMobile(Player player, int[] position)
        {
            Owner = player;
            MobileMetadata = MobileMetadata.BuildMobileMetadata(player.PrimaryMobile);
            Position = position;
            SynchronizableActionList = new List<SynchronizableAction>();
            Facing = Facing.Left;
            IsAlive = true;

            AimPreset aimPreset = MobileMetadata.MobileAimPreset[ShotType.S1];

            CrosshairAngle = (aimPreset.AimTrueRotationMax + aimPreset.AimTrueRotationMin) / 2;
        }

        public void Update(SyncMobile syncMobile)
        {
            MobileMetadata.CurrentHealth = syncMobile.MobileMetadata.CurrentHealth;
            MobileMetadata.CurrentShield = syncMobile.MobileMetadata.CurrentShield;
            Position = syncMobile.Position;
            CrosshairAngle = syncMobile.CrosshairAngle;
            SynchronizableActionList = syncMobile.SynchronizableActionList;
            SelectedShotType = syncMobile.SelectedShotType;
            Facing = syncMobile.Facing;
            IsAlive = syncMobile.IsAlive;
        }

        public void AddSynchronizableAction(SynchronizableAction synchronizableAction)
        {
            if (!SynchronizableActionList.Contains(synchronizableAction))
                SynchronizableActionList.Add(synchronizableAction);
        }

        public void RemoveSynchronizableAction(SynchronizableAction synchronizableAction)
        {
            SynchronizableActionList.RemoveAll((x) => x == synchronizableAction);
        }

        public bool ContainsAction(SynchronizableAction synchronizableAction)
        {
            return SynchronizableActionList.Contains(synchronizableAction);
        }
    }
}
