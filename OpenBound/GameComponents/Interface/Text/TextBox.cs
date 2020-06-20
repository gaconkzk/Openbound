using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenBound.Common;
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

        //Action Handlers
        public Action<PlayerMessage> OnSendMessage;

        //Asynchronous text handling thread objects
        Thread textHandlerThread;
        Queue<object> playerMessageQueue;
        bool isThreadEnabled;

        public TextBox(Vector2 position, Vector2 boxSize, int maximumNumberOfLines, float backgroundAlpha, float scrollBackgroundAlpha = 0.3f, bool hasScrollBar = true, Vector2 textFieldOffset = default, float textFieldBackground = 0.3f, bool hasTextField = true, Action<PlayerMessage> onSendMessage = default, int maximumTextLength = 50)
        {
            boxTextArea = boxSize;
            OnSendMessage = onSendMessage;
            this.maximumNumberOfLines = maximumNumberOfLines;
            this.textFieldOffset = textFieldOffset;
            
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
                textfieldBackground = new Sprite("Interface/TextBox/TextBoxBackground", position + new Vector2(0, boxSize.Y) + new Vector2(0, textFieldOffset.Y), layerDepth: DepthParameter.InterfaceButton);
                textfieldBackground.SetTransparency(backgroundAlpha);
                textfieldBackground.Scale *= new Vector2(boxSize.X, 30);
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
            textHandlerThread.Start();
            isThreadEnabled = true;
        }

        private void SendMessage(object textField)
        {
            TextField t = (TextField)textField;

            Player p = GameInformation.Instance.PlayerInformation;
            OnSendMessage(new PlayerMessage() { Player = new Player() { ID = p.ID, Nickname = p.Nickname }, Text = t.Text.Text });
            
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
                this.compositeSpriteTextList.RemoveRange(0, Math.Max(this.compositeSpriteTextList.Count - maximumRenderableLines - 1, 0));
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
            textField?.Update(gameTime);
            textfieldBackground?.UpdateAttatchmentPosition();
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
            //Textfield
            textField.Position = background.Position + boxTextArea * Vector2.UnitY + textFieldOffset;

            if (compositeSpriteTextList.Count == 0) return;

            //Updating selected index
            startingRenderingIndex = Math.Max(selectedIndex - maximumRenderableLines + 1, 0);
            finalRenderingIndex = Math.Min(startingRenderingIndex + maximumRenderableLines, compositeSpriteTextList.Count);

            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++)
            {
                CompositeSpriteText cst = compositeSpriteTextList[i];
                cst.PositionOffset = background.Position + (i - startingRenderingIndex) * elementYOffset;
                cst.ResetTextColor();
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
            textField.Dispose();
        }
    }
}
