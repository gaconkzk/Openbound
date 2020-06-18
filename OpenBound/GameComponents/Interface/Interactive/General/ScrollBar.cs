using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Interface.Interactive;
using OpenBound.GameComponents.Interface.Interactive.Misc;
using OpenBound.GameComponents.Level.Scene;
using System;

namespace OpenBound.GameComponents.Interface.General
{
    public class ScrollBar
    {
        Button scrollButtonUp, scrollButtonDown;
        Sprite scrollBackground;

        Sprite scrollBar;
        TransparentButton scrollBarTB;

        Vector2 position, boxSize;

        float maxScroll, minScroll, scrollSize;

        //Debug Variables
#if DEBUG
        DebugLine dL1 = new DebugLine(Color.Red);
        DebugLine dL2 = new DebugLine(Color.Red);
        DebugCrosshair d1 = new DebugCrosshair(Color.Blue);
        DebugCrosshair d2 = new DebugCrosshair(Color.Blue);
        DebugCrosshair d3 = new DebugCrosshair(Color.Blue);
#endif

        public int ElementWidth => scrollButtonUp.ButtonSprite.SourceRectangle.X + 5;

        public float NormalizedCurrentScrollPercentage { get; private set; }

        public ScrollBar(Vector2 position, Vector2 boxSize, float scrollBackgroundAlpha)
        {
            this.position = position;
            this.boxSize = boxSize;

            scrollButtonUp = new Button(ButtonType.ScrollBarUp, DepthParameter.InterfaceButtonAnimatedIcon, default, position);
            scrollButtonUp.ButtonOffset += new Vector2(0, scrollButtonUp.ButtonSprite.Pivot.Y - 1);
            Vector2 pivotOffset = scrollButtonUp.ButtonSprite.Pivot * Vector2.UnitX;
            scrollButtonUp.ButtonSprite.Pivot += pivotOffset;

            scrollButtonDown = new Button(ButtonType.ScrollBarDown, DepthParameter.InterfaceButtonAnimatedIcon, default, position + boxSize * Vector2.UnitY);
            scrollButtonDown.ButtonOffset -= new Vector2(0, scrollButtonDown.ButtonSprite.Pivot.Y - 1);
            scrollButtonDown.ButtonSprite.Pivot += pivotOffset;

            scrollBackground = new Sprite("Interface/TextBox/ScrollBarBackground", position + new Vector2(0, 1), DepthParameter.InterfaceButtonIcon);
            scrollBackground.Scale = new Vector2(1, boxSize.Y - 2);
            scrollBackground.Pivot += pivotOffset;
            scrollBackground.SetTransparency(scrollBackgroundAlpha);

            scrollBar = new Sprite("Interface/TextBox/ScrollBarBlock", scrollButtonDown.ButtonOffset, layerDepth: DepthParameter.InterfaceButtonAnimatedIcon, sourceRectangle: new Rectangle(0, 0, 25, 11));
            scrollBar.Pivot = new Vector2(12.5f, 5.5f) + pivotOffset;

            scrollBarTB = new TransparentButton((scrollButtonUp.ButtonOffset + scrollButtonDown.ButtonOffset) / 2, new Rectangle(0, 0, scrollButtonUp.ButtonSprite.SourceRectangle.Width, (int)boxSize.Y - scrollButtonUp.ButtonSprite.SourceRectangle.Height * 2 + 2));
            scrollBarTB.ButtonSprite.Pivot += scrollBarTB.ButtonSprite.Pivot * Vector2.UnitX;

            scrollBarTB.OnBeingDragged += (b) => { SetScrollPosition(Cursor.Instance.CurrentFlipbook.Position.Y + GameScene.Camera.CameraOffset.Y); };

#if DEBUG
            DebugHandler.Instance.Add(dL1);
            DebugHandler.Instance.Add(dL2);
            DebugHandler.Instance.Add(d1);
            DebugHandler.Instance.Add(d2);
            DebugHandler.Instance.Add(d3);
#endif

            minScroll = scrollButtonUp.ButtonOffset.Y + scrollButtonUp.ButtonSprite.Pivot.Y + 5;
            maxScroll = scrollButtonDown.ButtonOffset.Y - scrollButtonDown.ButtonSprite.Pivot.Y - 5;
            scrollSize = Math.Abs(minScroll) + Math.Abs(maxScroll);

            scrollButtonUp.OnBeingDragged += (b) => { UpdateScrollPosition(scrollSize / 100f); };
            scrollButtonDown.OnBeingDragged += (b) => { UpdateScrollPosition(-scrollSize / 100f); };

            UpdateScrollPosition(float.MaxValue);
            //Interface/TextBox/TextBoxBackground
        }

        public void SetScrollPercentagePosition(float newPosition)
        {
            scrollBar.PositionOffset = new Vector2(scrollBar.PositionOffset.X, Math.Min(minScroll, maxScroll) + scrollSize * newPosition);
            CalculateNormalizedPosition();
        }

        public void SetScrollPosition(float newPosition)
        {
            scrollBar.PositionOffset = new Vector2(scrollBar.PositionOffset.X, newPosition);
            CalculateNormalizedPosition();
        }

        public void UpdateScrollPosition(float newOffset)
        {
            scrollBar.PositionOffset -= new Vector2(0, newOffset);
            CalculateNormalizedPosition();
        }

        private void CalculateNormalizedPosition()
        {
            float min, max;

            if (minScroll > maxScroll)
            {
                max = minScroll;
                min = maxScroll;
            }
            else
            {
                max = maxScroll;
                min = minScroll;
            }

            scrollBar.PositionOffset = new Vector2(scrollBar.PositionOffset.X, MathHelper.Clamp(scrollBar.PositionOffset.Y, min, max));

            //Calculate the normalized (0..1) current scrolled percentage
            NormalizedCurrentScrollPercentage = (scrollBar.PositionOffset.Y - min) / (max - min);
        }

        public void Disable()
        {
            scrollButtonUp.Disable();
            scrollButtonDown.Disable();
            scrollBar.SetTransparency(0);
            scrollBarTB.Disable();
        }

        public void Enable()
        {
            scrollButtonUp.Enable();
            scrollButtonDown.Enable();
            scrollBar.SetTransparency(1);
            scrollBarTB.Enable();
        }

        public void Update()
        {
            scrollButtonUp.Update();
            scrollButtonDown.Update();

            scrollBackground.UpdateAttatchmentPosition();

            scrollBar.UpdateAttatchmentPosition();
            scrollBarTB.Update();

            //scrollBarTB.ButtonOffset = scrollBar.PositionOffset;

#if DEBUG
            dL1.Update(position - GameScene.Camera.CameraOffset, position + boxSize * Vector2.UnitY - GameScene.Camera.CameraOffset);
            dL2.Update(position - GameScene.Camera.CameraOffset + new Vector2(5, 0), position - GameScene.Camera.CameraOffset + new Vector2(5, 300));
            d1.Update(scrollButtonUp.ButtonSprite.Position);
            d2.Update(scrollButtonDown.ButtonSprite.Position);
            d3.Update(scrollBar.Position);
#endif
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            scrollButtonUp.Draw(gameTime, spriteBatch);
            scrollButtonDown.Draw(gameTime, spriteBatch);
            scrollBackground.Draw(null, spriteBatch);
            scrollBar.Draw(gameTime, spriteBatch);
        }
    }
}
