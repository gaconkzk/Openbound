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

#if DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace OpenBound.GameComponents.Debug
{
    class DebugRectangle : DebugElement
    {
        List<DebugLine> dLineList = new List<DebugLine>();

        public DebugRectangle(Color Color)
        {
            dLineList = new List<DebugLine>()
            {
                new DebugLine(Color),
                new DebugLine(Color),
                new DebugLine(Color),
                new DebugLine(Color),
            };
        }

        public void SetColor(Color Color)
        {
            dLineList.ForEach((x) => x.Sprite.Color = Color);
        }

        public override void Draw(SpriteBatch SpriteBatch)
        {
            dLineList.ForEach((x) => x.Draw(SpriteBatch));
        }

        public void Update(Vector2[] Corner)
        {
            dLineList[0].Update(Corner[0], Corner[1]);
            dLineList[1].Update(Corner[1], Corner[2]);
            dLineList[2].Update(Corner[2], Corner[3]);
            dLineList[3].Update(Corner[3], Corner[0]);
        }
    }
}
#endif