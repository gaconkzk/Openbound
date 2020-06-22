﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Interface.General;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenBound.GameComponents.Interface.Text
{
    public class TextBox : IDisposable
    {
        //Text being rendered
        List<CompositeSpriteText> toBeAddedCompositeSpriteTextList;
        List<CompositeSpriteText> compositeSpriteTextList;

        ScrollBar scrollBar;
        Sprite background;

        //Box size constraints
        Vector2 boxTextArea;
        int maximumNumberOfLines;
        int maximumRenderableLines;

        //Text size constraint
        Vector2 elementYOffset;

        //Rendering/Updating variables
        int startingRenderingIndex;
        int finalRenderingIndex;

        //Selected element index
        int selectedIndex;

        //Text Field
        TextField textField;
        Sprite textfieldBackground;
        Vector2 textFieldOffset;
        Vector2? textFieldFixedPosition;

        //Transparency
        public float Transparency { get; set; }

        //Alignment
        public Alignment alignment;

        //Action Handlers
        public Action<PlayerMessage> OnSendMessage;

        //Asynchronous text handling thread objects
        Thread textHandlerThread;
        Queue<object> playerMessageQueue;
        bool isThreadEnabled;

        public string Text => textField.Text.Text;
        public bool IsTextFieldActive => textField == null ? false : textField.IsActive;
        public Dictionary<Keys, Action<object>> OnKeyPress => textField.OnPressKey;

        public TextBox(Vector2 position, Vector2 boxSize, int maximumNumberOfLines, float backgroundAlpha,
            bool hasScrollBar = true, float scrollBackgroundAlpha = 0.3f,
            bool hasTextField = true, Vector2 textFieldOffset = default, Vector2? textFieldFixedPosition = null, float textFieldBackground = 0.3f, int maximumTextLength = 50,
            Action<PlayerMessage> onSendMessage = default, Alignment alignment = Alignment.Left)
        {
            Transparency = 1;
            boxTextArea = boxSize;
            OnSendMessage = onSendMessage;
            this.maximumNumberOfLines = maximumNumberOfLines;
            this.textFieldOffset = textFieldOffset;
            this.textFieldFixedPosition = textFieldFixedPosition;
            this.alignment = alignment;

            if (hasScrollBar)
            {
                //Instance, disable, add a action handler to scroll bar and reduce the text area proportional to textbar size.
                scrollBar = new ScrollBar(position + new Vector2(boxSize.X, 0), boxSize, scrollBackgroundAlpha);
                boxTextArea -= new Vector2(scrollBar.ElementWidth, 0);
                scrollBar.Disable();
                scrollBar.InstallOnChangeAction(OnBeingDragged);
            }

            if (hasTextField)
            {
                textField = new TextField(default, (int)(boxSize.X - textFieldOffset.X * 2), 30, maximumTextLength, FontTextType.Consolas10, Color.White, layerDepth: DepthParameter.InterfaceButtonText, outlineColor: Color.Black);
                textField.OnPressKey[Keys.Enter] = SendMessage;

                Vector2 tfBGPosition;

                if (textFieldFixedPosition == default)
                    tfBGPosition = position + new Vector2(0, boxSize.Y) + new Vector2(0, textFieldOffset.Y);
                else
                    tfBGPosition = (Vector2)textFieldFixedPosition;
                
                textfieldBackground = new Sprite("Interface/TextBox/TextBoxBackground", tfBGPosition, layerDepth: DepthParameter.InterfaceButton);
                textfieldBackground.SetTransparency(textFieldBackground);
                textfieldBackground.Scale *= new Vector2(boxSize.X, 30);

                /* textfieldBackground = new Sprite("Interface/TextBox/TextBoxBackground", position + new Vector2(0, boxSize.Y) + new Vector2(0, textFieldOffset.Y), layerDepth: DepthParameter.InterfaceButton);
                textfieldBackground.SetTransparency(backgroundAlpha);
                textfieldBackground.Scale *= new Vector2(boxSize.X, 30);*/
            }

            background = new Sprite("Interface/TextBox/TextBoxBackground", position, layerDepth: DepthParameter.InterfaceButton);
            background.SetTransparency(backgroundAlpha);
            background.Scale *= boxSize;

            compositeSpriteTextList = new List<CompositeSpriteText>();
            toBeAddedCompositeSpriteTextList = new List<CompositeSpriteText>();
            playerMessageQueue = new Queue<object>();

            //Create text producer thread
            textHandlerThread = new Thread(TextProcessingThread);
            textHandlerThread.Name = "TextHandlerThread";
            textHandlerThread.IsBackground = true;
            textHandlerThread.Start();
            isThreadEnabled = true;
        }

        public void SetTextFieldColor(Color color, Color outlineColor)
        {
            textField.Text.Color = textField.Text.BaseColor = color;
            textField.Text.OutlineColor = textField.Text.OutlineBaseColor = outlineColor;
        }

        private void SendMessage(object textField)
        {
            if (Text.Length == 0) return;

            TextField t = (TextField)textField;

            OnSendMessage?.Invoke(new PlayerMessage() { Player = new Player() { Nickname = GameInformation.Instance.PlayerInformation.Nickname }, Text = t.Text.Text });
            
            t.ClearText();
        }

        /// <summary>
        /// Text processing thread producer. All the texts are produced by this thread and stored on the <see cref="toBeAddedCompositeSpriteTextList"/>
        /// for further screen integration via <see cref="UpdateReceivedTexts"/>.
        /// </summary>
        private void TextProcessingThread()
        {
            while (isThreadEnabled)
            {
                Thread.Sleep(10);
                lock (playerMessageQueue)
                {
                    if (playerMessageQueue.Count == 0) continue;

                    object text = playerMessageQueue.Dequeue();

                    List<CompositeSpriteText> cstList = null;

                    switch (text)
                    {
                        case PlayerMessage pm:
                            cstList = CompositeSpriteText.CreateChatMessage(pm, (int)boxTextArea.X, DepthParameter.InterfaceButtonText);
                            break;
                        case CustomMessage cm:
                            cstList = CompositeSpriteText.CreateCustomMessage(cm, (int)boxTextArea.X, DepthParameter.InterfaceButtonText);
                            break;
                        case List<CustomMessage> cmL:
                            cstList = CompositeSpriteText.CreateCustomMessage(cmL, DepthParameter.InterfaceButtonText);
                            break;
                    }

                    lock (toBeAddedCompositeSpriteTextList)
                        toBeAddedCompositeSpriteTextList.AddRange(cstList);
                }
            }
        }

        /// <summary>
        /// Generates a text addition request to the producer thread.
        /// </summary>
        public void AsyncAppendText(object message)
        {
            lock (playerMessageQueue)
            {
                playerMessageQueue.Enqueue(message);
            }
        }

        public void EnableTextField()
        {
            textField.Enable();
        }

        public void DisableTextField()
        {
            textField.Disable();
        }

        public void ActivateTextField()
        {
            textField.ActivateElement();
        }

        public void DeactivateTextField()
        {
            textField.DeactivateElement();
        }

        /// <summary>
        /// Appends a TextList into the texts being rendered.
        /// </summary>
        private void AppendText(List<CompositeSpriteText> compositeSpriteTextList)
        {
            //If it is the very first text presented in the box, calculate all possible element offsets that depends on a text instance
            if (maximumRenderableLines == 0)
            {
                elementYOffset = compositeSpriteTextList[0].ElementDimensions * Vector2.UnitY;
                maximumRenderableLines = (int)(boxTextArea.Y / elementYOffset.Y);
            }

            if (scrollBar != null)
            {
                this.compositeSpriteTextList.AddRange(compositeSpriteTextList);

                //If the number of text exceeds the maximum capacity, enables the scroll bar
                if (this.compositeSpriteTextList.Count >= maximumRenderableLines && !scrollBar.IsEnabled) scrollBar.Enable();

                //If the selected index is the last
                if (selectedIndex + compositeSpriteTextList.Count + 1 == this.compositeSpriteTextList.Count)
                    //Selects the last element
                    selectedIndex += compositeSpriteTextList.Count;
                else
                    //Keeps the same selected element and just updates the scroll bar to its new position.
                    scrollBar.SetScrollPercentagePosition((float)(selectedIndex + 1) / this.compositeSpriteTextList.Count);

                //If the elements exceeds the maximum number of lines, delete all the top elements at once and keep it whithin the maximum number of lines
                int excludedElements = Math.Max(this.compositeSpriteTextList.Count - maximumNumberOfLines, 0);
                selectedIndex = Math.Max(0, selectedIndex - excludedElements);
                this.compositeSpriteTextList.RemoveRange(0, excludedElements);
            }
            else
            {
                //Adds the text into the list and delete all non visible texts
                this.compositeSpriteTextList.AddRange(compositeSpriteTextList);
                this.compositeSpriteTextList.RemoveRange(0, Math.Max(this.compositeSpriteTextList.Count - maximumRenderableLines, 0));
            }
        }

        /// <summary>
        /// Called whenever the scroll bar is moving.
        /// </summary>
        /// <param name="button"></param>
        public void OnBeingDragged(object button)
        {
            selectedIndex = Math.Max((int)(scrollBar.NormalizedCurrentScrollPercentage * compositeSpriteTextList.Count) - 1, 0);
        }

        public void Update(GameTime gameTime)
        {
            scrollBar?.Update();
            background.UpdateAttatchmentPosition();
            UpdateReceivedTexts();
            UpdateTextPosition();
            UpdateTextField(gameTime);
        }

        public void UpdateTextField(GameTime gameTime)
        {
            if (textField == null) return;

            textField.Update(gameTime);

            //Textfield
            if (textFieldFixedPosition == null)
            {
                textfieldBackground.UpdateAttatchmentPosition();
                textField.Position = (background.Position + boxTextArea * Vector2.UnitY + textFieldOffset).ToIntegerDomain();
            }
            else
            {
                textfieldBackground.UpdateAttatchmentPosition();
                textField.Position = textfieldBackground.Position.ToIntegerDomain()
                    + textFieldOffset.ToIntegerDomain();
            }
        }

        /// <summary>
        /// Removes the new processed texts and adds em' to the renderable list
        /// </summary>
        private void UpdateReceivedTexts()
        {
            lock (toBeAddedCompositeSpriteTextList)
            {
                if (toBeAddedCompositeSpriteTextList.Count == 0) return;

                AppendText(toBeAddedCompositeSpriteTextList);
                toBeAddedCompositeSpriteTextList.Clear();
            }
        }

        /// <summary>
        /// Updates only the texts that are being rendered on screen.
        /// </summary>
        private void UpdateTextPosition()
        {
            if (compositeSpriteTextList.Count == 0) return;

            //Updating selected index
            startingRenderingIndex = Math.Max(selectedIndex - maximumRenderableLines + 1, 0);
            finalRenderingIndex = Math.Min(startingRenderingIndex + maximumRenderableLines, compositeSpriteTextList.Count);

            Vector2 startingPosition = background.Position;

            if (alignment == Alignment.Right)
                startingPosition = background.Position + boxTextArea * Vector2.UnitX;

            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++)
            {
                CompositeSpriteText cst = compositeSpriteTextList[i];
                cst.PositionOffset = startingPosition + (i - startingRenderingIndex) * elementYOffset;

                if (alignment == Alignment.Right)
                    cst.PositionOffset -= cst.ElementDimensions * Vector2.UnitX;

                cst.ResetTextColor();
                cst.Transparency = Transparency;
            }

            //Tint selected line
            if (scrollBar != null)
            {
                compositeSpriteTextList[selectedIndex].ReplaceTextColor(Color.White, Parameter.TextColorTextBoxSelectedMessage);
                compositeSpriteTextList[startingRenderingIndex].ReplaceTextColor(Color.White, Parameter.TextColorTextBoxSelectedMessage);
                compositeSpriteTextList[Math.Max(finalRenderingIndex - 1, 0)].ReplaceTextColor(Color.White, Parameter.TextColorTextBoxSelectedMessage);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            scrollBar?.Draw(null, spriteBatch);
            background.Draw(null, spriteBatch);

            //Draw only the necessary elements. The indexes are beingcalculated on UpdateTextPosition method.
            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++) compositeSpriteTextList[i].Draw(spriteBatch);

            textField?.Draw(spriteBatch);
            textfieldBackground?.Draw(null, spriteBatch);
        }

        public void Dispose()
        {
            isThreadEnabled = false;
            textField?.Dispose();
        }
    }
}
