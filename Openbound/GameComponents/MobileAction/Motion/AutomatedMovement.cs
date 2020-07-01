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
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Pawn;
using Openbound_Network_Object_Library.Entity.Sync;
using System.Collections.Generic;

namespace OpenBound.GameComponents.MobileAction.Motion
{
    public class AutomatedMovement : Movement
    {
        public Vector2 TargetPosition;

        public AutomatedMovement(Mobile mobile) : base(mobile)
        { }

        public override void Update()
        {
            ApplyGravity();

            if (IsAbleToMove)
                Move();

            UpdateAngle();
        }

        public void Move()
        {
            //If is falling, dont move
            if (IsFalling) return;

            Vector2 movVector = Mobile.Position - TargetPosition;

            bool canMove = IsAbleToMove;

            //and move to the left
            if (movVector.X > 0)
            {
                MoveSideways(Mobile.Facing);
            }
            else if (movVector.X < 0)
            {
                MoveSideways(Mobile.Facing);
            }

            if (canMove && !IsAbleToMove)
            {
                Mobile.ChangeFlipbookState(ActorFlipbookState.Activated, true);
                Mobile.LoseTurn();
            }
        }

        public override void InvalidateMovementAttempt()
        {
            RemainingStepsThisTurn = 0;
        }
    }
}
