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
using Microsoft.Xna.Framework.Graphics;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Asset;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Pawn.Unit;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenBound.GameComponents.Pawn
{
    public enum AvatarState
    {
        Normal,
        Staring,
    }

    public class Rider
    {
        static Dictionary<AvatarState, AnimationInstance> headAvatarState = new Dictionary<AvatarState, AnimationInstance>()
        {
            { AvatarState.Normal,  new AnimationInstance(){ StartingFrame = 11, EndingFrame = 21, TimePerFrame = 1/10f, AnimationType = AnimationType.Cycle } },
            { AvatarState.Staring, new AnimationInstance(){ StartingFrame = 00, EndingFrame = 10, TimePerFrame = 1/10f, AnimationType = AnimationType.Cycle } },
        };

        static Dictionary<AvatarState, AnimationInstance> bodyAvatarState = new Dictionary<AvatarState, AnimationInstance>()
        {
            { AvatarState.Normal,  new AnimationInstance(){ StartingFrame = 00, EndingFrame = 10, TimePerFrame = 1/10f, AnimationType = AnimationType.Cycle } },
        };

        public Vector2 Position;
        public Vector2 headBasePosition, bodyBasePosition;

        public Flipbook head;
        public Flipbook body;

        readonly Mobile mobile;
        readonly List<int[]> riderOffset;

#if DEBUG
        DebugCrosshair dc1 = new DebugCrosshair(Color.Blue);
        DebugCrosshair dc2 = new DebugCrosshair(Color.White);
#endif

        public Rider(Mobile mobile, Vector2 positionOffset)
        {
            this.mobile = mobile;
            head = new Flipbook(Vector2.Zero, new Vector2(25, 12), 38, 24, "Graphics/Avatar/Male/Head/Base", headAvatarState[AvatarState.Staring], 1, 0);
            body = new Flipbook(Vector2.Zero, new Vector2(22, 12), 44, 28, "Graphics/Avatar/Male/Body/Base", bodyAvatarState[AvatarState.Normal], 1, 0);
            headBasePosition = positionOffset + new Vector2(10, -17);
            bodyBasePosition = positionOffset;

            riderOffset = (List<int[]>)MetadataManager.ElementMetadata[$@"Mobile/{mobile.MobileType}/RiderPivot"];

#if DEBUG
            DebugHandler.Instance.Add(dc1);
            DebugHandler.Instance.Add(dc2);
#endif

            Update();
        }

        public void Hide()
        {
            head.Color = Color.Transparent;
            body.Color = Color.Transparent;
        }

        public void Flip()
        {
            head.Flip();
            body.Flip();
        }

        static int value = 0;

        public void Update()
        {
            float baseAngle = mobile.MobileFlipbook.Rotation;
            Vector2 basePosition = Vector2.One;

            if (mobile.Facing == Facing.Right)
                basePosition = new Vector2(-1, 1);

            value = mobile.MobileFlipbook.GetCurrentFrame();

            Vector2 basePos = new Vector2(riderOffset[value][0], riderOffset[value][1]);
            Vector2 headPos = Vector2.Transform((basePos + headBasePosition) * basePosition, Matrix.CreateRotationZ(baseAngle));
            Vector2 bodyPos = Vector2.Transform((basePos + bodyBasePosition) * basePosition, Matrix.CreateRotationZ(baseAngle));

#if DEBUG
            dc1.Update(mobile.MobileFlipbook.Position + headPos);
            dc2.Update(mobile.MobileFlipbook.Position + bodyPos);
#endif

            head.Position = mobile.MobileFlipbook.Position + headPos;
            head.Rotation = mobile.MobileFlipbook.Rotation;
            body.Position = mobile.MobileFlipbook.Position + bodyPos;
            body.Rotation = mobile.MobileFlipbook.Rotation;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            head.Draw(gameTime, spriteBatch);
            body.Draw(gameTime, spriteBatch);
            Console.WriteLine(value + " " + mobile.MobileFlipbook.GetCurrentFrame());
        }
    }
}
