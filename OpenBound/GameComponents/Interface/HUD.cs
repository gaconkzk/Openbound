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
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Interface.Interactive;
using OpenBound.GameComponents.Interface.Interactive.Misc;
using OpenBound.GameComponents.Interface.Popup;
using OpenBound.GameComponents.Level.Scene;
using OpenBound.GameComponents.Pawn;
using Openbound_Network_Object_Library.Entity;
using System.Collections.Generic;
using Openbound_Network_Object_Library.Models;

namespace OpenBound.GameComponents.Interface
{
    public class HUD
    {
        public TimedProgressBar StrenghtBar { get; private set; }
        public ProgressBar MovementBar { get; private set; }

        private List<Sprite> spriteList;
        private Sprite strenghtBarPreviousShotMarker, shotStrenghtSelector;

        private ShotButton shot1Button;
        private List<Button> menuButtons;

        private List<SpriteNumericField> spriteNumericFieldList;

        private NumericSpriteFont currentAngle;
        private NumericSpriteFont previousAngle;

        private readonly Mobile mobile;

        //All game floating texts
        public FloatingTextHandler FloatingTextHandler;

        //Units lifebars
        private List<HealthBar> healthBarList;
        private List<Nameplate> nameplateList;

        //Weather effects
        public WindCompass windCompass;
        public WeatherDisplay weatherDisplay;

        //Static popups
        private Button gameOptionsButton;
        private PopupGameOptions popupGameOptions;

        //Pass Turn button
        private Button passTurnButton;

        public Delayboard Delayboard;

        private Vector2 origin;

