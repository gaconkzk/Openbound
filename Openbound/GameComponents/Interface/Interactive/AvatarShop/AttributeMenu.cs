using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Debug;
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Pawn;
using OpenBound.GameComponents.Pawn.UnitProjectiles;
using Openbound_Network_Object_Library.Common;
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
        List<Button> addAttributeButtonList;
        List<Button> removeAttributeButtonList;
        List<NumericSpriteFont> numericSpriteFontList;
        List<NumericSpriteFont> attributeNumericSpriteFontList;

        Sprite attack, defense,      attackDelay, dig;
        Sprite health, regeneration, itemDelay,   popularity;

        SpriteText goldSpriteText, cashSpriteText;
        Sprite goldIcon, cashIcon;

        List<SpriteText> remainingPointsSpriteTextList;
        NumericSpriteFont remaningPointsNumericSpriteFont;
        int remainingPoints;

        Button cancel, accept;
        int[] attributes;

        Vector2 basePosition;


        public AttributeMenu(Vector2 basePosition, Player player)
        {
            this.basePosition = basePosition;
            numericSpriteFontList = new List<NumericSpriteFont>();
            attributeNumericSpriteFontList = new List<NumericSpriteFont>();
            buttonList = new List<Button>();
            addAttributeButtonList = new List<Button>();
            removeAttributeButtonList = new List<Button>();

            attributes = GameInformation.Instance.PlayerInformation.Attribute;

            //Confirm/Cancel points
            accept = new Button(ButtonType.Accept, DepthParameter.InterfaceButton, AcceptChangesAction, basePosition + new Vector2(-78, 52));
            cancel = new Button(ButtonType.Cancel, DepthParameter.InterfaceButton, CancelChangesAction, basePosition + new Vector2(-48, 52));
            accept.ButtonSprite.Scale = cancel.ButtonSprite.Scale *= 3f / 4f;

            cancel.Disable();
            accept.Disable();

            //Nameplates
            nameplate = new Nameplate(player, Alignment.Left, basePosition + new Vector2(410, 363));

            //Remaining Points
            remainingPointsSpriteTextList =
                new List<SpriteText>() {
                    new SpriteText(FontTextType.Consolas10, "Remaining", Color.White, Alignment.Center, DepthParameter.InterfaceButtonText, outlineColor: Color.Black),
                    new SpriteText(FontTextType.Consolas10, "Points", Color.White, Alignment.Center, DepthParameter.InterfaceButtonText, outlineColor: Color.Black),
                };

            remainingPointsSpriteTextList[0].Position = basePosition + new Vector2(448, 380);
            remainingPointsSpriteTextList[1].Position = basePosition + new Vector2(448, 395);

            //Calculate remaining points
            remainingPoints = GameInformation.Instance.PlayerInformation.GetCurrentAttributePoints() - attributes.ToList().Sum();

            remaningPointsNumericSpriteFont = new NumericSpriteFont(FontType.AvatarShopStatusCounter, 3, DepthParameter.InterfaceButton,
                PositionOffset: basePosition + new Vector2(-73, 25),
                StartingValue: remainingPoints,
                forceRendingAllNumbers: true);

            numericSpriteFontList.Add(remaningPointsNumericSpriteFont);

            //Attribute Points
            AddAttributePoints();

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

            buttonList.Add(accept);
            buttonList.Add(cancel);
        }

        public void CancelChangesAction(object sender)
        {
            for(int i = 0; i < attributeNumericSpriteFontList.Count; i++)
            {
                NumericSpriteFont nsf = attributeNumericSpriteFontList[i];
                nsf.UpdateValue(attributes[i]);
            }

            remainingPoints = GameInformation.Instance.PlayerInformation.GetCurrentAttributePoints() - attributes.ToList().Sum();

            cancel.Disable();
            accept.Disable();

            UpdateAttributeButtons();
        }

        public void AcceptChangesAction(object sender)
        {
            for (int i = 0; i < attributeNumericSpriteFontList.Count; i++)
                attributes[i] = (int)attributeNumericSpriteFontList[i].CurrentValue;

            accept.Disable();
            cancel.Disable();
        }

        public void AddAttributePoints()
        {
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

            for (int i = 0; i < spriteList.Count; i++)
            {
                Sprite s = spriteList[i];
                s.PositionOffset = basePosition + new Vector2((i % 2) * 60, (i / 2) * 18);
                s.UpdateAttatchmentPosition();
                s.Pivot = new Vector2(9, 7f);

                NumericSpriteFont nsf = new NumericSpriteFont(FontType.AvatarShopStatusCounter, 2, DepthParameter.InterfaceButton,
                    PositionOffset: s.PositionOffset + new Vector2(10, -7), forceRendingAllNumbers: true,
                    StartingValue: GameInformation.Instance.PlayerInformation.Attribute[i]);

                attributeNumericSpriteFontList.Add(nsf);

                Button b1 = new Button(ButtonType.ScrollBarUp, DepthParameter.InterfaceButton, default, s.PositionOffset + new Vector2(30, 0));
                Button b2 = new Button(ButtonType.ScrollBarDown, DepthParameter.InterfaceButton, default, s.PositionOffset + new Vector2(-12, 0));

                b1.OnClick = (o) => { AddAttributeButtonAction(b1, b2, nsf); };
                b2.OnClick = (o) => { RemoveAttributeButtonAction(b1, b2, nsf); };

                b1.ButtonSprite.Scale = b2.ButtonSprite.Scale /= 2;

                if (GameInformation.Instance.PlayerInformation.Attribute[i] == NetworkObjectParameters.PlayerAttributeMaximumPerCategory)
                    b1.Disable();
                else if (GameInformation.Instance.PlayerInformation.Attribute[i] == 0)
                    b2.Disable();

                addAttributeButtonList.Add(b1);
                removeAttributeButtonList.Add(b2);
            }

            buttonList.AddRange(addAttributeButtonList);
            buttonList.AddRange(removeAttributeButtonList);

            numericSpriteFontList.AddRange(attributeNumericSpriteFontList);

            UpdateAttributeButtons();
        }

        public void AddAttributeButtonAction(Button addButton, Button removeButton, NumericSpriteFont nsf)
        {
            if (remainingPoints == 0) return;

            remainingPoints--;

            if (nsf.CurrentValue == NetworkObjectParameters.PlayerAttributeMaximumPerCategory)
            {
                UpdateAttributeButtons();
                return;
            }

            nsf.UpdateValue((int)nsf.CurrentValue + 1);

            UpdateAttributeButtons();
        }

        public void RemoveAttributeButtonAction(Button addButton, Button removeButton, NumericSpriteFont nsf)
        {
            if (nsf.CurrentValue == 0)
            {
                UpdateAttributeButtons();
                return;
            }

            remainingPoints++;

            nsf.UpdateValue((int)nsf.CurrentValue - 1);

            UpdateAttributeButtons();
        }

        public void UpdateAttributeButtons()
        {
            for(int i = 0; i < attributes.Length; i++)
            {
                if (attributeNumericSpriteFontList[i].CurrentValue == 0)
                    removeAttributeButtonList[i].Disable();
                else
                    removeAttributeButtonList[i].Enable();

                if (remainingPoints == 0 || attributes[i] >= NetworkObjectParameters.PlayerAttributeMaximumPerCategory)
                    addAttributeButtonList[i].Disable();
                else
                    addAttributeButtonList[i].Enable();

                if (attributes[i] != attributeNumericSpriteFontList[i].CurrentValue)
                {
                    cancel.Enable();
                }
            }

            if (cancel.IsEnabled)
                accept.Enable();

            remaningPointsNumericSpriteFont.UpdateValue(remainingPoints);
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
