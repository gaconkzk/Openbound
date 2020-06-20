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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Input;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Interface.Popup;
using System;

namespace OpenBound.GameComponents.Level.Scene
{
    public abstract class GameScene : IDisposable
    {
        public static Camera Camera;
        protected GraphicsDevice graphicsDevice;
        protected SpriteBatch spriteBatch;

        public Sprite Background { get; protected set; }
        public Vector2 BackgroundOffset { get; set; }

        public GameScene()
        {
            GameInformation.Instance.GameState = GameState.Menu;

            if (Camera == null)
                Camera = new Camera(this);

            PopupHandler.Initialize();
        }

        public virtual void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Camera.AdjustAttackParameters(this);

            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
        }

        public virtual void Update(GameTime gameTime)
        {
            Camera.Update();

            if (InputHandler.IsCKDown(Keys.LeftAlt) && InputHandler.IsCKDown(Keys.F4) &&
                !(InputHandler.IsPKUp(Keys.LeftAlt) && InputHandler.IsPKUp(Keys.F4)))
            {
                OnTryClosingGame();
            }

            PopupHandler.Update(gameTime);
        }

        public virtual void OnSceneIsActive()
        {

        }

        public virtual void OnTryClosingGame()
        {
            SceneHandler.Instance.CloseGame();
        }

        public void BeginDraw()
        {
            spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, transformMatrix: Camera.Transform);
        }

        public virtual void Draw(GameTime gameTime)
        {
            Cursor.Instance.Draw(gameTime, spriteBatch);
            Background.Draw(gameTime, spriteBatch);
            PopupHandler.Draw(gameTime, spriteBatch);
        }

        public void EndDraw()
        {
            spriteBatch.End();
        }

        public virtual void Dispose() { }
    }
}
