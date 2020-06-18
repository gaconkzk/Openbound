using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Interface.General;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Interface.Text
{
    public class TextBox
    {
        List<CompositeSpriteText> compositeSpriteTextList;
        ScrollBar scrollBar;

        Sprite background;

        Vector2 boxTextArea, elementYOffset;
        int maximumLines;
        int startingRenderingIndex, finalRenderingIndex;

        protected int GetSelectedIndex => Math.Max(0, (int)(scrollBar.NormalizedCurrentScrollPercentage * compositeSpriteTextList.Count) - 1);

        public TextBox(Vector2 position, Vector2 boxSize)
        {
            scrollBar = new ScrollBar(position + new Vector2(boxSize.X, 0), boxSize);

            boxTextArea = boxSize - new Vector2(scrollBar.ElementWidth, 0);

            background = new Sprite("Interface/TextBox/TextBoxBackground", position, layerDepth: 0.7f);
            background.SetTransparency(0.3f);
            background.Scale *= boxSize;

            compositeSpriteTextList = new List<CompositeSpriteText>();
            scrollBar.Disable();
        }

        public void AppendText(Player player, string text)
        {
            //float previousPosition = elementYOffset.Y * compositeSpriteTextList.Count;
            //float 

            compositeSpriteTextList.AddRange(CompositeSpriteText.CreateChatMessage(player, text, (int)boxTextArea.X, 1));

            if (maximumLines == 0)
            {
                elementYOffset = compositeSpriteTextList[0].ElementDimensions * Vector2.UnitY;
                maximumLines = (int)(boxTextArea.Y / elementYOffset.Y);
            }
            else if (compositeSpriteTextList.Count >= maximumLines)
            {
                scrollBar.Enable();
            }

            //scrollBar.UpdateScrollPosition(scrollBar.NormalizedCurrentScrollPercentage - currentScroll);
        }

        public void Update()
        {
            scrollBar.Update();
            background.UpdateAttatchmentPosition();
            UpdateTextPosition();
        }

        public void UpdateTextPosition()
        {
            if (compositeSpriteTextList.Count == 0) return;

            startingRenderingIndex = (int)Math.Max(scrollBar.NormalizedCurrentScrollPercentage * compositeSpriteTextList.Count - maximumLines, 0);
            finalRenderingIndex = Math.Min(startingRenderingIndex + maximumLines, compositeSpriteTextList.Count);

            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++)
            {
                CompositeSpriteText cst = compositeSpriteTextList[i];
                cst.PositionOffset = background.Position + (i - startingRenderingIndex) * elementYOffset;
                cst.ResetTextColor();
            }

            //Tint selected line
            compositeSpriteTextList[GetSelectedIndex].ReplaceTextColor(Color.White, Color.Red);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            scrollBar.Draw(gameTime, spriteBatch);
            background.Draw(gameTime, spriteBatch);

            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++)
            {
                compositeSpriteTextList[i].Draw(spriteBatch);
            }
        }
    }
}
