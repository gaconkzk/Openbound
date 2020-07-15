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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 1)]
        public int ID { get; set; }
        
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Date { get; set; }

        [Key, Column(Order = 0)]
        public AvatarCategory Category { get; set; }
        
        [Required]
        public Gender Gender { get; set; }
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

        public AvatarMetadata(int id, string name, DateTime date, AvatarCategory category, Gender gender, int goldPrice, int cashPrice, float[] pivot, int[] frameDimensions)
        {
            ID = id;
            Name = name;
            Date = date;
            Category = category;
            Gender = gender;
            GoldPrice = goldPrice;
            CashPrice = cashPrice;
            Pivot = pivot;
            FrameDimensions = frameDimensions;
        }
    }
}
