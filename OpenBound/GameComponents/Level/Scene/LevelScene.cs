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

#define DebugScene

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Animation.InGame;
using OpenBound.GameComponents.Audio;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Interface.Popup;
using OpenBound.GameComponents.Pawn;
using OpenBound.ServerCommunication;
using OpenBound.ServerCommunication.Service;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Openbound_Network_Object_Library.Models;
using OpenBound.GameComponents.WeatherEffect;
using Openbound_Network_Object_Library.Entity.Text;
using OpenBound.GameComponents.Input;
using Microsoft.Xna.Framework.Input;
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Interface.Interactive;
using OpenBound.GameComponents.Pawn.Unit;
using OpenBound.GameComponents.MobileAction;
using OpenBound.GameComponents.MobileAction.Motion;

namespace OpenBound.GameComponents.Level.Scene
{
    /// <summary>
    /// LevelScene is used as an abstract model for all the levels (or maps) in the game.
    /// </summary>
    public class LevelScene : GameScene
    {
        #region Scenario/Animation Variables
        //Level Background & Parallaxing effect
        public Vector2 BackgroundRemainingImage { get; set; }

        //Level Foreground (Playable map area)
        public Sprite Foreground { get; protected set; }

        //Background flipbook animations
        public List<Renderable> BackgroundFlipbookList { get; protected set; }
        public List<Vector2> BackgroundFlipbookOffsetList { get; protected set; }
        #endregion

        //Game Constants/Information
        public static MatchMetadata MatchMetadata;
        
        //Mobile list
        public static List<Mobile> MobileList;

        //Summoned list
        public static List<Mobile> MineList;
        public static List<Mobile> ToBeRemovedMineList;

        //Weather
        public static WeatherHandler WeatherHandler;
        public static ThorSatellite ThorSatellite;

        //Interface
        public static HUD HUD;

        private bool isLeaveGamePopupRendered;

        public static IEnumerable<Mobile> DamageableMobiles => MobileList.Union(MineList).Except(ToBeRemovedMineList);

