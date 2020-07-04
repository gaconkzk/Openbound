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
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Pawn;
using OpenBound.GameComponents.MobileAction;
using System;
using System.Collections.Generic;

namespace OpenBound.GameComponents.Level
{
    public enum CameraOperationMode
    {
        InGame,
        Menus,
    }

    public class Camera
    {
        public Matrix Transform { get; set; }
        public Vector2 Zoom { get; set; }
        public List<Renderable> TargetList { get; set; }

        private Vector2 cameraOffset;
        public Vector2 CameraOffset
        {
            get => cameraOffset;
            set => cameraOffset = new Vector2(
                MathHelper.Clamp(value.X, CameraMinPosition.X, CameraMaxPosition.X),
                MathHelper.Clamp(value.Y, CameraMinPosition.Y, CameraMaxPosition.Y));
        }

        private Cursor cursor;

        private Vector2 CameraMinPosition;
        private Vector2 CameraMaxPosition;

        public Vector2 ScreenCenterPosition { get; private set; }
        public Vector2 CameraReachableRange { get; private set; }

        public Vector2 TransformedScreenCenterPosition => -new Vector2(Transform.M41, Transform.M42) + Parameter.ScreenCenter;

        public object TrackedObject { get; private set; }

#if DEBUG
        DebugLine debugLine = new DebugLine(Color.Black);
#endif

        public Camera(GameScene gameScene)
        {
            cursor = Cursor.Instance;
            TargetList = new List<Renderable>();
            Transform = Matrix.CreateTranslation(0, 0, 0);
            CameraOffset = new Vector2(0, 0);
            Zoom = new Vector2(1, 1);

#if DEBUG
            //Debug
            DebugCrosshair[] debugCrosshair = new DebugCrosshair[8];

            for (int i = 0; i < debugCrosshair.Length; i++)
                debugCrosshair[i] = new DebugCrosshair(Color.Red);

            debugCrosshair[0].Update(new Vector2(-900, -900));
            debugCrosshair[1].Update(new Vector2(-900, 900 - Parameter.ScreenResolution.Y));
            debugCrosshair[2].Update(new Vector2(900 - Parameter.ScreenResolution.X, -900));
            debugCrosshair[3].Update(new Vector2(900 - Parameter.ScreenResolution.X, 900 - Parameter.ScreenResolution.Y));

            debugCrosshair[4].Update(new Vector2(0, 0) - new Vector2(900, 900));
            debugCrosshair[5].Update(new Vector2(0, 1800) - new Vector2(900, 900));
            debugCrosshair[6].Update(new Vector2(1800, 0) - new Vector2(900, 900));
            debugCrosshair[7].Update(new Vector2(1800, 1800) - new Vector2(900, 900));

            DebugHandler.Instance.AddRange(debugCrosshair);
            
            DebugHandler.Instance.Add(debugLine);
#endif
            AdjustAttackParameters(gameScene);
        }

        public void AdjustAttackParameters(GameScene scene)
        {
            //If Camera is InGame, the Min & Max positions should be set using the foreground
            int spriteWidth, spriteHeight;
            float newMinX, newMinY, newMaxX, newMaxY;
            if (GameInformation.Instance.GameState == GameState.InGame)
            {
                Zoom = new Vector2(1, 1);

                spriteWidth = ((LevelScene)scene).Foreground.SpriteWidth;
                spriteHeight = ((LevelScene)scene).Foreground.SpriteHeight;

                newMinX = Parameter.ScreenCenter.X - spriteWidth / 2;
                newMinY = Parameter.ScreenCenter.Y - spriteHeight / 2;

                newMaxX = -newMinX;
                newMaxY = -newMinY;
            }
            else
            {
                newMinX = newMaxX = -Parameter.ScreenCenter.X;
                newMinY = newMaxY = -Parameter.ScreenCenter.Y;

                Zoom = new Vector2(
                    (float)Parameter.ScreenResolution.X /
                    Parameter.MenuSupportedResolution.X,
                    (float)Parameter.ScreenResolution.Y /
                    Parameter.MenuSupportedResolution.Y);
            }

            CameraMinPosition = new Vector2(newMinX, newMinY);
            CameraMaxPosition = new Vector2(newMaxX, newMaxY);

            //This is used to calculate the stage parallax
            CameraReachableRange = CameraMaxPosition - CameraMinPosition;

            //Centering Camera on the middle of the arena
            //The Camera Sensibility vector bellow is a fix because the engine starts the mouse at 0,0
            ScreenCenterPosition = (CameraMaxPosition + CameraMinPosition) / 2;

            CameraOffset = ScreenCenterPosition;
            //- new Vector2(Parameter.CameraSensibility, Parameter.CameraSensibility);
        }

        public void Update()
        {
            //Update changes CameraOffset, must be called before the matrix calculation

            cursor.Update(this);

            UpdateTrackedObject();

            Transform =
                Matrix.CreateTranslation(new Vector3(CameraOffset.X, CameraOffset.Y, 0)) *
                Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                Matrix.CreateTranslation(new Vector3(Parameter.ScreenCenter.X, Parameter.ScreenCenter.Y, 0));
        }

        public void TrackObject(object trackedObject)
        {
            this.TrackedObject = trackedObject;

#if DEBUG
            debugLine.Sprite.ShowElement();
#endif

        }

        public void UpdateTrackedObject()
        {
            if (TrackedObject == null) return;

            Vector2 objectPosition = Vector2.Zero;

            if (TrackedObject is Projectile)
                objectPosition = ((Projectile)TrackedObject).Position;

            if (TrackedObject is Mobile)
                objectPosition = ((Mobile)TrackedObject).Position;

            Vector2 screenCenter = TransformedScreenCenterPosition;

            float cameraSpeed = (float)Helper.EuclideanDistance(screenCenter, objectPosition);
            double ang = Helper.AngleBetween(TransformedScreenCenterPosition, objectPosition);

#if DEBUG
            debugLine.Update(TransformedScreenCenterPosition, objectPosition);
#endif

            float factor = cameraSpeed / Parameter.CameraTrackingDampeningFactor;
            CameraOffset += new Vector2((int)(Math.Cos(ang) * factor), (int)(Math.Sin(ang) * factor));
        }

        public void CancelTracking()
        {
            TrackedObject = null;

#if DEBUG
            debugLine.Sprite.HideElement();
#endif
        }
    }
}
