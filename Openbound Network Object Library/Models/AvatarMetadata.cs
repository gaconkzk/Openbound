using Newtonsoft.Json;
using Openbound_Network_Object_Library.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openbound_Network_Object_Library.Models
{
    public class AvatarMetadata
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int ID { get; set; }
        //[Required]
        [Key, Column(Order = 1)]
        public int ID { get; set; }
        [Key, Column(Order = 2)]
        public Gender Gender { get; set; }
        [Key, Column(Order = 3)]
        public AvatarCategory AvatarCategory { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int GoldPrice { get; set; }
        [Required]
        public int CashPrice { get; set; }
        [Required]
        public float PivotX { get; set; }
        [Required]
        public float PivotY { get; set; }
        [Required]
        public int FrameDimensionX { get; set; }
        [Required]
        public int FrameDimensionY { get; set; }

        [JsonIgnore]
        public virtual ICollection<Player> PlayerList { get; set; }

        [JsonIgnore, NotMapped]
        public float[] Pivot
        {
            get => new float[] { PivotX, PivotY };
            set
            {
                PivotX = value[0];
                PivotY = value[1];
            }
        }

        [JsonIgnore, NotMapped]
        public int[] FrameDimensions
        {
            get => new int[] { FrameDimensionX, FrameDimensionY };
            set
            {
                FrameDimensionX = value[0];
                FrameDimensionY = value[1];
            }
        }

        public AvatarMetadata() { }

        public AvatarMetadata(int partID, string name, DateTime date, AvatarCategory avatarCategory, Gender gender, int goldPrice, int cashPrice, float[] pivot, int[] frameDimensions)
        {
            ID = partID;
            Name = name;
            Date = date;
            AvatarCategory = avatarCategory;
            Gender = gender;
            GoldPrice = goldPrice;
            CashPrice = cashPrice;
            Pivot = pivot;
            FrameDimensions = frameDimensions;
        }

        public override int GetHashCode()
        {
            return ID ^ (int)AvatarCategory;
        }
    }
}
