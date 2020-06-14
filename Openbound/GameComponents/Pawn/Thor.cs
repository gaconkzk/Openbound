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
        Vector2 position, positionMovement;
        float movementElapsedTime;

        //Cannon position
        Vector2 cannonPosition;

        //Oscilation movement
        Vector2 oscilatingPositionOffset;
        float oscilatingElapsedTime1, oscilatingElapsedTime2;

        //Numeric fields
        Sprite levelText;
        NumericSpriteFont levelSpriteFont, experienceSpriteFont;

        //Debug
        DebugCrosshair dc;

        public Thor(Vector2 position)
        {
            this.position = position;

            oscilatingPositionOffset = new Vector2(3, 0);
            cannonOffset = new Vector2(50, 0);

            flipbook = Flipbook.CreateFlipbook(position, new Vector2(118, 111), 197, 190, "Graphics/Entity/Thor/Spritesheet", thorStatePresets[ActorFlipbookState.Stand], false, DepthParameter.Mobile, 0);

            levelSpriteFont      = new NumericSpriteFont(FontType.HUDBlueThorLevelIndicator,      1, DepthParameter.MobileSatellite, TextAnchor: TextAnchor.Right, AttachToCamera: false, StartingValue: 1);
            experienceSpriteFont = new NumericSpriteFont(FontType.HUDBlueThorExperienceIndicator, 5, DepthParameter.MobileSatellite, TextAnchor: TextAnchor.Right, AttachToCamera: false);

            levelText = new Sprite("Interface/Spritefont/HUD/Blue/ThorLevelLV", Vector2.Zero, layerDepth: DepthParameter.MobileSatellite);
            levelText.Pivot = Vector2.Zero;

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

            flipbook.Rotation = (float)Helper.AngleBetween(Cursor.Instance.CurrentFlipbook.Position, flipbook.Position);
            if (InputHandler.IsBeingReleased(MKeys.Left))
            {
                Shot();
                SpecialEffectBuilder.ThorShot((cannonPosition + Cursor.Instance.CurrentFlipbook.Position) / 2, Parameter.NeonGreen, (float)Helper.EuclideanDistance(cannonPosition, Cursor.Instance.CurrentFlipbook.Position) / 256, (float)Helper.AngleBetween(cannonPosition, Cursor.Instance.CurrentFlipbook.Position) - MathHelper.PiOver2);
            }
        }

        public void SetPosition(Vector2 newPosition)
        {
            positionMovement = newPosition - position;
            movementElapsedTime = 0;
        }

        private void UpdateThorRotation(GameTime gameTime)
        {
            dc.Update(
                cannonPosition = position + positionMovement + 
                Vector2.Transform(
                    cannonOffset, 
                    Matrix.CreateRotationZ(
                        (float)Helper.AngleBetween(Cursor.Instance.CurrentFlipbook.Position, position + positionMovement)
                        )
                    )
                );
        }

        private void UpdatePosition(GameTime gameTime)
        {
            //Updating oscilating movement
            oscilatingElapsedTime1 += (float)gameTime.ElapsedGameTime.TotalSeconds * MathHelper.Pi / 4;
            oscilatingElapsedTime2 = oscilatingElapsedTime1 / 2;

            MathHelper.WrapAngle(oscilatingElapsedTime1);

            Vector2 tmpOscilatingPosition = Vector2.Transform(oscilatingPositionOffset, Matrix.CreateRotationZ(oscilatingElapsedTime1 / MathHelper.Pi)) * new Vector2((float)Math.Sin(oscilatingElapsedTime1), (float)Math.Cos(oscilatingElapsedTime2));

            //Update movement position
            Vector2 tmpPositionOffset = Vector2.Zero;
            if (positionMovement != Vector2.Zero)
            {
                movementElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds * MathHelper.PiOver2;

                tmpPositionOffset =  positionMovement * (float)Math.Sin(movementElapsedTime);
                
                if (movementElapsedTime >= MathHelper.PiOver2)
                {
                    position += positionMovement;
                    positionMovement = Vector2.Zero;
                    flipbook.Position = position + tmpOscilatingPosition;
                    return;
                }
            }

            flipbook.Position = position + tmpOscilatingPosition + tmpPositionOffset;

            //Update texts
            levelSpriteFont.Position = flipbook.Position - tmpOscilatingPosition + new Vector2(50, 30);
            experienceSpriteFont.Position = flipbook.Position - tmpOscilatingPosition + new Vector2(50, 45);
            levelText.Position = levelSpriteFont.Position - new Vector2(30, -1);
            levelSpriteFont.Update(gameTime);
            experienceSpriteFont.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            flipbook.Draw(gameTime, spriteBatch);
            levelSpriteFont.Draw(gameTime, spriteBatch);
            experienceSpriteFont.Draw(gameTime, spriteBatch);
            levelText.Draw(null, spriteBatch);
        }
    }
}
