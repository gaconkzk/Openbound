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
using OpenBound.Common;
using OpenBound.GameComponents.Renderer;
using System;
using System.Collections.Generic;

namespace OpenBound.GameComponents.Animation
{
    public class Flipbook : Renderable
    {
        private List<AnimationInstance> AnimationCycle { get; set; }

        private int currentAnimatedInstanceIndex;
        private bool repeatAnimationCycle;
        private int framesPerLine;

        public List<FlipbookAnimation> FlipbookAnimationList { get; private set; }

        public AnimationInstance CurrentAnimationInstance => FlipbookAnimationList[currentAnimatedInstanceIndex].AnimationInstance;

        private Flipbook() : base() { }

        public static Flipbook CreateFlipbook(Vector2 Position, Vector2 Pivot,
            int SpriteWidth, int SpriteHeight, string Texture2DPath,
            List<AnimationInstance> AnimationCycle, bool RepeatAnimationCycle,
            float LayerDepth, float Rotation = 0)
        {
            Flipbook f = new Flipbook();

            f.SpriteWidth = SpriteWidth;
            f.SpriteHeight = SpriteHeight;
            f.Texture2DPath = Texture2DPath;
            f.Rotation = Rotation;
            f.AnimationCycle = AnimationCycle;

            f.Position = Position;
            f.Pivot = Pivot;

            f.currentAnimatedInstanceIndex = 0;
            f.repeatAnimationCycle = false;

            f.LayerDepth = LayerDepth;
            f.Effect = SpriteEffects.None;

            f.Texture2D = AssetHandler.Instance.RequestTexture(Texture2DPath);

            f.FlipbookAnimationList = new List<FlipbookAnimation>();

            f.CreateFlipbookInstances();

            f.framesPerLine = (int)(f.Texture2D.Width / SpriteWidth);

            return f;
        }

        public static Flipbook CreateFlipbook(Vector2 Position, Vector2 Pivot,
            int SpriteWidth, int SpriteHeight, string Texture2DPath,
            AnimationInstance Animation, bool RepeatAnimationCycle, float LayerDepth,
            float Rotation = 0)
        {
            List<AnimationInstance> lAnm = new List<AnimationInstance>();
            if (Animation != null) lAnm.Add(Animation);

            return CreateFlipbook(Position, Pivot, SpriteWidth, SpriteHeight, Texture2DPath,
                lAnm, RepeatAnimationCycle, LayerDepth, Rotation);
        }

        public void JumpToRandomAnimationFrame()
        {
            int nextFrameIndex = Parameter.Random.Next(CurrentAnimationInstance.StartingFrame, CurrentAnimationInstance.EndingFrame);
            SetCurrentFrame(nextFrameIndex);
        }

        public void Flip()
        {
            Effect = (Effect == SpriteEffects.None) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Rotation += (float)Math.PI;

            if (Rotation > 2 * (float)Math.PI)
                Rotation = Rotation % (2 * (float)Math.PI);

            Pivot = new Vector2(SpriteWidth - Pivot.X, Pivot.Y);

            Texture2D = AssetHandler.Instance.RequestTexture(Texture2DPath);
            SourceRectangle = new Rectangle((int)Position.X, (int)Position.Y, SpriteWidth, SpriteHeight);
        }

        private void CreateFlipbookInstances()
        {
            FlipbookAnimationList.Clear();
            foreach (AnimationInstance fbI in AnimationCycle)
                FlipbookAnimationList.Add(FlipbookAnimation.GetInstance(fbI));
        }

        public void AppendAnimationIntoCycle(AnimationInstance newAnimation, bool clearAnimationCycle = false)
        {
            if (clearAnimationCycle) AnimationCycle.Clear();
            AnimationCycle.Add(newAnimation);
            CreateFlipbookInstances();
        }

        public void AppendAnimationIntoCycle(List<AnimationInstance> newAnimationList, bool clearAnimationCycle = false)
        {
            if (clearAnimationCycle) AnimationCycle.Clear();
            AnimationCycle.AddRange(newAnimationList);
            CreateFlipbookInstances();
        }

        public void SetCurrentFrame(int newFrame) =>
            FlipbookAnimationList[currentAnimatedInstanceIndex].CurrentFrameIndex = newFrame;

        public int GetCurrentFrame() =>
            FlipbookAnimationList[currentAnimatedInstanceIndex].CurrentFrameIndex;

        public void ResetCurrentAnimation()
        {
            FlipbookAnimationList[currentAnimatedInstanceIndex].Reset();
        }

        public override void Draw(GameTime GameTime, SpriteBatch SpriteBatch)
        {
            FlipbookAnimation fbA = FlipbookAnimationList[currentAnimatedInstanceIndex];

            int currentFrameIndex = fbA.GetNextAnimationIndex(GameTime);

            if (fbA.IsLastFrame && AnimationCycle != null && AnimationCycle.Count > 1)
            {
                if (!repeatAnimationCycle)
                {
                    FlipbookAnimationList.Remove(fbA);
                    AnimationCycle.Remove(fbA.AnimationInstance);
                }

                currentAnimatedInstanceIndex = 0;
            }

            SourceRectangle = new Rectangle(currentFrameIndex % framesPerLine * SpriteWidth, currentFrameIndex / framesPerLine * SpriteHeight, SpriteWidth, SpriteHeight);
            base.Draw(GameTime, SpriteBatch);
        }
    }
}
