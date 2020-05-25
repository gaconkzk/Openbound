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
using Openbound_Network_Object_Library.Entity;

namespace Openbound_Network_Object_Library.Models
{
    public class Guild
    {
        [Key]
        public int ID { get; set; }
        [Required, MinLength(2), MaxLength(6), Index(IsUnique = true)]
        public string Tag { get; set; }
        [Required, MinLength(4), MaxLength(20)]
        public string Name { get; set; }
        public List<Player> GuildMembers { get; set; }
        public Guild() { }
    }
}
