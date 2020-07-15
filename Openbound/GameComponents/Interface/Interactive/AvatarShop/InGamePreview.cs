using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Pawn;
using OpenBound.GameComponents.Pawn.Unit;
using Openbound_Network_Object_Library.Models;
using System;

namespace OpenBound.GameComponents.Interface.Interactive.AvatarShop
{
    public class InGamePreview
    {
        Sprite background, foreground;

        Mobile mobile;

        public InGamePreview(Vector2 position)
        {
            background = new Sprite("Interface/InGame/Scene/AvatarShop/InGamePreview/MiramoTown/Background", position, DepthParameter.Background);
            foreground = new Sprite("Interface/InGame/Scene/AvatarShop/InGamePreview/MiramoTown/Foreground2", position, DepthParameter.BackgroundAnim);

            mobile = new RaonLauncher(GameInformation.Instance.PlayerInformation, position);
            mobile.Flip();
        }

        public void Hide()
        {
            mobile.HideElement();
            mobile.Rider.Hide();
        }

        public void Show()
        {
            mobile.ShowElement();
            mobile.Rider.Show();
        }

        public void ReplaceAvatar(AvatarMetadata avatarMetadata)
        {
            mobile.Rider.ReplaceAvatar(avatarMetadata);
        }

        public void Update()
        {
            mobile.Rider.Update();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            background.Draw(null, spriteBatch);
            foreground.Draw(null, spriteBatch);
            mobile.Draw(gameTime, spriteBatch);
        }
    }
}
