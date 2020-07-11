using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Pawn;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace OpenBound.GameComponents.Interface.Interactive.AvatarShop
{
    public class AvatarButton : Button
    {
        AvatarCategory avatarCategory;
        AvatarType avatarType;

        List<Sprite> spriteList;
        List<SpriteText> spriteTextList;

        Sprite typeIcon;
        Sprite thumb;
        Sprite newMarker;
        Sprite equippedIndicator;
        Sprite ownedCheck;
        Sprite goldIcon;
        Sprite cashIcon;

        //Texts
        SpriteText itemNameSpriteText;
        SpriteText goldSpriteText;
        SpriteText cashSpriteText;

        float newMarkerElapsedTime = 0;

        public AvatarButton(Vector2 position, int goldPrice, int cashPrice, string name, AvatarCategory avatarCategory, AvatarType avatarType, Gender gender, Action<object> onClick)
            : base(ButtonType.AvatarButton, DepthParameter.InterfacePopupButtons, onClick, position)
        {
            spriteList = new List<Sprite>();
            spriteTextList = new List<SpriteText>();

            thumb = new Sprite($"Graphics/Avatar/{gender}/Portrait/{avatarCategory}/{avatarType}",
                position + new Vector2(1, 6.5f), DepthParameter.InterfacePopupButtonIcon);
            Vector2 scaleFactor = new Vector2(87, 48) / new Vector2(thumb.SpriteWidth, thumb.SpriteHeight);
            float newScale = Math.Min(scaleFactor.X, scaleFactor.Y);
            thumb.Scale = new Vector2(newScale, newScale);

            spriteList.Add(thumb);

            newMarker = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/NewItem",
                position + new Vector2(40, -30), DepthParameter.InterfacePopupMessageBackground);
            newMarker.Scale /= 2;

            spriteList.Add(newMarker);

            equippedIndicator = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/EquipedIndicator",
                position + new Vector2(-47, 0), DepthParameter.InterfacePopupButtonIcon);

            spriteList.Add(equippedIndicator);

            ownedCheck = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/OwnedCheck",
                position + new Vector2(38, -14), DepthParameter.InterfacePopupButtonIcon);
            ownedCheck.Scale /= 2;

            spriteList.Add(ownedCheck);

            //Selecting button icon
            int index = GetCorrespondingIconIndex(avatarCategory, gender);
            typeIcon = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons",
                position + new Vector2(-32, -12), DepthParameter.InterfacePopupText,
                new Rectangle((index % 6) * 26, (index / 6) * 17, 26, 17));
            typeIcon.Pivot = new Vector2(13, 8.5f);

            spriteList.Add(typeIcon);

            //Texts
            if (name.Length > 13)
                name = name.Substring(0, 12) + ".";

            itemNameSpriteText = new SpriteText(FontTextType.Consolas10, name, Color.White, Alignment.Left, DepthParameter.InterfacePopupText, position, Color.Black);
            itemNameSpriteText.PositionOffset = position + new Vector2(-44, -36);
            spriteTextList.Add(itemNameSpriteText);

            int pricePos = 0;
            if (goldPrice != 0)
            {
                goldSpriteText = new SpriteText(FontTextType.Consolas10, string.Format("{0:N0}", goldPrice), Parameter.InterfaceAvatarShopButtonGoldColor, Alignment.Right, DepthParameter.InterfacePopupText, position, Parameter.InterfaceAvatarShopButtonGoldOutlineColor);
                goldSpriteText.PositionOffset = position + new Vector2(30, 20 - 10 * pricePos);
                goldIcon = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/GoldIcon",
                    position + new Vector2(38, 26 - 10 * pricePos++), DepthParameter.InterfacePopupText, new Rectangle(0, 0, 26, 21));
                goldIcon.Pivot = new Vector2(13, 10.5f);
                goldIcon.Scale /= 2;

                spriteTextList.Add(goldSpriteText);
                spriteList.Add(goldIcon);
            }

            if (cashPrice != 0)
            {
                cashSpriteText = new SpriteText(FontTextType.Consolas10, string.Format("{0:N0}", cashPrice), Parameter.InterfaceAvatarShopButtonCashColor, Alignment.Right, DepthParameter.InterfacePopupText, position, Parameter.InterfaceAvatarShopButtonCashOutlineColor);
                cashSpriteText.PositionOffset = position + new Vector2(30, 20 - 10 * pricePos);
                cashIcon = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/CashIcon",
                    position + new Vector2(38, 26 - 10 * pricePos++), DepthParameter.InterfacePopupText, new Rectangle(0, 0, 26, 21));
                cashIcon.Pivot = new Vector2(13, 10.5f);
                cashIcon.Scale /= 2;

                spriteTextList.Add(cashSpriteText);
                spriteList.Add(cashIcon);
            }
        }

        public void Update(GameTime gameTime)
        {
            base.Update();

            spriteList.ForEach((x) => x.UpdateAttatchmentPosition());
            spriteTextList.ForEach((x) => x.UpdateAttatchedPosition());

            newMarkerElapsedTime += MathHelper.Pi / 2 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            newMarkerElapsedTime %= MathHelper.TwoPi;

            newMarker.SetTransparency((float)Math.Sin(newMarkerElapsedTime));
        }

        private static int GetCorrespondingIconIndex(AvatarCategory avatarCategory, Gender gender)
        {
            switch (avatarCategory)
            {
                case AvatarCategory.Body:
                    return (gender == Gender.Male) ? 0 : 1;
                case AvatarCategory.Head:
                    return (gender == Gender.Male) ? 2 : 3;
                case AvatarCategory.Goggles:
                    return (gender == Gender.Male) ? 4 : 5;
                case AvatarCategory.Flag:
                    return 6;
                case AvatarCategory.Set:
                    return (gender == Gender.Male) ? 27 : 28;
                case AvatarCategory.ExItem:
                    return 29;
                case AvatarCategory.Landscape:
                    return 31;
                case AvatarCategory.Ring:
                    return 32;
                case AvatarCategory.Necklace:
                    return 33;
                case AvatarCategory.Pet:
                    return 34;

                default:
                    return 7;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            spriteList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            spriteTextList.ForEach((x) => x.Draw(spriteBatch));
        }
    }
}
