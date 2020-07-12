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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
        WoodHammer1 = 1, WoodHammer2 = 2, StoneHammer1 = 3, StoneHammer2 = 4,
        Axe1 = 5, Axe2 = 6, SilverAxe1 = 7, SilverAxe2 = 8, GoldenAxe1 = 9, GoldenAxe2 = 10,
        DAxe1 = 11, DAxe2 = 12, DSilverAxe1 = 13, DSilverAxe2 = 14, DGoldenAxe1 = 15, DGoldenAxe2 = 16,

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
        //Player Credentials/Storable objects
        [Key]
        public int ID { get; set; }

        [Required, MinLength(4), MaxLength(30), Index(IsUnique = true)]
        public string Nickname { get; set; }

        [Required, MinLength(5), MaxLength(172)]
        public string Password { get; set; }

        [Required, MaxLength(60), Index(IsUnique = true)]
        public string Email { get; set; }

        [Required]
        public Gender CharacterGender { get; set; }

        public Guild Guild { get; set; }

        [JsonIgnore]
        public List<Player> FriendList { get; set; }

        [NotMapped]
        public SecurityToken SecurityToken { get; set; }

        //In-game variables
        [NotMapped]
        public PlayerRoomStatus PlayerRoomStatus { get; set; }
        [NotMapped]
        public PlayerTeam PlayerTeam { get; set; }
        [NotMapped]
        public PlayerRank PlayerRank { get; set; }
        [NotMapped]
        public PlayerStatus PlayerStatus { get; set; }

        [Required]
        public MobileType PrimaryMobile { get; set; }
        [Required]
        public MobileType SecondaryMobile { get; set; }
        public float LeavePercentage { get; set; }

        [JsonIgnore, NotMapped]
        public PlayerRoomStatus PlayerLoadingStatus { get; set; }
        [JsonIgnore, NotMapped]
        public int LoadingScreenPercentage { get; set; }
        [JsonIgnore, NotMapped]
        public PlayerNavigation PlayerNavigation;

        //Avatar Region
        public int[] Avatar { get; set; }
        
        [JsonIgnore, NotMapped] public int EquippedAvatarHead => Avatar[0];
        [JsonIgnore, NotMapped] public int EquippedAvatarBody => Avatar[1];
        [JsonIgnore, NotMapped] public int EquippedAvatarGoggles => Avatar[2];
        [JsonIgnore, NotMapped] public int EquippedAvatarFlag => Avatar[3];
        [JsonIgnore, NotMapped] public int EquippedAvatarExItem => Avatar[4];
        [JsonIgnore, NotMapped] public int EquippedAvatarPet => Avatar[5];
        [JsonIgnore, NotMapped] public int EquippedAvatarMisc => Avatar[6];
        [JsonIgnore, NotMapped] public int EquippedAvatarExtra => Avatar[7];

        [JsonIgnore] public List<int> AvatarHead;
        [JsonIgnore] public List<int> AvatarBody;
        [JsonIgnore] public List<int> AvatarGoggles;
        [JsonIgnore] public List<int> AvatarFlag;
        [JsonIgnore] public List<int> AvatarExItem;
        [JsonIgnore] public List<int> AvatarPet;
        [JsonIgnore] public List<int> AvatarMisc;
        [JsonIgnore] public List<int> AvatarExtra;

        //Status Region
        public int[] Status { get; set; }

        [JsonIgnore, NotMapped] public int Attack => Avatar[0];
        [JsonIgnore, NotMapped] public int Health => Avatar[1];
        [JsonIgnore, NotMapped] public int Defense => Avatar[2];
        [JsonIgnore, NotMapped] public int Regeneration => Avatar[3];
        [JsonIgnore, NotMapped] public int AttackDelay => Avatar[4];
        [JsonIgnore, NotMapped] public int ItemDelay => Avatar[5];
        [JsonIgnore, NotMapped] public int Dig => Avatar[6];
        [JsonIgnore, NotMapped] public int Popularity => Avatar[7];

        public Player()
        {
            PlayerNavigation = PlayerNavigation.InGameMenus;
            Avatar = new int[8];
            Status = new int[8];
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
