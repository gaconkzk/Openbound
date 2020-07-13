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
using Openbound_Network_Object_Library.Models;
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
        public Vector2 Position;
        public Vector2 headBasePosition, bodyBasePosition;

        public Avatar Head;
        public Avatar Body;

        readonly Mobile mobile;
        readonly List<int[]> riderOffset;

#if DEBUG
        DebugCrosshair dc1 = new DebugCrosshair(Color.Blue);
        DebugCrosshair dc2 = new DebugCrosshair(Color.White);
#endif

        //Used on Avatar shop. No updates are supported for variables instanced with this constructor.
        public Rider(Facing facing, Player player, Vector2 positionOffset)
        {
            List<AvatarMetadata> headMetadata = (List<AvatarMetadata>)MetadataManager.ElementMetadata[$@"Avatar/{player.CharacterGender}/{AvatarCategory.Head}/Metadata"];
            List<AvatarMetadata> bodyMetadata = (List<AvatarMetadata>)MetadataManager.ElementMetadata[$@"Avatar/{player.CharacterGender}/{AvatarCategory.Body}/Metadata"];

            Head = new Avatar(headMetadata.Find((x) => x.ID == player.EquippedAvatarHead));
            Body = new Avatar(bodyMetadata.Find((x) => x.ID == player.EquippedAvatarBody));

            Head.Position = positionOffset + new Vector2(((facing == Facing.Right) ? -1 : 1) * 10, -17);
            Body.Position = positionOffset;

            if (facing == Facing.Right) Flip();
        }

        public Rider(Mobile mobile, Vector2 positionOffset)
        {
            this.mobile = mobile;

            List<AvatarMetadata> headMetadata = (List<AvatarMetadata>)MetadataManager.ElementMetadata[$@"Avatar/{mobile.Owner.CharacterGender}/{AvatarCategory.Head}/Metadata"];
            List<AvatarMetadata> bodyMetadata = (List<AvatarMetadata>)MetadataManager.ElementMetadata[$@"Avatar/{mobile.Owner.CharacterGender}/{AvatarCategory.Body}/Metadata"];

            Head = new Avatar(headMetadata.Find((x) => x.ID == mobile.Owner.EquippedAvatarHead));
            Body = new Avatar(bodyMetadata.Find((x) => x.ID == mobile.Owner.EquippedAvatarBody));

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
            Head.Hide();
            Body.Hide();
        }

        public void Flip()
        {
            Head.Flip();
            Body.Flip();
        }

        public void ReplaceAvatar(AvatarMetadata avatarMetadata)
        {
            Avatar avatar = new Avatar(avatarMetadata);
            Avatar previousAvatar = null;

            switch (avatarMetadata.Category)
            {
                case AvatarCategory.Head:
                    previousAvatar = Head;
                    Head = avatar;
                    break;
                case AvatarCategory.Body:
                    previousAvatar = Body;
                    Body = avatar;
                    break;
            }

            avatar.Position = previousAvatar.Position;

            if (avatar.Flipbook.Effect != previousAvatar.Flipbook.Effect)
                avatar.Flip();

            ResetCurrentAnimation();
        }

        public void Update()
        {
            float baseAngle = mobile.MobileFlipbook.Rotation;
            Vector2 basePosition = Vector2.One;

            if (mobile.Facing == Facing.Right)
                basePosition = new Vector2(-1, 1);

            int value = mobile.MobileFlipbook.GetCurrentFrame();
            Vector2 basePos = new Vector2(riderOffset[value][0], riderOffset[value][1]);
            Vector2 headPos = Vector2.Transform((basePos + headBasePosition) * basePosition, Matrix.CreateRotationZ(baseAngle));
            Vector2 bodyPos = Vector2.Transform((basePos + bodyBasePosition) * basePosition, Matrix.CreateRotationZ(baseAngle));

#if DEBUG
            dc1.Update(mobile.MobileFlipbook.Position + headPos);
            dc2.Update(mobile.MobileFlipbook.Position + bodyPos);
#endif

            Head.Position = mobile.MobileFlipbook.Position + headPos;
            Head.Rotation = mobile.MobileFlipbook.Rotation;
            Body.Position = mobile.MobileFlipbook.Position + bodyPos;
            Body.Rotation = mobile.MobileFlipbook.Rotation;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Head.Draw(gameTime, spriteBatch);
            Body.Draw(gameTime, spriteBatch);
        }

        internal void ResetCurrentAnimation()
        {
            Head.Flipbook.ResetCurrentAnimation();
            Body.Flipbook.ResetCurrentAnimation();
        }
    }
}
