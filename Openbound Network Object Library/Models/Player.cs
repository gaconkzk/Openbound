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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Remoting.Messaging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Security;

namespace Openbound_Network_Object_Library.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public enum PlayerRoomStatus
    {
        Master, Ready, NotReady
    }

    public enum PlayerTeam
    {
        Red, Blue,
    }

    public enum PlayerRank
    {
        Chick = 0,

        WoodHammer1 = 1, WoodHammer2 = 2,
        StoneHammer1 = 3, StoneHammer2 = 4,

        Axe1 = 5, Axe2 = 6,
        SilverAxe1 = 7, SilverAxe2 = 8,
        GoldenAxe1 = 9, GoldenAxe2 = 10,

        DAxe1 = 11, DAxe2 = 12,
        DSilverAxe1 = 13, DSilverAxe2 = 14,
        DGoldenAxe1 = 15, DGoldenAxe2 = 16,

        Staff1 = 17, Staff2 = 18, Staff3 = 19, Staff4 = 20,

        Dragon1 = 21, Dragon2 = 22, Dragon3 = 23,

        Champion1 = 24, WorldChampion = 25, Vip = 26,
        
        GM = 27,
    }

    public enum PlayerStatus
    {
        Normal,
        PowerUser1,
        PowerUser2,
        PowerUser3
    }

    public enum PlayerNavigation
    {
        InGameMenus,
        InGameRoom,
        InLoadingScreen,
        InGame,
    }

    public class Player
    {
        #region Storable
        //Storable - Player Credentials
        [Key]
        public int ID { get; set; }

        [Required, MinLength(4), MaxLength(30), Index(IsUnique = true)]
        public string Nickname { get; set; }

        [Required, MinLength(5), MaxLength(172)]
        public string Password { get; set; }

        [Required, MaxLength(60), Index(IsUnique = true)]
        public string Email { get; set; }

        //Storable - Game Information
        [Required] public Gender CharacterGender { get; set; }

        public int Gold { get; set; }

        public int Cash { get; set; }

        public int Experience { get; set; }

        public Guild Guild { get; set; }

        public float LeavePercentage { get; set; }

        //Storable - User Preferences
        [Required] public MobileType PrimaryMobile { get; set; }
        [Required] public MobileType SecondaryMobile { get; set; }

        [JsonIgnore] public List<Player> FriendList { get; set; }
        [NotMapped] public SecurityToken SecurityToken { get; set; }

        [NotMapped] public int[] Avatar { get; set; }
        [NotMapped] public int[] Attribute { get; set; }
        #endregion

        //In-game variables - General
        [JsonIgnore, NotMapped] public PlayerNavigation PlayerNavigation;

        //In-game variables - Room
        [NotMapped] public PlayerRoomStatus PlayerRoomStatus { get; set; }
        [NotMapped] public PlayerTeam PlayerTeam { get; set; }
        [NotMapped] public PlayerRank PlayerRank { get; set; }
        [NotMapped] public PlayerStatus PlayerStatus { get; set; }
        
        //In-game variables - Loading
        [JsonIgnore, NotMapped] public PlayerRoomStatus PlayerLoadingStatus { get; set; }
        [JsonIgnore, NotMapped] public int LoadingScreenPercentage { get; set; }

        //Avatar Region
        [JsonIgnore] public int EquippedAvatarHat { get => Avatar[0]; set => Avatar[0] = value; }
        [JsonIgnore] public int EquippedAvatarBody { get => Avatar[1]; set => Avatar[1] = value; }
        [JsonIgnore] public int EquippedAvatarGoggles { get => Avatar[2]; set => Avatar[2] = value; }
        [JsonIgnore] public int EquippedAvatarFlag { get => Avatar[3]; set => Avatar[3] = value; }
        [JsonIgnore] public int EquippedAvatarExItem { get => Avatar[4]; set => Avatar[4] = value; }
        [JsonIgnore] public int EquippedAvatarPet { get => Avatar[5]; set => Avatar[5] = value; }
        [JsonIgnore] public int EquippedAvatarMisc { get => Avatar[6]; set => Avatar[6] = value; }
        [JsonIgnore] public int EquippedAvatarExtra { get => Avatar[7]; set => Avatar[7] = value; }

        [JsonIgnore, NotMapped] public Dictionary<AvatarCategory, HashSet<int>> OwnedAvatar;

        [JsonIgnore] public List<AvatarMetadata> AvatarMetadata;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarBody;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarGoggles;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarFlag;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarExItem;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarPet;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarMisc;
        //[JsonIgnore] public List<AvatarMetadata> OwnedAvatarExtra;

        //Attribute Region
        [JsonIgnore] public int Attack => Attribute[0];
        [JsonIgnore] public int Health => Attribute[1];
        [JsonIgnore] public int Defense => Attribute[2];
        [JsonIgnore] public int Regeneration => Attribute[3];
        [JsonIgnore] public int AttackDelay => Attribute[4];
        [JsonIgnore] public int ItemDelay => Attribute[5];
        [JsonIgnore] public int Dig => Attribute[6];
        [JsonIgnore] public int Popularity => Attribute[7];

        public Player()
        {
            PlayerNavigation = PlayerNavigation.InGameMenus;
            Avatar = new int[8];
            Attribute = new int[8];

            OwnedAvatar = new Dictionary<AvatarCategory, HashSet<int>>()
            {
                { AvatarCategory.Hat,     new HashSet<int>(){ 0, } },
                { AvatarCategory.Body,    new HashSet<int>(){ 0, } },
                { AvatarCategory.Goggles, new HashSet<int>() },
                { AvatarCategory.Flag,    new HashSet<int>() },
                { AvatarCategory.ExItem,  new HashSet<int>() },
                { AvatarCategory.Pet,     new HashSet<int>() },
                { AvatarCategory.Misc,    new HashSet<int>() },
                { AvatarCategory.Extra,   new HashSet<int>() },
            };
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public int GetEquippedAvatar(AvatarCategory avatarCategory)
        {
            switch (avatarCategory)
            {
                case AvatarCategory.Hat:     return EquippedAvatarHat;
                case AvatarCategory.Body:    return EquippedAvatarBody;
                case AvatarCategory.Goggles: return EquippedAvatarGoggles;
                case AvatarCategory.Flag:    return EquippedAvatarFlag;
                case AvatarCategory.ExItem:  return EquippedAvatarExItem;
                case AvatarCategory.Pet:     return EquippedAvatarPet;
                case AvatarCategory.Misc:    return EquippedAvatarMisc;
                default:                     return EquippedAvatarExtra;
            }
        }

        public void EquipAvatar(AvatarCategory avatarCategory, int avatarID)
        {
            if (!OwnedAvatar[avatarCategory].Contains(avatarID))
                return;

            switch (avatarCategory)
            {
                case AvatarCategory.Hat:     EquippedAvatarHat     = avatarID; return;
                case AvatarCategory.Body:    EquippedAvatarBody    = avatarID; return;
                case AvatarCategory.Goggles: EquippedAvatarGoggles = avatarID; return;
                case AvatarCategory.Flag:    EquippedAvatarFlag    = avatarID; return;
                case AvatarCategory.ExItem:  EquippedAvatarExItem  = avatarID; return;
                case AvatarCategory.Pet:     EquippedAvatarPet     = avatarID; return;
                case AvatarCategory.Misc:    EquippedAvatarMisc    = avatarID; return;
                default:                     EquippedAvatarExtra   = avatarID; return;
            }
        }

        public int GetCurrentAttributePoints()
        {
            return Math.Min((int)PlayerRank * NetworkObjectParameters.PlayerAttributePerLevel, NetworkObjectParameters.PlayerAttributeMaximumPerLevel);
        }
    }
}