        public HUD(Mobile mobile, List<Mobile> mobileList)
        {
            //Attributions
            this.mobile = mobile;

            //Initializing lists
            RoomMetadata room = GameInformation.Instance.RoomMetadata;

            spriteList = new List<Sprite>();
            menuButtons = new List<Button>();
            FloatingTextHandler = new FloatingTextHandler();
            spriteNumericFieldList = new List<SpriteNumericField>();

            //Background
            Sprite barBG = new Sprite("Interface/InGame/HUD/Blue/ActionBar/Background", layerDepth: DepthParameter.HUDBackground);
            spriteList.Add(barBG);

            //Defining the origin
            origin = new Vector2(0, (Parameter.ScreenResolution.Y - barBG.SpriteHeight) / 2);
            barBG.PositionOffset = origin;

            //Right Menu (TAG || SCORE)
            if (room.GameMode == GameMode.Score)
            {
                Sprite rightMenuScoreboard = new Sprite("Interface/InGame/HUD/Blue/ActionBar/RightMenuScoreBoard", layerDepth: DepthParameter.HUDForeground,
                        sourceRectangle: (this.mobile.Owner.PlayerTeam == PlayerTeam.Red) ? new Rectangle(0, 0, 149, 63) : new Rectangle(149, 0, 149, 63))
                { PositionOffset = new Vector2(401, -11) + origin };
                spriteList.Add(rightMenuScoreboard);

                //Team Score - Left
                spriteNumericFieldList.Add(new NumericSpriteFont(FontType.HUDBlueScoreboard, 1, DepthParameter.HUDL5,
                    PositionOffset: ((this.mobile.Owner.PlayerTeam == PlayerTeam.Red) ? new Vector2(318, -5) : new Vector2(392, -5)) + origin,
                    StartingValue: 2, TextAnchor: TextAnchor.Right));

                //Team Score - Right
                spriteNumericFieldList.Add(new NumericSpriteFont(FontType.HUDBlueScoreboard, 1, DepthParameter.HUDL5,
                    PositionOffset: ((this.mobile.Owner.PlayerTeam == PlayerTeam.Blue) ? new Vector2(318, -5) : new Vector2(392, -5)) + origin,
                    StartingValue: 2, TextAnchor: TextAnchor.Right));

                //GoldCounter
                spriteNumericFieldList.Add(new CurrencySpriteFont(FontType.HUDBlueGold, 6, DepthParameter.HUDL5,
                    positionOffset: new Vector2(372, 23) + origin, startingValue: 1200, textAnchor: TextAnchor.Right));
            }
            else if (room.GameMode == GameMode.Tag)
            {
                Sprite rightMenuTag = new Sprite("Interface/InGame/HUD/Blue/ActionBar/RightMenuTag", layerDepth: DepthParameter.HUDForeground)
                { PositionOffset = new Vector2(326, 1) + origin };
                spriteList.Add(rightMenuTag);

                menuButtons.Add(new Button(ButtonType.BlueTag, DepthParameter.HUDL1, default, new Vector2(288, 9) + origin));
            }

            //Shot Strenght Metter
            StrenghtBar = new TimedProgressBar("Interface/InGame/HUD/Blue/ActionBar/StrenghtMetter",
                DepthParameter.HUDForeground, Parameter.ShotStrenghtTimeLimit,
                Parameter.ShotStrenghtBarStep, new Vector2(41, 17) + origin, true);

            strenghtBarPreviousShotMarker = new Sprite("Interface/InGame/HUD/Blue/ActionBar/PreviousShootingStrenghtPointer",
                layerDepth: DepthParameter.HUDForeground)
            { PositionOffset = new Vector2(-160, 26) + origin };
            spriteList.Add(strenghtBarPreviousShotMarker);

            shotStrenghtSelector = new Sprite("Interface/InGame/HUD/Blue/ActionBar/ShotStrenghtSelector",
                layerDepth: DepthParameter.HUDForeground)
            { PositionOffset = new Vector2(-160, 17) + origin };
            spriteList.Add(shotStrenghtSelector);

            //Movement Metter
            MovementBar = new ProgressBar("Interface/InGame/HUD/Blue/ActionBar/MovementMetter",
                DepthParameter.HUDForeground, -100f / this.mobile.Movement.MaximumStepsPerTurn,
                new Vector2(41, 35) + origin);

            //Reset HUD components
            MovementBar.Reset();

            //Panel that spawns above the items
            Sprite matchStatusClosed = new Sprite("Interface/InGame/HUD/Blue/ActionBar/MatchStatus", layerDepth: DepthParameter.HUDL3,
                sourceRectangle: new Rectangle(0, 0, 211, 33))
            { PositionOffset = new Vector2(312, -20) + origin };
            spriteList.Add(matchStatusClosed);

            Sprite matchStatus = new Sprite("Interface/InGame/HUD/Blue/ActionBar/MatchStatus", layerDepth: DepthParameter.HUDL4,
                sourceRectangle: new Rectangle(((int)room.MatchSuddenDeathType + 1) * 211, 0, 211, 33))
            { PositionOffset = new Vector2(312, -20) + origin };
            spriteList.Add(matchStatus);

            currentAngle = new NumericSpriteFont(FontType.HUDBlueCurrentAngle, 3, DepthParameter.HUDL5,
                PositionOffset: new Vector2(-149, -26) + origin, TextAnchor: TextAnchor.Right);
            spriteNumericFieldList.Add(currentAngle);

            previousAngle = new NumericSpriteFont(FontType.HUDBluePreviousAngle, 3, DepthParameter.HUDL5,
                PositionOffset: new Vector2(-185, -20) + origin, TextAnchor: TextAnchor.Right);
            spriteNumericFieldList.Add(previousAngle);

            //Menu Buttons
            shot1Button = new ShotButton(ButtonType.BlueShot1, this.mobile, DepthParameter.HUDForeground, ShotType.S1, true, new Vector2(-371, -4) + origin);
            ShotButton shot2Button = new ShotButton(ButtonType.BlueShot2, this.mobile, DepthParameter.HUDForeground, ShotType.S2, ButtonOffset: new Vector2(-328, -4) + origin);
            ShotButton shotSSButton = new ShotButton(ButtonType.BlueShotSS, this.mobile, DepthParameter.HUDForeground, ShotType.SS, ButtonOffset: new Vector2(-285, -4) + origin);

            shot1Button.OtherButtons = shot2Button.OtherButtons = shotSSButton.OtherButtons = new List<ShotButton>() { shot1Button, shot2Button, shotSSButton };
            shot1Button.ChangeButtonState(ButtonAnimationState.Activated);

            menuButtons.Add(shot2Button);
            menuButtons.Add(shotSSButton);

            gameOptionsButton = new Button(ButtonType.BlueOptions, DepthParameter.HUDForeground, GameOptionsOnClickAction, new Vector2(-359, 26) + origin);
            menuButtons.Add(gameOptionsButton);

            passTurnButton = new Button(ButtonType.BlueSkipTurn, DepthParameter.HUDForeground, PassTurnButtonAction, new Vector2(-296, 26) + origin);
            menuButtons.Add(passTurnButton);

            //Shot Strenght Selector (Button)
            TransparentButton shotStrenghtSelectorButton = new TransparentButton(
                new Vector2(41, 17) + origin, new Rectangle(0, 0, 400, 19));
            shotStrenghtSelectorButton.OnBeingDragged = (button) =>
            {
                shotStrenghtSelector.PositionOffset = new Vector2(
                    MathHelper.Clamp(
                        Cursor.Instance.CurrentFlipbook.Position.X + GameScene.Camera.CameraOffset.X,
                        -160 + origin.X,
                        -160 + origin.X + StrenghtBar.BarSprite.SpriteWidth),
                    shotStrenghtSelector.PositionOffset.Y);
            };
            menuButtons.Add(shotStrenghtSelectorButton);

            //Health Bars
            healthBarList = new List<HealthBar>();
            nameplateList = new List<Nameplate>();

            mobileList.ForEach((x) =>
            {
                healthBarList.Add(new HealthBar(x));
                nameplateList.Add(new Nameplate(x));
            });

            //Wind Compass
            windCompass = new WindCompass(Vector2.Zero + new Vector2(0, 30 - Parameter.ScreenCenter.Y));

            //Weather Display
            weatherDisplay = new WeatherDisplay(origin + new Vector2(157, -20));

            //Delayboard
            Delayboard = new Delayboard(mobileList, new Vector2(origin.X - spriteList[0].SpriteWidth / 2, origin.Y - spriteList[0].SpriteHeight / 2));

            //Popups
            popupGameOptions = new PopupGameOptions(default);
            popupGameOptions.OnClose += PopupGameOptionsOnCloseAction;
            PopupHandler.Add(popupGameOptions);
        }