        public LevelScene()
        {
            GameInformation.Instance.GameState = GameState.InGame;

            MobileList = new List<Mobile>();
            WeatherHandler = new WeatherHandler();
            ThorSatellite = new ThorSatellite();

            MineList = new List<Mobile>();
            ToBeRemovedMineList = new List<Mobile>();

            //Popup related
            isLeaveGamePopupRendered = false;

            //Spawning units on the selected coordinates
            RoomMetadata room = GameInformation.Instance.RoomMetadata;
            Mobile mobile = null;

            foreach (KeyValuePair<int, int[]> coordinate in room.SpawnPositions)
            {
                foreach (Player p in room.PlayerList)
                {
                    if (p.ID == coordinate.Key)
                    {
                        Mobile mob = ActorBuilder.BuildMobile(p.PrimaryMobile, p, new Vector2(coordinate.Value[0], coordinate.Value[1]));

                        MobileList.Add(mob);

                        if (p.ID == GameInformation.Instance.PlayerInformation.ID) mobile = mob;
                    }
                }
            }

            SpawnMapElements();

            //Initializing HUD component
            HUD = new HUD(mobile, MobileList);

            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameStartMatch, StartMatchAsyncCallback);
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameRequestShot, RequestShotAsyncCallback);
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameRefreshSyncMobile, RefreshSyncMobileAsyncCallback);
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameRequestNextPlayerTurn, RequestNextPlayerTurnAsyncCallback);
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameRequestDeath, RequestDeathAsyncCallback);
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameRequestGameEnd, RequestGameEndAsyncCallback);
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerInGameRequestDisconnect, RequestDisconnectAsyncCallback);

            //Textbox handlers
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerChatSendPlayerMessage, (o) => HUD.OnReceiveMessageAsyncCallback(o, 0));
            ServerInformationBroker.Instance.ActionCallbackDictionary.AddOrReplace(NetworkObjectParameters.GameServerChatSendSystemMessage, (o) => HUD.OnReceiveMessageAsyncCallback(o, 1));
        }

        public override void OnSceneIsActive()
        {
            base.OnSceneIsActive();
            AudioHandler.ChangeSong(SongParameter.LevelScene, ChangeEffect.Fade);
        }

        public void StartMobileFacingTrajectory()
        {
            if (MobileList.Count <= 1) return;

            foreach (Mobile refMob in MobileList)
            {
                Mobile targetRef = null;
                double lesserDistance = double.MaxValue;

                List<Mobile> mobList = MobileList.Where((x) => x.Owner.PlayerTeam != refMob.Owner.PlayerTeam).ToList();
                foreach (Mobile mob in mobList)
                {
                    double distance = Helper.EuclideanDistance(mob.Position, refMob.Position);

                    if (distance < lesserDistance)
                    {
                        lesserDistance = distance;
                        targetRef = mob;
                    }
                }

                if ((refMob.Position - targetRef.Position).X < 0)
                {
                    if (refMob.Facing == Facing.Left) refMob.Flip();
                }
                else
                {
                    if (refMob.Facing == Facing.Right) refMob.Flip();
                }

                refMob.SyncMobile.Facing = refMob.Facing;
            }
        }

        #region Level-Related Methods
        public void SpawnMapElements()
        {
            GameMap map = GameInformation.Instance.RoomMetadata.Map.GameMap;
            GameMapType type = GameInformation.Instance.RoomMetadata.Map.GameMapType;

            Background = new Sprite($@"Graphics/Maps/{map}/Background", layerDepth: DepthParameter.Background);
            BackgroundOffset = new Vector2(0, 0);

            Foreground = new Sprite($@"Graphics/Maps/{map}/Foreground{type}",
                layerDepth: DepthParameter.Foreground, shouldCopyAsset: true);

            BackgroundFlipbookList = new List<Renderable>();
            BackgroundFlipbookOffsetList = new List<Vector2>();

            int i = 1;

            switch (map)
            {
                #region Metamine/Facky
                case GameMap.Metamine:
                    BackgroundFlipbookList = new List<Renderable>
                    {
                        new Flipbook(Vector2.Zero, Vector2.Zero,  52, 111, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame = 14, TimePerFrame = 0.1f },                                      DepthParameter.BackgroundAnim),
                        new Flipbook(Vector2.Zero, Vector2.Zero, 116, 191, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame =  9, TimePerFrame = 0.1f, AnimationType = AnimationType.Cycle }, DepthParameter.BackgroundAnim),
                        new Flipbook(Vector2.Zero, Vector2.Zero,  64, 192, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame =  9, TimePerFrame = 0.1f },                                      DepthParameter.BackgroundAnim),
                        new Flipbook(Vector2.Zero, Vector2.Zero, 100, 186, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame = 11, TimePerFrame = 0.2f },                                      DepthParameter.BackgroundAnim),
                    };

                    BackgroundFlipbookOffsetList = new List<Vector2>() { new Vector2(-38, -403), new Vector2(-192, -158), new Vector2(163, -270), new Vector2(124, -116) };
                    break;
                #endregion
                #region SeaOfHero/Candy
                case GameMap.SeaOfHero:
                    BackgroundFlipbookList = new List<Renderable>
                    {
                        new Flipbook(Vector2.Zero, Vector2.Zero, 142, 161, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame = 19, TimePerFrame = 0.2f },                                      DepthParameter.BackgroundAnim),
                        new Flipbook(Vector2.Zero, Vector2.Zero,  74, 124, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame = 19, TimePerFrame = 0.5f, AnimationType = AnimationType.Cycle }, DepthParameter.BackgroundAnim),
                        new Flipbook(Vector2.Zero, Vector2.Zero,  86,  76, $@"Graphics/Maps/{map}/BackgroundAnimation{i++}", new AnimationInstance() { EndingFrame =  1, TimePerFrame = 5f   },                                      DepthParameter.BackgroundAnim),
                    };

                    BackgroundFlipbookOffsetList = new List<Vector2>() { new Vector2(-415, -235), new Vector2(208, -34), new Vector2(110, 033) };
                    break;
                #endregion
                default:
                    return;
            }
        }

        public void UpdateBackgroundParallaxPosition()
        {
            //Change BackgroundImageLeft signal for different parallax effect
            Vector2 currentCamPos = (Camera.ScreenCenterPosition - Camera.CameraOffset);
            Vector2 newPos = currentCamPos * -BackgroundRemainingImage / Camera.CameraReachableRange;
            newPos = BackgroundOffset - Camera.CameraOffset + newPos;

            Background.Position = new Vector2((int)newPos.X, (int)newPos.Y);

            for (int i = 0; i < BackgroundFlipbookList.Count; i++)
            {
                BackgroundFlipbookList[i].Position = Background.Position + BackgroundFlipbookOffsetList[i];
            }
        }
        #endregion
        #region Netcode
        public override void OnTryClosingGame()
        {
            if (isLeaveGamePopupRendered) return;

            PopupAlertMessage popup = (PopupAlertMessage)PopupAlertMessageBuilder.BuildPopupAlertMessage(PopupAlertMessageType.LeaveGame);
            isLeaveGamePopupRendered = true;

            popup.OnClose = (sender) =>
            {
                isLeaveGamePopupRendered = false;
            };
        }

        private void RequestDisconnectAsyncCallback(object answer)
        {
            try
            {
                SyncMobile sm = (SyncMobile)answer;

                lock (MobileList)
                {
                    Mobile mob = MobileList.Find((x) => x.Owner.ID == sm.Owner.ID);
                    mob.Die();

                    if (mob.Owner.ID != MatchMetadata.CurrentTurnOwner.Owner.ID || !mob.IsAbleToShoot) return;

                    ServerInformationHandler.RequestNextPlayerTurn();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dsync At Disconnect Request: " + ex.Message);
            }
        }

        private void RequestGameEndAsyncCallback(object answer)
        {
            RoomMetadata room = GameInformation.Instance.RoomMetadata;

            lock (room)
            {
                GameInformation.Instance.RoomMetadata.VictoriousTeam = (PlayerTeam)answer;
                PopupHandler.Add(new PopupGameResults());
            }
        }

        private void RequestNextPlayerTurnAsyncCallback(object answer)
        {
            try
            {
                lock (MobileList)
                {
                    //A team have won
                    if (answer == null) return;

                    MatchMetadata = (MatchMetadata)answer;

                    //Weather Variations
                    HUD.WeatherDisplay.AppendWeatherToList(MatchMetadata.IncomingWeatherList);

                    //Change Wind (If Necessary)
                    HUD.windCompass.ChangeWind(MatchMetadata.WindAngleDegrees, MatchMetadata.WindForce);
                }

                Thread.Sleep(Parameter.GameplayConstantDelayTimerWindChange);

                lock (MobileList)
                {
                    //Grant Turn
                    Mobile m = MobileList.Find((x) => x.Owner.ID == MatchMetadata.CurrentTurnOwner.Owner.ID);
                    m.GrantTurn();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dsync at RequestNextPlayerTurnAsyncCallback: " + ex.Message);
            }
        }

        private void StartMatchAsyncCallback(object answer)
        {
            RefreshSyncMobileAsyncCallback(answer);

            Thread.Sleep(Parameter.GameplayConstantDelayTimerFirstTurn);

            ServerInformationHandler.RequestNextPlayerTurn();

            //Fixing (again) player initial facing position
            StartMobileFacingTrajectory();
        }

        public void RefreshSyncMobileAsyncCallback(object answer)
        {
            try
            {
                List<SyncMobile> syncMobileList = (List<SyncMobile>)answer;

                Player p = GameInformation.Instance.PlayerInformation;

                lock (MobileList)
                {
                    foreach (SyncMobile syncMob in syncMobileList)
                    {
                        Mobile mob = MobileList.Find((x) => x.Owner.ID == syncMob.Owner.ID);
                        mob.HandleSyncMobile(syncMob);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ex: " + ex.Message);
                Console.WriteLine("Ex: " + ex.StackTrace);
            }
        }

        public void RequestDeathAsyncCallback(object answer)
        {
            try
            {
                SyncMobile syncMobile = (SyncMobile)answer;

                lock (MobileList)
                {
                    Mobile mob = MobileList.Find((x) => x.Owner.ID == syncMobile.Owner.ID);
                    mob.Die();
                    HUD.MobileDeath(mob);

                    if (mob.Owner.ID == MatchMetadata.CurrentTurnOwner.Owner.ID)
                        ServerInformationHandler.RequestNextPlayerTurn();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}");
            }
        }

        public void RequestShotAsyncCallback(object answer)
        {
            try
            {
                SyncMobile syncMobile = (SyncMobile)answer;

                Mobile mob = null;

                lock (MobileList)
                {
                    mob = MobileList.Find((x) => syncMobile.Owner.ID == x.Owner.ID);
                    mob.HandleSyncMobile(syncMobile);
                }

                if (!mob.IsPlayable)
                    while (!((RemoteMovement)mob.Movement).IsReadyToShoot) Thread.Sleep(100);

                lock (MobileList)
                {
                    mob.SyncMobile = syncMobile;
                    mob.ConsumeShootAction();

                    if (syncMobile.Owner.ID == GameInformation.Instance.PlayerInformation.ID)
                        HUD.Delayboard.ComputeDelay(syncMobile);

                    mob.LoseTurn();
                }
                //Helper.EuclideanDistance(mob.Position, mob.Movement.DesiredPosition)
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ex: " + ex.Message);
                Console.WriteLine("Ex: " + ex.StackTrace);
            }
        }
        #endregion

        public override void Initialize(GraphicsDevice GraphicsDevice, SpriteBatch SpriteBatch)
        {
            base.Initialize(GraphicsDevice, SpriteBatch);

            //Initialize Camera
            Camera.AdjustAttackParameters(this);

            //Extract terrain geography
            Topography.Initialize(Foreground);

            MobileList.ForEach((x) =>
            {
                //Repositioning the actors in order to accurately reproduce the scenario coordinate showed in the loading screen
                //This must be called here because in this position the background will be already initialized
                x.Position -= Foreground.Pivot;

                //Define the spawning points for each remote mobile
                if (!x.IsPlayable) ((RemoteMovement)x.Movement).DesiredPosition = x.Position;
            });

            //Start all mobiles facing trajectory
            StartMobileFacingTrajectory();

            //Parallax Vectors
            BackgroundOffset = Vector2.Zero;
            BackgroundRemainingImage = new Vector2(Background.SpriteWidth, Background.SpriteHeight) - Parameter.ScreenResolution;

            //Clear all sfx from the list
            SpecialEffectHandler.Initialize();

            //Clear all pending death animations
            DeathAnimation.Initialize();

            //Start Match
#if !DEBUGSCENE
            ServerInformationHandler.StartMatch();
#endif
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            WeatherHandler.Update(gameTime);

            lock (MobileList)
            {
                MobileList.ForEach((m) => m.Update(gameTime));
                HUD.Update(gameTime);
            }

            MineList.ForEach((x) => x.Update(gameTime));
            ToBeRemovedMineList.ForEach((x) => MineList.Remove(x));
            ToBeRemovedMineList.Clear();

            UpdateBackgroundParallaxPosition();

            ThorSatellite.Update(gameTime);

            DeathAnimation.Update(gameTime);
            SpecialEffectHandler.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            BackgroundFlipbookList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            Foreground.Draw(gameTime, spriteBatch);

            WeatherHandler.Draw(gameTime, spriteBatch);

            lock (MobileList)
            {
                MobileList.ForEach((x) => x.Draw(gameTime, spriteBatch));
                HUD.Draw(gameTime, spriteBatch);
            }

            MineList.ForEach((x) => x.Draw(gameTime, spriteBatch));

            ThorSatellite.Draw(gameTime, spriteBatch);

            DeathAnimation.Draw(gameTime, spriteBatch);
            SpecialEffectHandler.Draw(gameTime, spriteBatch);
        }

        public override void Dispose()
        {
            base.Dispose();
            HUD.Dispose();
        }
    }
}