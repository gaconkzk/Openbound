using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Asset;
using OpenBound.GameComponents.Interface.Builder;
using OpenBound.GameComponents.Interface.Interactive;
using OpenBound.GameComponents.Interface.Interactive.AvatarShop;
using OpenBound.GameComponents.Interface.Interactive.GameList;
using OpenBound.GameComponents.Interface.Popup;
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Pawn;
using OpenBound.GameComponents.Pawn.Unit;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Sync;
using Openbound_Network_Object_Library.Entity.Text;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace OpenBound.GameComponents.Level.Scene.Menu
{
    public class AvatarSearchFilter
    {
        public AvatarCategory AvatarCategory;
        public string AvatarName;
        public int CurrentPage;
        public int LastPage;
        public bool IsRenderingInventory;

        public AvatarSearchFilter() { }
    }
    public class AvatarShop : GameScene
    {
        private List<AnimatedButton> animatedButtonList;

        private List<Sprite> spriteList;
        private Sprite foreground1, foreground2;

        public List<AvatarButton> avatarButtonList;
        private List<Button> buttonList;

        private List<SpriteText> spriteTextList;
        private TextField searchTextField;

        private Button tabInventoryButton, tabShopButton;
        private SpriteText inventorySpriteText, shopSpriteText;

        private Rider riderPreview;

        private AttributeMenu attributeMenu;
        public AvatarMetadata selectedAvatarMetadata;

        private SpriteText filterCurrentPage, filterLastPage, filterDivider;

        // Filter buttons
        private List<AnimatedButton> filterButtonList;

        // Footer buttons
        private AnimatedButton tryButton, buyButton, giftButton;

        // Filtering options
        private AvatarSearchFilter searchFilter;
        private AnimatedButton filterLeftButton, filterRightButton;

        public AvatarShop()
        {
            GameInformation.Instance.PlayerInformation = new Player()
            {
                ID = 1,
                CharacterGender = Gender.Male,
                Email = "c@c.c",
                Guild = new Guild() { ID = 1, Name = "Zica", Tag = "Zik" },
                LeavePercentage = 0,
                Nickname = "Winged",
                Password = "123",
                PlayerRank = PlayerRank.GM,
                Gold = 500,
                Cash = 400,
            };

            Background = new Sprite(@"Interface/InGame/Scene/AvatarShop/Background",
                position: Parameter.ScreenCenter,
                layerDepth: DepthParameter.Background,
                shouldCopyAsset: false);

            animatedButtonList = new List<AnimatedButton>();
            filterButtonList = new List<AnimatedButton>();
            spriteList = new List<Sprite>();
            avatarButtonList = new List<AvatarButton>();
            spriteTextList = new List<SpriteText>();
            buttonList = new List<Button>();

            foreground1 = new Sprite("Interface/InGame/Scene/AvatarShop/Foreground1", Vector2.Zero, DepthParameter.Background);
            foreground2 = new Sprite("Interface/InGame/Scene/AvatarShop/Foreground2", Vector2.Zero, DepthParameter.Background);
            foreground2.Color = Color.Transparent;
            foreground1.Pivot = foreground2.Pivot = Vector2.Zero;
            foreground1.Position = foreground2.Position = Parameter.ScreenCenter - Background.Pivot;

            spriteList.Add(foreground1);
            spriteList.Add(foreground2);

            AddBottomAnimatedButtonsToScene();
            AddFilterAnimatedButtonsToScene();
            AddSearchTextToScene();
            AddTabControlToScene();

            PopupHandler.PopupGameOptions.OnClose = OptionsCloseAction;

            //Room Button
            riderPreview = new Rider(Facing.Right, GameInformation.Instance.PlayerInformation, Parameter.ScreenCenter + new Vector2(-280, -60));

            //AttributeButton
            attributeMenu = new AttributeMenu(new Vector2(-285, -235), GameInformation.Instance.PlayerInformation);

            //Buttons standard preset
            tryButton.Disable();
            buyButton.Disable();
            giftButton.Disable();

            //Since Hats is the first selected filter, start the window rendering all hats
            //Filtering
            filterButtonList[0].Disable(); // 0 = head
            searchFilter = new AvatarSearchFilter();
            searchFilter.AvatarCategory = AvatarCategory.Head;

            //Filtering Text
            Vector2 dividerPosition = (filterLeftButton.Flipbook.Position + filterRightButton.Flipbook.Position) / 2 + new Vector2(0, -10);
            filterDivider = new SpriteText(FontTextType.Consolas14, "/", Color.White,
                Alignment.Center, DepthParameter.InterfaceButton,
                dividerPosition, Color.Black);
            filterCurrentPage = new SpriteText(FontTextType.Consolas14, "", Color.White,
                Alignment.Right, DepthParameter.InterfaceButton,
                dividerPosition - new Vector2(6, 0), Color.Black);
            filterLastPage = new SpriteText(FontTextType.Consolas14, "", Color.White,
                Alignment.Left, DepthParameter.InterfaceButton,
                dividerPosition + new Vector2(6, 0), Color.Black);

            spriteTextList.Add(filterCurrentPage);
            spriteTextList.Add(filterLastPage);
            spriteTextList.Add(filterDivider);

            UpdateFilter(AvatarCategory.Head, 0);
        }

        #region Button Actions
        private void OptionsCloseAction(object sender)
        {
            //roomButtonList.ForEach((x) => x.ShouldUpdate = true);
            animatedButtonList.ForEach((x) => x.ShouldUpdate = true);
            ((Button)sender).Enable();
        }

        private void OptionsAction(object sender)
        {
            //roomButtonList.ForEach((x) => x.ShouldUpdate = false);
            animatedButtonList.ForEach((x) => x.ShouldUpdate = false);
            PopupHandler.PopupGameOptions.ShouldRender = true;
            ((AnimatedButton)sender).Disable();
        }

        private void ExitDoorAction(object sender)
        {
            //ServerInformationBroker.Instance.DisconnectFromGameServer();
            SceneHandler.Instance.RequestSceneChange(SceneType.GameList, TransitionEffectType.RotatingRectangles);
            ((AnimatedButton)sender).Disable();
        }

        private void AvatarButtonClick(object sender)
        {
            AvatarButton avatarButton = ((AvatarButton)sender);
            selectedAvatarMetadata = avatarButton.AvatarMetadata;
            avatarButtonList.ForEach((x) => { x.Disable(); x.Enable(); });

            tryButton.Enable();
            buyButton.Enable();
            giftButton.Enable();

            avatarButton.ChangeButtonState(ButtonAnimationState.Activated, true);
        }

        private void TryButtonAction(object sender)
        {
            if (selectedAvatarMetadata != null)
            {
                riderPreview.ReplaceAvatar(selectedAvatarMetadata);
            }

            tryButton.Disable();
        }

        private void BuyButtonAction(object sender)
        {
            PopupBuyAvatar pba = new PopupBuyAvatar(selectedAvatarMetadata, (o) => {
                buyButton.Enable();
            }, default, default);

            PopupHandler.Add(pba);
            buyButton.Disable();
        }

        private void ShopButtonAction(object sender)
        {
            shopSpriteText.BaseColor = shopSpriteText.Color = Color.White;

            tabInventoryButton.Enable();
            tabShopButton.Disable();

            foreground1.SetTransparency(1);
            foreground2.SetTransparency(0);

            searchFilter.IsRenderingInventory = false;
            UpdateFilter(searchFilter.AvatarCategory, 0);
        }

        private void InventoryButtonAction(object sender)
        {
            inventorySpriteText.BaseColor = inventorySpriteText.Color = Color.White;

            tabShopButton.Enable();
            tabInventoryButton.Disable();
            
            foreground1.SetTransparency(0);
            foreground2.SetTransparency(1);

            searchFilter.IsRenderingInventory = true;
            UpdateFilter(searchFilter.AvatarCategory, 0);
        }

        public void FilterButtonAction(object sender, AvatarCategory category)
        {
            filterButtonList.ForEach((x) =>
            {
                if (x != sender) x.Enable();
            });

            ((AnimatedButton)sender).Disable();

            UpdateFilter(category, 0);
        }
        #endregion

        public void UpdateFilter(AvatarCategory category, int currentPage)
        {
            searchFilter.AvatarCategory = category;
            searchFilter.CurrentPage = currentPage;

            IEnumerable<AvatarMetadata> metadataList = (IEnumerable<AvatarMetadata>)MetadataManager.ElementMetadata[$@"Avatar/{Gender.Male}/{searchFilter.AvatarCategory}/Metadata"];

            if (searchFilter.IsRenderingInventory)
                metadataList = metadataList.Where((x) => GameInformation.Instance.PlayerInformation.OwnedAvatar[category].Contains(x.ID));

            searchFilter.LastPage = Math.Max((int)Math.Ceiling(metadataList.Count() / 25f), 1);

            metadataList = metadataList.Skip(searchFilter.CurrentPage * 25).Take(searchFilter.CurrentPage * 25 + 25).ToList();

            RenderAvatarList(metadataList.ToList());

            filterCurrentPage.Text = (searchFilter.CurrentPage + 1).ToString();
            filterLastPage.Text = searchFilter.LastPage.ToString();

            //Update filter buttons
            if (currentPage == 0) filterLeftButton.Disable();
            else if (filterLeftButton.IsDisabled) filterLeftButton.Enable();

            if (currentPage + 1 == searchFilter.LastPage) filterRightButton.Disable();
            else if (filterRightButton.IsDisabled) filterRightButton.Enable();
        }

        public void RenderAvatarList(List<AvatarMetadata> metadataList)
        {
            avatarButtonList.Clear();

            const int offsetX = 108;
            const int offsetY = 86;

            for (int i = 0; i < 25 && i < metadataList.Count(); i++)
            {
                avatarButtonList.Add(new AvatarButton(
                    new Vector2(-95, -175) + new Vector2(offsetX * (i % 5), offsetY * (i / 5)),
                    metadataList[i],
                    AvatarButtonClick,
                    DepthParameter.InterfaceButton));
            }
        }

        public void AddTabControlToScene()
        {
            tabShopButton      = new Button(ButtonType.AvatarTabIndex, DepthParameter.InterfaceButton, ShopButtonAction,      new Vector2(-110, -285));
            tabInventoryButton = new Button(ButtonType.AvatarTabIndex, DepthParameter.InterfaceButton, InventoryButtonAction, new Vector2(-030, -285));
            tabShopButton.UpdateAttatchedPosition();
            tabInventoryButton.UpdateAttatchedPosition();

            tabShopButton.Disable();

            buttonList.Add(tabShopButton);
            buttonList.Add(tabInventoryButton);

            shopSpriteText      = new SpriteText(FontTextType.Consolas10, "Shop",      Color.White,     Alignment.Center, DepthParameter.InterfaceButtonText, tabShopButton.ButtonSprite.Position,      Color.Black);
            inventorySpriteText = new SpriteText(FontTextType.Consolas10, "Inventory", Color.LightGray, Alignment.Center, DepthParameter.InterfaceButtonText, tabInventoryButton.ButtonSprite.Position, Color.Black);
            shopSpriteText.Position -= (shopSpriteText.MeasureSize * Vector2.UnitY) / 3;
            inventorySpriteText.Position -= (inventorySpriteText.MeasureSize * Vector2.UnitY) / 3;
            spriteTextList.Add(shopSpriteText);
            spriteTextList.Add(inventorySpriteText);
        }

        public void AddSearchTextToScene()
        {
            SpriteText spriteText = new SpriteText(FontTextType.FontAwesome10, "" + (char)0xf002 + "12312313123123", Color.White, Alignment.Left, DepthParameter.InterfaceButtonText,
                Parameter.ScreenCenter + new Vector2(242, -253), Color.Black);
            spriteTextList.Add(spriteText);

            searchTextField = new TextField(Parameter.ScreenCenter + new Vector2(260, -252), 120, 16, 16, FontTextType.Consolas10, Color.White, DepthParameter.InterfaceButtonText, Color.Black);
            searchTextField.ActivateElement();
        }

        public void AddFilterAnimatedButtonsToScene()
        {
            Vector2 position = Parameter.ScreenCenter - new Vector2(130, 250);
            Vector2 offset = new Vector2(45, 0);
            int i = 0;
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Hat,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Head); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Body,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Body); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Goggles,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Goggles); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Flag,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Flag); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExItem,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.ExItem); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Pet,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Pet); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Necklace,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Misc); }));
            filterButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Ring,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Extra); }));
        }

        public void AddBottomAnimatedButtonsToScene()
        {
            int i = 0;

            Vector2 basePosition = Parameter.ScreenCenter - new Vector2(370, -265);
            Vector2 offset = new Vector2(54, 0);
            
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor,   basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Buddy,      basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Options,    basePosition + offset * i++, OptionsAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.CashCharge, basePosition + offset * i++, ExitDoorAction));

            basePosition += new Vector2(0, -55);
            i = 0;

            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));

            basePosition = Parameter.ScreenCenter - new Vector2(-370, -265) - 2 * offset;
            i = 0;

            animatedButtonList.Add(tryButton  = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Try,  basePosition + offset * i++, TryButtonAction));
            animatedButtonList.Add(buyButton  = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Buy,  basePosition + offset * i++, BuyButtonAction));
            animatedButtonList.Add(giftButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Gift, basePosition + offset * i++, ExitDoorAction));

            animatedButtonList.Add(filterLeftButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.LeftArrow,
                Parameter.ScreenCenter - new Vector2(-141 + 40 + 30, -245), (o) => { UpdateFilter(searchFilter.AvatarCategory, searchFilter.CurrentPage - 1); }));
            animatedButtonList.Add(filterRightButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.RightArrow,
                Parameter.ScreenCenter - new Vector2(-141 - 40, -245), (o) => { UpdateFilter(searchFilter.AvatarCategory, searchFilter.CurrentPage + 1); }));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            animatedButtonList.ForEach((x) => x.Update());
            filterButtonList.ForEach((x) => x.Update());
            avatarButtonList.ForEach((x) => x.Update(gameTime));
            searchTextField.Update(gameTime);
            buttonList.ForEach((x) => x.Update());
            attributeMenu.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            spriteList.ForEach((x) => x.Draw(null, spriteBatch));
            animatedButtonList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            filterButtonList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            avatarButtonList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            spriteTextList.ForEach((x) => x.Draw(spriteBatch));
            searchTextField.Draw(spriteBatch);
            buttonList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            riderPreview.Draw(gameTime, spriteBatch);
            attributeMenu.Draw(gameTime, spriteBatch);
        }
    }
}
