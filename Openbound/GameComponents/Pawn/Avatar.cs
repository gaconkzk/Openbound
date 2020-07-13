using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Pawn
{
    public class AvatarMetadata
    {
        public int ID;
        public string Name;
        public DateTime Date;
        public AvatarCategory Category;
        public Gender Gender;
        public int GoldPrice;
        public int CashPrice;
        public float[] Pivot;
        public int[] FrameDimensions;

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

    public class Avatar
    {
        static Dictionary<AvatarCategory, float> depthDictioary = new Dictionary<AvatarCategory, float>()
        {
            { AvatarCategory.Head,    DepthParameter.AvatarHead },
            { AvatarCategory.Body,    DepthParameter.AvatarBody },
            { AvatarCategory.Goggles, DepthParameter.AvatarGoggles },
            { AvatarCategory.Flag,    DepthParameter.AvatarFlag },
            { AvatarCategory.Pet,     DepthParameter.AvatarPet },
            { AvatarCategory.Misc,    DepthParameter.AvatarMisc },
            { AvatarCategory.Extra,   DepthParameter.AvatarExtra },
        };

        static Dictionary<AvatarCategory, Dictionary<AvatarState, AnimationInstance>> avatarState =
            new Dictionary<AvatarCategory, Dictionary<AvatarState, AnimationInstance>>()
            {
                {
                    AvatarCategory.Head,
                    new Dictionary<AvatarState, AnimationInstance>()
                    {
                        { AvatarState.Normal, new AnimationInstance() { StartingFrame = 11, EndingFrame = 21, TimePerFrame = 1 / 10f, AnimationType = AnimationType.Cycle } },
                        { AvatarState.Staring, new AnimationInstance() { StartingFrame = 00, EndingFrame = 10, TimePerFrame = 1 / 10f, AnimationType = AnimationType.Cycle } },
                    }
                },
                {
                    AvatarCategory.Body,
                    new Dictionary<AvatarState, AnimationInstance>()
                    {
                        { AvatarState.Normal,  new AnimationInstance() { StartingFrame = 00, EndingFrame = 10, TimePerFrame = 1 / 10f, AnimationType = AnimationType.Cycle } },
                    }
                },
            };

        public AvatarMetadata Metadata { get; private set; }
        public Flipbook Flipbook { get; private set; }

        public Vector2 Position { get => Flipbook.Position; set => Flipbook.Position = value; }
        public float Rotation { get => Flipbook.Rotation; set => Flipbook.Rotation = value; }

        public Avatar(AvatarMetadata metadata)
        {
            Metadata = metadata;
            Flipbook = new Flipbook(default, metadata.Pivot.ToVector2(),
                metadata.FrameDimensions[0], metadata.FrameDimensions[1],
                $"Graphics/Avatar/{metadata.Gender}/{metadata.Category}/{metadata.Name}",
                avatarState[metadata.Category][AvatarState.Normal],
                depthDictioary[metadata.Category]);
        }

        public void Flip() => Flipbook.Flip();
        public void Hide() => Flipbook.SetTransparency(0);
        public void Show() => Flipbook.SetTransparency(1);

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) => Flipbook.Draw(gameTime, spriteBatch);
    }
}
