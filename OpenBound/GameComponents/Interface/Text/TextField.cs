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
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.Interface.Text
{
    public class TextField
    {
        public Vector2 position { get; set; }
        public Vector2 Position
        {
            get => position;
            set { UpdatePostion(value); }
        }

        public SpriteText Text { get; private set; }
        private SpriteText textPointer;

        //Control
        public bool IsActive { get; set; }
        public bool IsEnabled { get; private set; }

        //Collision Box preset
        private int boxWidth, boxHeight;
        private Rectangle collisionRectangle;

        //Maximum character
        private readonly int maximumTextLength;

        //Flow Control
        private float textPointerBlinkingTime;
        private float keyInputTimer;
        private int textPointerLocation;

        //Tabing Index
        public List<TextField> TabIndex;

        //Event
        Action<object> OnActive { get; set; }

#if DEBUG
        DebugRectangle debugRectangle;
#endif

        public TextField(Vector2 positionOffset, int boxWidth, int boxHeight, int maximumTextLength, FontTextType fontTextType, Color color, float layerDepth, Color outlineColor = default)
        {
            this.maximumTextLength = maximumTextLength;
            this.boxHeight = boxHeight;
            this.boxWidth = boxWidth;

            textPointerBlinkingTime = 0;
            keyInputTimer = 0;
            textPointerLocation = 0;

            //Text
            Text = new SpriteText(fontTextType, "", color, Alignment.Left, layerDepth, default, outlineColor);
            textPointer = new SpriteText(fontTextType, "|", Parameter.NameplateGuildColor, Alignment.Left, layerDepth + 0.01f, default, Parameter.NameplateGuildOutlineColor);

            IsEnabled = true;

#if DEBUG
            debugRectangle = new DebugRectangle(Color.CornflowerBlue);
            DebugHandler.Instance.Add(debugRectangle);
#endif
        }

        public void UpdatePostion(Vector2 positionOffset)
        {
            position = positionOffset;

            collisionRectangle = new Rectangle((int)position.X, (int)position.Y, boxWidth, boxHeight);
            Text.Position = position;

#if DEBUG
            debugRectangle.Update(
                new Vector2[]
                {
                    new Vector2(collisionRectangle.Left, collisionRectangle.Top),
                    new Vector2(collisionRectangle.Left, collisionRectangle.Bottom),
                    new Vector2(collisionRectangle.Right, collisionRectangle.Bottom),
                    new Vector2(collisionRectangle.Right, collisionRectangle.Top),
                });
#endif
        }

        public void Update(GameTime gameTime)
        {
            if (!IsEnabled) return;

            CheckMouseIntersection();
            UpdateBlinkingBar(gameTime);
            UpdateActivatedTextInputs(gameTime);
        }

        private void CheckMouseIntersection()
        {
            Rectangle mouseRect = new Rectangle(
             (int)Cursor.Instance.CurrentFlipbook.Position.X,
             (int)Cursor.Instance.CurrentFlipbook.Position.Y, 1, 1);

            if (mouseRect.Intersects(collisionRectangle) && InputHandler.IsBeingReleased(MKeys.Left))
            {
                ActivateElement();
            }
        }

        private void ActivateElement()
        {
            if (TabIndex != null)
            {
                TabIndex.ForEach((x) =>
                {
                    x.IsActive = false;
                    Game1.Instance.Window.TextInput -= x.GetKeyboardInput;
                });
            }
            else
            {
                Game1.Instance.Window.TextInput -= GetKeyboardInput;
            }

            IsActive = true;
            OnActive?.Invoke(this);
            Game1.Instance.Window.TextInput += GetKeyboardInput;
        }

        private void UpdateActivatedTextInputs(GameTime gameTime)
        {
            if (!IsActive) return;

            List<Keys> pressedKeys = InputHandler.PreviousKBState.GetPressedKeys()
                .Intersect(InputHandler.CurrentKBState.GetPressedKeys())
                .ToList();

            if (pressedKeys.Count == 0)
            {
                keyInputTimer = 0;
                return;
            }

            keyInputTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyInputTimer > 0) return;

            pressedKeys.ForEach((key) =>
            {
                if (key == Keys.Left)
                {
                    textPointerLocation--;
                    if (textPointerLocation < 0) textPointerLocation = 0;
                }
                else if (key == Keys.Right)
                {
                    textPointerLocation++;
                    if (textPointerLocation > Text.Text.Length) textPointerLocation = Text.Text.Length;
                }
                else if (key == Keys.Delete)
                {
                    if (textPointerLocation + 1 > Text.Text.Length) return;

                    string before = Text.Text.Substring(0, textPointerLocation);
                    string after = Text.Text.Substring(textPointerLocation + 1, Text.Text.Length - textPointerLocation - 1);

                    Text.Text = before + after;
                }
                else if (key == Keys.Home)
                {
                    textPointerLocation = 0;
                }
                else if (key == Keys.End)
                {
                    textPointerLocation = Text.Text.Length;
                }
            });

            textPointerBlinkingTime = 0;
            keyInputTimer = 0.10f;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        private void UpdateBlinkingBar(GameTime gameTime)
        {
            Vector2 blinkingMeasured = textPointer.SpriteFont.MeasureString(textPointer.Text);
            Vector2 textPointerMeasured = Text.SpriteFont.MeasureString(Text.Text.Substring(0, textPointerLocation));

            textPointer.Position = Position + new Vector2(textPointerMeasured.X - blinkingMeasured.X / 2, 0);

            textPointerBlinkingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            textPointerBlinkingTime = textPointerBlinkingTime % 1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Text.Draw(spriteBatch);
            if (IsActive && textPointerBlinkingTime < 0.5) textPointer.Draw(spriteBatch);
        }

        private void GetKeyboardInput(object gameWindow, TextInputEventArgs pressedKey)
        {
            if (!IsActive) return;
            //Backspace
            if (pressedKey.Key == Keys.Back)
            {
                if (textPointerLocation == 0) return;

                string before = Text.Text.Substring(0, textPointerLocation - 1);
                string after = Text.Text.Substring(textPointerLocation, Text.Text.Length - textPointerLocation);

                Text.Text = before + after;
                textPointerLocation--;
            }
            else if (pressedKey.Key == Keys.Tab)
            {
                if (TabIndex == null) return;

                int currentIndex = TabIndex.IndexOf(this);
                currentIndex++;

                if (currentIndex > TabIndex.Count - 1)
                    currentIndex = 0;

                Game1.Instance.Window.TextInput -= GetKeyboardInput;
                TabIndex[currentIndex].ActivateElement();
                IsActive = false;
            }
            else if (pressedKey.Character < 32 || pressedKey.Character > 254)
            {
                return;
            }
            else
            {
                if (Text.Text.Length > maximumTextLength) return;

                string before = Text.Text.Substring(0, textPointerLocation);
                string after = Text.Text.Substring(textPointerLocation, Text.Text.Length - textPointerLocation);
                Text.Text = before + pressedKey.Character + after;
                textPointerLocation++;
            }
        }
    }
}
