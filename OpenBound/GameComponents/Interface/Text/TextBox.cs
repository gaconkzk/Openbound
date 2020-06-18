using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
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

        public TextBox(Vector2 position, Vector2 boxSize, float backgroundAlpha, float scrollBackgroundAlpha = 0.3f, bool hasScrollBar = true)
        {
            boxTextArea = boxSize;

            if (hasScrollBar)
            {
                scrollBar = new ScrollBar(position + new Vector2(boxSize.X, 0), boxSize, scrollBackgroundAlpha);
                boxTextArea -= new Vector2(scrollBar.ElementWidth, 0);
                scrollBar.Disable();
            }
            else
            {
                boxTextArea -= new Vector2(8, 0);
            }

            background = new Sprite("Interface/TextBox/TextBoxBackground", position, layerDepth: DepthParameter.InterfaceButton);
            background.SetTransparency(backgroundAlpha);
            background.Scale *= boxSize;

            compositeSpriteTextList = new List<CompositeSpriteText>();
        }

        public void AppendText(Player player, string text)
        {
            List<CompositeSpriteText> cst = CompositeSpriteText.CreateChatMessage(player, text, (int)boxTextArea.X, 1);
            
            if (maximumLines == 0)
            {
                elementYOffset = cst[0].ElementDimensions * Vector2.UnitY;
                maximumLines = (int)(boxTextArea.Y / elementYOffset.Y);
            }

            if (scrollBar != null)
            {
                float currentCont = GetSelectedIndex;

                compositeSpriteTextList.AddRange(cst);

                if (compositeSpriteTextList.Count >= maximumLines)
                    scrollBar.Enable();

                if (currentCont + 1 != compositeSpriteTextList.Count - 1)
                    scrollBar.SetScrollPercentagePosition((currentCont + 1) / compositeSpriteTextList.Count);
            }
            else
            {
                compositeSpriteTextList.AddRange(cst);
                compositeSpriteTextList.RemoveRange(0, Math.Max(compositeSpriteTextList.Count - maximumLines - 1, 0));
            }
        }

        public void Update()
        {
            scrollBar?.Update();
            background.UpdateAttatchmentPosition();
            UpdateTextPosition();
        }

        public void UpdateTextPosition()
        {
            if (compositeSpriteTextList.Count == 0) return;
            
            if (scrollBar != null)
                startingRenderingIndex = (int)Math.Max(scrollBar.NormalizedCurrentScrollPercentage * compositeSpriteTextList.Count - maximumLines, 0);
            finalRenderingIndex = Math.Min(startingRenderingIndex + maximumLines, compositeSpriteTextList.Count);

            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++)
            {
                CompositeSpriteText cst = compositeSpriteTextList[i];
                cst.PositionOffset = background.Position + (i - startingRenderingIndex) * elementYOffset;
                cst.ResetTextColor();
            }

            //Tint selected line
            if (scrollBar != null)
                compositeSpriteTextList[GetSelectedIndex].ReplaceTextColor(Color.White, Parameter.TextColorTextBoxSelectedMessage);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            scrollBar?.Draw(gameTime, spriteBatch);
            background.Draw(gameTime, spriteBatch);

            for (int i = startingRenderingIndex; i < finalRenderingIndex; i++)
            {
                compositeSpriteTextList[i].Draw(spriteBatch);
            }
        }
    }
}
