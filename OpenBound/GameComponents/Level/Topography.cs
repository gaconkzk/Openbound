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
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;

namespace OpenBound.GameComponents.Level
{
    public class Topography
    {
        public static bool[][] CollidableForegroundMatrix { get; protected set; }
        private static Color[][] StageMatrix;

        public static int MapHeight => CollidableForegroundMatrix.Length;
        public static int MapWidth => CollidableForegroundMatrix[0].Length;

        //Object Reference
        private static Sprite foreground;

        /// <summary>
        /// This method initializes the topography class loading all informations about the terrain
        /// collidable spots. Must be initialized on a scene that contains the foreground sprite
        /// (playable map area).
        /// </summary>
        /// <param name="Foreground"></param>
        public static void Initialize(Sprite Foreground)
        {
            foreground = Foreground;
            ExtractTerrainInformation();
        }

        /// <summary>
        /// Erodes the scenario on a determined circular position with a determined radius.
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Radius"></param>
        /// <returns>The number of removed pixels</returns>
        public static int CreateErosion(Vector2 Position, float Radius)
        {
            int[] pos = GetRelativePosition(Position);

            float blastRadius = Radius + Parameter.BlastBlackmaskRadius;

            int startingIndexX = MathHelper.Clamp(pos[0] - (int)blastRadius, 0, StageMatrix[0].Length);
            int endingIndexX = MathHelper.Clamp(pos[0] + (int)blastRadius + 1, 0, StageMatrix[0].Length);

            int startingIndexY = MathHelper.Clamp(pos[1] - (int)blastRadius, 0, StageMatrix.Length);
            int endingIndexY = MathHelper.Clamp(pos[1] + (int)blastRadius + 1, 0, StageMatrix.Length);

            int numberOfRemovedPixels = 0;

            for (int h = startingIndexY; h < endingIndexY; h++)
            {
                for(int w = startingIndexX; w < endingIndexX; w++)
                {
                    //Using Squared euclidean to prevent SQRT method
                    float calculatedDistance = Helper.SquaredEuclideanDistance(pos[0], pos[1], w, h);

                    //If it the explosion is able to generate black mask
                    if (calculatedDistance <= blastRadius * blastRadius)
                    {
                        //If the explosion is able to open holes
                        if (calculatedDistance <= Radius * Radius)
                        {
                            numberOfRemovedPixels++;
                            StageMatrix[h][w] = Color.Transparent;
                            CollidableForegroundMatrix[h][w] = false;
                        }
                        else
                        {
                            StageMatrix[h][w] = StageMatrix[h][w].MultiplyByFactor(Parameter.BlastBlackmaskExplosionRadiusColorFactor);
                        }
                    }
                }
            }

            //Change the scenario to a 1D matrix and set the texture data. Ideally there should be no
            //2D matrix at all, but since this method runs pretty quick i see no problem in leaving it like that
            //fror now
            //TODO: Fix This
            foreground.Texture2D.SetData(StageMatrix.ConvertTo1D());

            return numberOfRemovedPixels;
        }

        /// <summary>
        /// Extracts the terrain information and initializes all static variables.
        /// </summary>
        private static void ExtractTerrainInformation()
        {
            Color[] color1D = new Color[foreground.Texture2D.Width * foreground.Texture2D.Height];
            foreground.Texture2D.GetData(color1D);

            StageMatrix = new Color[foreground.Texture2D.Height][];
            CollidableForegroundMatrix = new bool[foreground.Texture2D.Height][];

            for (int h = 0, index1D = 0; h < foreground.Texture2D.Height; h++)
            {
                StageMatrix[h] = new Color[foreground.Texture2D.Width];
                CollidableForegroundMatrix[h] = new bool[foreground.Texture2D.Width];
                for (int w = 0; w < foreground.Texture2D.Width; w++)
                {
                    Color tmp = color1D[index1D];
                    index1D++;
                    StageMatrix[h][w] = tmp;
                    CollidableForegroundMatrix[h][w] = tmp.A > 0;
                }
            }
        }

        public static int[] GetRelativePosition(Vector2 Position)
        {
            return new int[] { (int)Position.X + CollidableForegroundMatrix[0].Length / 2, (int)Position.Y + CollidableForegroundMatrix.Length / 2 };
        }

        public static int[] GetTransformedPosition(Vector2 Position)
        {
            return new int[] { (int)Position.X - CollidableForegroundMatrix[0].Length / 2, (int)Position.Y - CollidableForegroundMatrix.Length / 2 };
        }

        public static bool IsInsideMapBoundaries(Vector2 Position) => !IsNotInsideMapBoundaries(Position);

        public static bool IsNotInsideMapBoundaries(Vector2 Position)
        {
            //The map boundaires allows the projectiles to exceed its Y position on values
            //lower than the limit 0.

            int[] relPos = GetRelativePosition(Position);
            return relPos[1] >= StageMatrix.Length || relPos[1] < Parameter.ProjectilePlayableMapAreaYLimit || relPos[0] < 0 || relPos[0] >= StageMatrix[0].Length;
        }

        public static bool IsNotInsideMapYBoundaries(Vector2 Position)
        {
            int[] relPos = GetRelativePosition(Position);
            return relPos[1] >= StageMatrix.Length || relPos[1] < Parameter.ProjectilePlayableMapAreaYLimit;
        }

        public static bool CheckCollision(Vector2 Position)
        {
            int[] relPos = GetRelativePosition(Position);

            //if the position is above the stage limit, its collision must not be tested
            //in order to avoid detect collision in a index out of bounds
            if (relPos[1] < 0) return false;

            return CollidableForegroundMatrix[relPos[1]][relPos[0]];
        }
    }
}
