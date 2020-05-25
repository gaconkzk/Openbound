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
using OpenBound.GameComponents.Debug;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBound.GameComponents.Interface.Text
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public class CompositeSpriteText
    {
        private List<List<SpriteText>> spriteTextMatrix;
        private Vector2 elementOffset;
        private Vector2 position;
        private Vector2 positionOffset;
        private Alignment alignment;

        #region Properties
        public List<List<SpriteText>> SpriteTextMatrix
        {
            get => spriteTextMatrix;
            set
            {
                spriteTextMatrix = value;
                RecalculatePosition();
            }
        }

        public Vector2 ElementOffset
        {
            get => elementOffset;
            set
            {
                elementOffset = value;
                RecalculatePosition();
            }
        }

        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                RecalculatePosition();
            }
        }

        public Vector2 PositionOffset
        {
            get => positionOffset;
            set
            {
                positionOffset = value;
                UpdateAttatchedPosition();
                RecalculatePosition();
            }
        }

        public Vector2 ElementDimensions
        {
            get
            {
                Vector2 sum = Vector2.Zero;

                int sumX = 0, sumY = 0;
                int tmpSumX = 0, tmpSumY = 0;

                foreach (List<SpriteText> spriteTextList in spriteTextMatrix)
                {
                    foreach (SpriteText spriteText in spriteTextList)
                    {
                        Vector2 sentenceSize = spriteText.MeasureSize;

                        tmpSumX += (int)sentenceSize.X;
                        tmpSumY = (int)Math.Max(tmpSumY, sentenceSize.Y);
                    }

                    sumX += Math.Max(sumX, tmpSumX);
                    sumY += tmpSumY;
                }

                sumX += (int)(spriteTextMatrix.Max(x => x.Count) * elementOffset.X);
                sumY += (int)(spriteTextMatrix.Count * elementOffset.Y);

                return new Vector2(sumX, sumY);
            }
        }
        #endregion

        public static CompositeSpriteText CreateCompositeSpriteText(List<List<SpriteText>> spriteTextMatrix, Alignment alignment, Vector2 position, Vector2 elementOffset)
        {
            return new CompositeSpriteText(spriteTextMatrix, alignment, position, elementOffset);
        }

        public static CompositeSpriteText CreateCompositeSpriteText(List<SpriteText> spriteTextList, Orientation orientation, Alignment alignment, Vector2 position, float elementOffset = 0)
        {
            if (orientation == Orientation.Horizontal)
            {
                return new CompositeSpriteText(new List<List<SpriteText>>() { spriteTextList }, alignment, position, new Vector2(elementOffset, 0));
            }
            else
            {
                List<List<SpriteText>> stList = new List<List<SpriteText>>();
                spriteTextList.ForEach((x) => stList.Add(new List<SpriteText>() { x }));
                return new CompositeSpriteText(stList, alignment, position, new Vector2(0, elementOffset));
            }
        }

        private CompositeSpriteText(List<List<SpriteText>> spriteTextMatrix, Alignment alignment, Vector2 position, Vector2 elementOffset)
        {
            this.elementOffset = elementOffset;
            this.position = position;
            this.spriteTextMatrix = spriteTextMatrix;
            this.alignment = alignment;

            spriteTextMatrix.ForEach((x) => x.ForEach((y) => y.Alignment = alignment == Alignment.Center ? Alignment.Left : alignment));

            RecalculatePosition();

#if DEBUG
            DebugCrosshair dc = new DebugCrosshair(Color.Blue);
            dc.Update(this.position);
            DebugHandler.Instance.Add(dc);
#endif
        }

        public void UpdateAttatchedPosition()
        {
            position = positionOffset;
            spriteTextMatrix.ForEach((x) => x.ForEach((y) => y.UpdateAttatchedPosition()));
        }

        private void RecalculatePosition()
        {
            Vector2 newPosition = Position;

            if (alignment == Alignment.Center) newPosition -= new Vector2(ElementDimensions.X, 0) / 2;

            float offsetY = 0;

            foreach (List<SpriteText> spriteList in spriteTextMatrix)
            {
                float offsetX = 0;
                float maxY = 0;

                foreach (SpriteText spriteText in spriteList)
                {
                    Vector2 elemSize = spriteText.MeasureSize;

                    spriteText.Position = spriteText.PositionOffset = newPosition + new Vector2(offsetX, offsetY);
                    offsetX += elemSize.X + elementOffset.X;
                    maxY = Math.Max(maxY, elemSize.Y);
                }

                offsetY += maxY + elementOffset.Y;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteTextMatrix.ForEach((x) => x.ForEach((y) => y.Draw(spriteBatch)));
        }
    }
}
