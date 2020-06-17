using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Interface.General;
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

        public TextBox(Vector2 position, Vector2 boxSize)
        {
            scrollBar = new ScrollBar(position + new Vector2(boxSize.X, 0), boxSize);

            background = new Sprite("Interface/TextBox/TextBoxBackground", position, layerDepth: 0.7f);
            background.SetTransparency(0.3f);
            background.Scale *= boxSize;

            compositeSpriteTextList = new List<CompositeSpriteText>();
        }

        public void AppendText()
        {
             
        }

        public void Update()
        {
            scrollBar.Update();

            background.UpdateAttatchmentPosition();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            scrollBar.Draw(gameTime, spriteBatch);
            background.Draw(gameTime, spriteBatch);
        }
    }
}
