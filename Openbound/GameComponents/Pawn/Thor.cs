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
using Microsoft.Xna.Framework.Input;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Input;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Level.Scene;
using System;
using System.Collections.Generic;

namespace OpenBound.GameComponents.Pawn
{
    public class Thor
    {
        static Dictionary<ActorFlipbookState, AnimationInstance> thorStatePresets = new Dictionary<ActorFlipbookState, AnimationInstance>()
        {
            { ActorFlipbookState.Stand,      new AnimationInstance(){ StartingFrame = 00, EndingFrame = 19, TimePerFrame = 1/15f } },
            { ActorFlipbookState.Active,     new AnimationInstance(){ StartingFrame = 20, EndingFrame = 39, TimePerFrame = 1/15f } },
            { ActorFlipbookState.ShootingS1, new AnimationInstance(){ StartingFrame = 40, EndingFrame = 55, TimePerFrame = 1/15f } },
        };

        Flipbook flipbook;
        Vector2 cannonOffset;

        //Movement Animation
        Vector2 position;
        Vector2 positionOffset;
        Vector2 cannonPosition;
        float elapsedTime1, elapsedTime2;

        //Debug
        DebugCrosshair dc;

        public Thor(Vector2 position)
        {
            this.position = position;

            positionOffset = new Vector2(3, 0);
            cannonOffset = new Vector2(50, 0);

            flipbook = Flipbook.CreateFlipbook(position, new Vector2(118, 111), 197, 190, "Graphics/Entity/Thor/Spritesheet", thorStatePresets[ActorFlipbookState.Stand], false, DepthParameter.Mobile, 0);

            dc = new DebugCrosshair(Color.Cyan);
            DebugHandler.Instance.Add(dc);
        }

        public void Shot()
        {
            flipbook.AppendAnimationIntoCycle(thorStatePresets[ActorFlipbookState.ShootingS1], true);
            flipbook.AppendAnimationIntoCycle(thorStatePresets[ActorFlipbookState.Active]);
        }

        public void Activate()
        {
            flipbook.AppendAnimationIntoCycle(thorStatePresets[ActorFlipbookState.Active], true);
        }

        public void Deactivate()
        {
            flipbook.AppendAnimationIntoCycle(thorStatePresets[ActorFlipbookState.Stand], true);
        }

        public void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            UpdateThorRotation(gameTime);

            flipbook.Rotation = (float)Helper.AngleBetween(Cursor.Instance.CurrentFlipbook.Position, position);
            if (InputHandler.IsBeingReleased(MKeys.Left))
            {
                Shot();
                SpecialEffectBuilder.ThorShot((cannonPosition + Cursor.Instance.CurrentFlipbook.Position) / 2, Parameter.NeonGreen, (float)Helper.EuclideanDistance(cannonPosition, Cursor.Instance.CurrentFlipbook.Position) / 256, (float)Helper.AngleBetween(flipbook.Position, Cursor.Instance.CurrentFlipbook.Position) - MathHelper.PiOver2);
            }
        }

        private void UpdateThorRotation(GameTime gameTime)
        {
            dc.Update(cannonPosition = position + Vector2.Transform(cannonOffset, Matrix.CreateRotationZ(flipbook.Rotation)));
        }

        private void UpdatePosition(GameTime gameTime)
        {
            elapsedTime1 += (float)gameTime.ElapsedGameTime.TotalSeconds * MathHelper.Pi / 4;
            elapsedTime2 = elapsedTime1 / 2;

            Vector2 positionOffset = Vector2.Transform(this.positionOffset, Matrix.CreateRotationZ(elapsedTime1 / MathHelper.Pi));

            MathHelper.WrapAngle(elapsedTime1);

            flipbook.Position = position + new Vector2(positionOffset.X * (float)Math.Sin(elapsedTime1), positionOffset.Y * (float)Math.Cos(elapsedTime2));
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            flipbook.Draw(gameTime, spriteBatch);
        }
    }
}