        public void MobileDeath(Mobile mobile)
        {
            Delayboard.RemoveEntry(mobile);
            healthBarList.Find((x) => x.Mobile == mobile).HideElement();
            nameplateList.Find((x) => x.Mobile == mobile).HideElement();
        }

        public void PassTurnButtonAction(object sender)
        {
            //mobile.RequestLoseTurn();
        }

        public void LoseTurn()
        {
            passTurnButton.Disable();
        }

        public void GrantTurn()
        {
            StrenghtBar.Reset();
            MovementBar.Reset();
            passTurnButton.Enable();
        }

        public void GameOptionsOnClickAction(object sender)
        {
            popupGameOptions.ShouldRender = true;
            gameOptionsButton.Disable();
        }

        public void PopupGameOptionsOnCloseAction(object sender)
        {
            gameOptionsButton.Enable();
        }

        public void Update(GameTime GameTime)
        {
            //Sprites
            spriteList.ForEach((x) => x.UpdateAttatchmentPosition());

            //Bars
            MovementBar.UpdateAttatchmentPosition();
            StrenghtBar.UpdateAttatchmentPosition();

            //Floating Texts
            FloatingTextHandler.Update(GameTime);

            //Menu buttons
            menuButtons.ForEach((x) => x.Update());
            shot1Button.Update();

            //Angle
            currentAngle.UpdateValue(mobile.Crosshair.HUDSelectedAngle);

            //Fonts and texts
            spriteNumericFieldList.ForEach((x) => x.Update(GameTime));

            //Health bars
            healthBarList.ForEach((x) => x.Update());
            nameplateList.ForEach((x) => x.Update());

            //Delayboard
            Delayboard.Update();

            //Wind Compass
            windCompass.Update(GameTime);
            weatherDisplay.Update(GameTime);
        }

        public void UpdatePreviousShotMarker()
        {
            strenghtBarPreviousShotMarker.PositionOffset = new Vector2(-160 + StrenghtBar.BarSprite.SourceRectangle.Width, 26) + origin;
            previousAngle.UpdateValue((int)currentAngle.CurrentValue);
        }

        public void Draw(GameTime GameTime, SpriteBatch SpriteBatch)
        {
            //Sprites
            spriteList.ForEach((x) => x.Draw(GameTime, SpriteBatch));

            //Bars
            StrenghtBar.Draw(GameTime, SpriteBatch);
            MovementBar.Draw(GameTime, SpriteBatch);

            //Floating Texts
            FloatingTextHandler.Draw(GameTime, SpriteBatch);

            //SpriteFont
            spriteNumericFieldList.ForEach((x) => x.Draw(GameTime, SpriteBatch));

            //Static Buttons
            shot1Button.Draw(GameTime, SpriteBatch);
            menuButtons.ForEach((x) => x.Draw(GameTime, SpriteBatch));

            //Health bars
            healthBarList.ForEach((x) => x.Draw(GameTime, SpriteBatch));
            nameplateList.ForEach((x) => x.Draw(SpriteBatch));

            //Delayboard
            Delayboard.Draw(GameTime, SpriteBatch);

            //Wind Compass
            windCompass.Draw(SpriteBatch);
            weatherDisplay.Draw(GameTime, SpriteBatch);
        }
    }
}
