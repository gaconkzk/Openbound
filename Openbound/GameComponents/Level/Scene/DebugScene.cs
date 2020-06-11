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
using Microsoft.Xna.Framework.Input;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Input;
using OpenBound.GameComponents.Interface;
using OpenBound.GameComponents.Interface.Popup;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using Openbound_Network_Object_Library.Security;
using System;
using System.Collections.Generic;
using System.Threading;
using Openbound_Network_Object_Library.Models;
using OpenBound.GameComponents.WeatherEffect;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBound.GameComponents.Level.Scene
{
    public class DebugScene : LevelScene
    {
        public float sceneTimespan;
        public bool hasRequestedNextScene;

        public PopupGameOptions optionsMenu;
        public PopupSelectMobile popupSelectMobile;

        internal static void InitializeObjects()
        {
            GameInformation.Instance.PlayerInformation = new Player()
            {
                ID = 0,
            };

            #region Sync Mobile List
            sMobList = new List<SyncMobile>();

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Lightning),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 0,
                    Nickname = "Wicked",
                    Password = "123",
                    PlayerRank = PlayerRank.Staff4,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Turtle,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Red,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 300, 100 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 1,
                    Nickname = "Zicoman",
                    Password = "123",
                    PlayerRank = PlayerRank.Dragon1,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Blue,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 2,
                    Nickname = "Big String  To Test The Box Size Calc",
                    Password = "123",
                    PlayerRank = PlayerRank.Axe1,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Red,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 3,
                    Nickname = "Winged",
                    Password = "123",
                    PlayerRank = PlayerRank.GM,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Blue,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 4,
                    Nickname = "You woke me up!",
                    Password = "123",
                    PlayerRank = PlayerRank.DGoldenAxe2,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Blue,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 5,
                    Nickname = "Ordinary World",
                    Password = "123",
                    PlayerRank = PlayerRank.Chick,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Blue,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 6,
                    Nickname = "JonathanJoestar",
                    Password = "123",
                    PlayerRank = PlayerRank.Champion1,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Red,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });

            sMobList.Add(new SyncMobile()
            {
                CrosshairAngle = 10,
                Delay = 500,
                //Facing = Facing.Left,
                MobileMetadata = MobileMetadata.BuildMobileMetadata(MobileType.Knight),
                Owner = new Player()
                {
                    CharacterGender = Gender.Feminine,
                    Email = "c@c.com",
                    ID = 7,
                    Nickname = "JosephJoestar",
                    Password = "123",
                    PlayerRank = PlayerRank.Axe1,
                    PlayerRoomStatus = PlayerRoomStatus.Ready,
                    PrimaryMobile = MobileType.Knight,
                    SecondaryMobile = MobileType.Knight,
                    PlayerTeam = PlayerTeam.Red,
                    FriendList = new List<Player>(),
                    Guild = new Guild() { GuildMembers = new List<Player>(), ID = 1, Name = "Zica", Tag = "zicamasters" },
                    PlayerStatus = PlayerStatus.Normal,
                    SecurityToken = new SecurityToken() { DateTime = DateTime.Now, Token = "123123123", UnifiedSecurityToken = "123123123" }
                },
                Position = new int[] { 1, 0 },
                SelectedShotType = ShotType.SS,
                SynchronizableActionList = new List<SynchronizableAction>() { SynchronizableAction.LeftMovement, SynchronizableAction.ChargingShot }
            });
            #endregion

            //delayboard = new Delayboard(mobList, new Vector2(0, 0) /* Parameter.ScreenCenter */);

            GameInformation.Instance.RoomMetadata = new RoomMetadata(GameMode.Score, TurnsToSuddenDeath.Turn40, MatchSuddenDeathType.DoubleDeath, RoomSize.FourVsFour, sMobList[0].Owner, 1, "123", "123")
            {
                SpawnPositions = new Dictionary<int, int[]>()
                {
                    { 0, new int[] { 1100, 700 } },
                    { 1, new int[] { 300, 700 } }, { 2, new int[] { 700, 700 } },
                    { 3, new int[] { 400, 700 } }, { 4, new int[] { 800, 700 } },
                    { 5, new int[] { 500, 700 } }, { 6, new int[] { 900, 700 } },
                    { 7, new int[] { 600, 700 } }, { 8, new int[] { 1000, 700 } },
                },
                TeamA = new List<Player>() { sMobList[0].Owner, sMobList[1].Owner, sMobList[2].Owner, sMobList[3].Owner },
                TeamB = new List<Player>() { sMobList[4].Owner, sMobList[5].Owner, sMobList[6].Owner, sMobList[7].Owner },
            };
            GameInformation.Instance.RoomMetadata.Map = Map.GetMap(GameMapType.A, GameMap.CozyTower) ;
        }


        //Delayboard delayboard;

        static List<MobileFlipbook> mFlipbook;
        static List<SyncMobile> sMobList;
        //static List<Mobile> mobList;

        public DebugScene()
        {
            sceneTimespan = 1f;
            hasRequestedNextScene = false;

            Camera.Zoom = new Vector2(1, 1);

            #region Mobile List
            /*
            mobList = new List<Mobile>();
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[0].Owner, new Vector2(100 * 1, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[1].Owner, new Vector2(100 * 2, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[2].Owner, new Vector2(100 * 3, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[3].Owner, new Vector2(100 * 4, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[4].Owner, new Vector2(100 * 5, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[5].Owner, new Vector2(100 * 6, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[6].Owner, new Vector2(100 * 7, 100)));
            mobList.Add(ActorBuilder.BuildMobile(MobileType.Knight, sMobList[7].Owner, new Vector2(100 * 8, 100)));

            for (int i = 0; i < 8; i++) ;*/

            MobileList.ForEach((x) =>
            {
                x.SyncMobile = sMobList[MobileList.IndexOf(x)];
            });
            #endregion

            MatchMetadata = new MatchMetadata();
            MatchMetadata.CurrentTurnOwner = MobileList[0].SyncMobile;

            //optionsMenu.ShouldRender = true;

            //Popup create char
            popupSelectMobile = new PopupSelectMobile((x) => { }, () => { });
            PopupHandler.Add(popupSelectMobile);

            //MobFlipbook

            mFlipbook = new List<MobileFlipbook>();

            for (int k = 0; k < 21; k++)
                mFlipbook.Add(MobileFlipbook.CreateMobileFlipbook(MobileType.Lightning, new Vector2(-500 + 100 * (k % 5), -500 + 100 * (k / 5))));

            int i = 0;

            mFlipbook[i++].ChangeState(MobileFlipbookState.Stand, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.StandLowHealth, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.Moving, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.MovingLowHealth, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.UnableToMove, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.Emotion1, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.Emotion2, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.BeingDamaged1, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.BeingDamaged2, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.BeingShocked, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.BeingFrozen, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.ChargingS1, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.ShootingS1, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.ChargingS2, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.ShootingS2, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.ChargingSS, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.ShootingSS, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.UsingItem, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.Dead, true);

            mFlipbook[i++].ChangeState(MobileFlipbookState.Falling, true);
            mFlipbook[i++].ChangeState(MobileFlipbookState.All, true);
        }

        public override void Initialize(GraphicsDevice GraphicsDevice, SpriteBatch SpriteBatch)
        {
            base.Initialize(GraphicsDevice, SpriteBatch);
        }


        Random r = new Random();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (InputHandler.IsBeingPressed(Keys.F1))
            {
                //CurrentTurnOwner.LoseTurn();
                MobileList[0].GrantTurn();
                MobileList[0].LayerDepth = 0.6f;
                //MobileList.ForEach((x) => x.Movement.RemainingStepsThisTurn = 100000);
            }

            if (InputHandler.IsBeingPressed(Keys.F2))
            {
                new Thread(() =>
                {
                    lock (MobileList)
                        MobileList[r.Next(0, 8)].Die();

                }).Start();
            }

            if (InputHandler.IsBeingPressed(Keys.F3))
            {
                PopupHandler.Add(new PopupGameResults());
            }

            if (InputHandler.IsBeingPressed(Keys.F4))
            {
                MatchMetadata.WindForce = r.Next(0, 35);
                MatchMetadata.WindAngleDegrees = r.Next(0, 360);
                HUD.windCompass.ChangeWind(MatchMetadata.WindAngleDegrees, MatchMetadata.WindForce);
            }

            if (InputHandler.IsBeingPressed(Keys.F5))
            {
                WeatherHandler.Add(WeatherType.Mirror, new Vector2(-500, -Topography.MapHeight / 2));
            }

            if (InputHandler.IsBeingPressed(Keys.F6))
            {
                WeatherHandler.Add(WeatherType.Weakness, new Vector2(-400, -Topography.MapHeight / 2));
            }

            if (InputHandler.IsBeingPressed(Keys.F7))
            {
                WeatherHandler.Add(WeatherType.Force, new Vector2(-300, -Topography.MapHeight / 2));
            }

            if (InputHandler.IsBeingPressed(Keys.F8))
            {
                WeatherHandler.Add(WeatherType.Tornado, new Vector2(-200, -Topography.MapHeight / 2));
            }

            if (InputHandler.IsBeingPressed(Keys.F9))
            {
                WeatherHandler.Add(WeatherType.Electricity, new Vector2(200, -Topography.MapHeight / 2));
            }
            //optionsMenu.Update(GameTime, MouseState, PreviousMouseState, KeyboardState, PreviousKeyboardState);
        }

        public override void Draw(GameTime gameTime)
        {
            mFlipbook.ForEach((x) => x.Draw(gameTime, spriteBatch));
            base.Draw(gameTime);

            //optionsMenu.Draw(GameTime, spriteBatch);
            //delayboard.Draw(GameTime, SpriteBatch);
            //mobList.ForEach((x) => x.Draw(GameTime, SpriteBatch));
        }
    }
}
