using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Pawn;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Interface.Interactive.AvatarShop
{
    //Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons
    public class AttributeMenu
    {
        Nameplate nameplate;

        List<Sprite> spriteList;
        List<Button> buttonList;
        List<NumericSpriteFont> numericSpriteFontList;

        Sprite attack, defense,      attackDelay, dig;
        Sprite health, regeneration, itemDelay,   popularity;

        SpriteText goldSpriteText, cashSpriteText;
        Sprite goldIcon, cashIcon;

        List<SpriteText> remainingPointsSpriteTextList;
        NumericSpriteFont remaningPointsNumericSpriteFont;

        public AttributeMenu(Vector2 basePosition, Player player)
        {
            //Nameplates
            nameplate = new Nameplate(player, Alignment.Left, basePosition + new Vector2(410, 363));

            //Perks icon and attributes
            attack = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 1, 17 * 2, 26, 17));
            health = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 5, 17 * 2, 26, 17));

            defense = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 3, 17 * 2, 26, 17));
            regeneration = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 5, 17 * 3, 26, 17));

            attackDelay = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 3, 17 * 1, 26, 17));
            itemDelay = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 1, 17 * 3, 26, 17));

            dig = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 3, 17 * 3, 26, 17));
            popularity = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/ButtonIcons", default, DepthParameter.InterfaceButton, new Rectangle(26 * 5, 17 * 1, 26, 17));

            spriteList = new List<Sprite>() { attack, health, defense, regeneration, attackDelay, itemDelay, dig, popularity };

            numericSpriteFontList = new List<NumericSpriteFont>();
            buttonList = new List<Button>();

            for (int i = 0; i < spriteList.Count; i++)
            {
                Sprite s = spriteList[i];
                s.PositionOffset = basePosition + new Vector2((i % 2) * 60, (i / 2) * 18);
                s.UpdateAttatchmentPosition();
                s.Pivot = new Vector2(9, 7f);

                numericSpriteFontList.Add(new NumericSpriteFont(FontType.AvatarShopStatusCounter, 2, DepthParameter.InterfaceButton, PositionOffset: s.PositionOffset + new Vector2(10, -7), forceRendingAllNumbers: true));

                Button b1 = new Button(ButtonType.ScrollBarUp, DepthParameter.InterfaceButton, (o) => { }, s.PositionOffset + new Vector2(30, 0));
                Button b2 = new Button(ButtonType.ScrollBarDown, DepthParameter.InterfaceButton, (o) => { }, s.PositionOffset + new Vector2(-12, 0));

                b1.ButtonSprite.Scale = b2.ButtonSprite.Scale /= 2;

                buttonList.Add(b1);
                buttonList.Add(b2);
            }

            //Gold/Cash
            goldSpriteText = new SpriteText(FontTextType.Consolas10, "", Parameter.InterfaceAvatarShopButtonGoldColor, Alignment.Right, DepthParameter.InterfaceButton, basePosition, Parameter.InterfaceAvatarShopButtonGoldOutlineColor);
            goldSpriteText.Position = basePosition + new Vector2(582, 452);
            goldIcon = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/GoldIcon",
                basePosition + new Vector2(594, 458), DepthParameter.InterfaceButton, new Rectangle(0, 0, 26, 21));
            goldIcon.Pivot = new Vector2(13, 10.5f);
            goldIcon.Scale /= 2;

            cashSpriteText = new SpriteText(FontTextType.Consolas10, "", Parameter.InterfaceAvatarShopButtonCashColor, Alignment.Right, DepthParameter.InterfaceButton, basePosition, Parameter.InterfaceAvatarShopButtonCashOutlineColor);
            cashSpriteText.Position = basePosition + new Vector2(582, 472);
            cashIcon = new Sprite("Interface/StaticButtons/AvatarShop/AvatarButton/CashIcon",
                basePosition + new Vector2(594, 478), DepthParameter.InterfaceButton, new Rectangle(0, 0, 26, 21));
            cashIcon.Pivot = new Vector2(13, 10.5f);
            cashIcon.Scale /= 2;

            RefreshCurrencyValues();

            //Remaining Points
            remainingPointsSpriteTextList =
                new List<SpriteText>() {
                    new SpriteText(FontTextType.Consolas10, "Remaining", Color.White, Alignment.Center, DepthParameter.InterfaceButtonText, outlineColor: Color.Black),
                    new SpriteText(FontTextType.Consolas10, "Points", Color.White, Alignment.Center, DepthParameter.InterfaceButtonText, outlineColor: Color.Black),
                };

            remainingPointsSpriteTextList[0].Position = basePosition + new Vector2(448, 380);
            remainingPointsSpriteTextList[1].Position = basePosition + new Vector2(448, 395);

            remaningPointsNumericSpriteFont = new NumericSpriteFont(FontType.AvatarShopStatusCounter, 3, DepthParameter.InterfaceButton,
                PositionOffset: basePosition + new Vector2(-73, 25),
                StartingValue: 300,
                forceRendingAllNumbers: true);

            numericSpriteFontList.Add(remaningPointsNumericSpriteFont);

            //Confirm/Cancel points
            Button accept = new Button(ButtonType.Accept, DepthParameter.InterfaceButton, (o) => { }, basePosition + new Vector2(-78, 52));
            Button cancel = new Button(ButtonType.Cancel, DepthParameter.InterfaceButton, (o) => { }, basePosition + new Vector2(-48, 52));
            accept.ButtonSprite.Scale = cancel.ButtonSprite.Scale *= 3f / 4f;

            buttonList.Add(accept);
            buttonList.Add(cancel);
        }

        public void RefreshCurrencyValues()
        {
            goldSpriteText.Text = string.Format("{0:N0}", GameInformation.Instance.PlayerInformation.Gold);
            cashSpriteText.Text = string.Format("{0:N0}", GameInformation.Instance.PlayerInformation.Cash);
        }

        public void Update(GameTime gameTime)
        {
            buttonList.ForEach((x) => x.Update());
            numericSpriteFontList.ForEach((x) => x.Update());
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            nameplate.Draw(spriteBatch);
            spriteList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            numericSpriteFontList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            buttonList.ForEach((x) => x.Draw(gameTime, spriteBatch));

            goldSpriteText.Draw(spriteBatch);
            cashSpriteText.Draw(spriteBatch);

            goldIcon.Draw(gameTime, spriteBatch);
            cashIcon.Draw(gameTime, spriteBatch);

            remainingPointsSpriteTextList.ForEach((x) => x.Draw(spriteBatch));
        }
    }
}
