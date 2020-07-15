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

        public AvatarSearchFilter() { AvatarName = ""; }
    }
    public class AvatarShop : GameScene
    {
        //Interface animated buttons
        private List<AnimatedButton> animatedButtonList;
        private AnimatedButton cashChargeButton;

        private List<Sprite> spriteList;
        private Sprite foreground1, foreground2;

        public List<AvatarButton> avatarButtonList;
        private List<Button> buttonList;

        private List<SpriteText> spriteTextList;
        private TextField searchTextField;

        private Button tabInventoryButton, tabShopButton;
        private SpriteText inventorySpriteText, shopSpriteText;

        //Preview
        private Rider shopRiderPreview, inventoryRiderPreview;
        private SpriteText avatarPreviewSpriteText;

        //In-Game Preview
        private InGamePreview shopInGamePreview, inventoryInGamePreview;
        private SpriteText inGamePreviewSpriteText;

        private AttributeMenu attributeMenu;
        public AvatarMetadata selectedAvatarMetadata;

        private SpriteText filterCurrentPage, filterLastPage, filterDivider;

        // Filter buttons
        private List<AnimatedButton> filterButtonList;
        private AnimatedButton filterHatButton, filterBodyButton, filterGogglesButton, filterFlagButton;
        private AnimatedButton filterExItemButton, filterPetButton, filterMiscButton, filterExtraButton;

        // Footer buttons
        private AnimatedButton tryButton, buyButton, giftButton;

        // Filtering options
        private AvatarSearchFilter searchFilter;
        private AnimatedButton filterLeftButton, filterRightButton;

        // Popups
        private PopupBuyAvatar popupBuyAvatar;

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
                layerDepth: DepthParameter.Foreground,
                shouldCopyAsset: false);

            animatedButtonList = new List<AnimatedButton>();
            filterButtonList = new List<AnimatedButton>();
            spriteList = new List<Sprite>();
            avatarButtonList = new List<AvatarButton>();
            spriteTextList = new List<SpriteText>();
            buttonList = new List<Button>();

            foreground1 = new Sprite("Interface/InGame/Scene/AvatarShop/Foreground1", Vector2.Zero, DepthParameter.Foreground);
            foreground2 = new Sprite("Interface/InGame/Scene/AvatarShop/Foreground2", Vector2.Zero, DepthParameter.Foreground);
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
            shopRiderPreview = new Rider(Facing.Right, GameInformation.Instance.PlayerInformation, Parameter.ScreenCenter + new Vector2(-280, -40));
            inventoryRiderPreview = new Rider(Facing.Right, GameInformation.Instance.PlayerInformation, Parameter.ScreenCenter + new Vector2(-280, -40));
            inventoryRiderPreview.Hide();

            avatarPreviewSpriteText = new SpriteText(FontTextType.Consolas10, Parameter.PreviewTextAvatarShop,
                Color.White, Alignment.Left, DepthParameter.InterfaceButton,
                Parameter.ScreenCenter - new Vector2(385, 110), Color.Black);

            //Other preview
            shopInGamePreview = new InGamePreview(Parameter.ScreenCenter + new Vector2(-290, 80));
            inventoryInGamePreview = new InGamePreview(Parameter.ScreenCenter + new Vector2(-290, 80));
            inventoryInGamePreview.Hide();

            inGamePreviewSpriteText = new SpriteText(FontTextType.Consolas10, Parameter.InGamePreviewTextAvatarShop,
                Color.White, Alignment.Left, DepthParameter.InterfaceButton,
                Parameter.ScreenCenter - new Vector2(385, -17), Color.Black);

            //AttributeButton
            attributeMenu = new AttributeMenu(new Vector2(-285, -235), GameInformation.Instance.PlayerInformation);

            //Since Hats is the first selected filter, start the window rendering all hats
            //Filtering
            filterHatButton.ChangeButtonState(ButtonAnimationState.Activated, true);
            searchFilter = new AvatarSearchFilter();
            searchFilter.AvatarCategory = AvatarCategory.Hat;

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

            UpdateFilter(AvatarCategory.Hat, 0);

            //Buttons standard preset
            tryButton.Disable(true);
            buyButton.Disable(true);
            giftButton.Disable(true);
            cashChargeButton.Disable(true);
        }

        #region AddComponents Into Screen
        public void AddTabControlToScene()
        {
            tabShopButton = new Button(ButtonType.AvatarTabIndex, DepthParameter.InterfaceButton, ShopButtonAction, new Vector2(-110, -285));
            tabInventoryButton = new Button(ButtonType.AvatarTabIndex, DepthParameter.InterfaceButton, InventoryButtonAction, new Vector2(-030, -285));
            tabShopButton.UpdateAttatchedPosition();
            tabInventoryButton.UpdateAttatchedPosition();

            tabShopButton.Disable();

            buttonList.Add(tabShopButton);
            buttonList.Add(tabInventoryButton);

            shopSpriteText = new SpriteText(FontTextType.Consolas10, "Shop", Color.White, Alignment.Center, DepthParameter.InterfaceButtonText, tabShopButton.ButtonSprite.Position, Color.Black);
            inventorySpriteText = new SpriteText(FontTextType.Consolas10, "Inventory", Color.LightGray, Alignment.Center, DepthParameter.InterfaceButtonText, tabInventoryButton.ButtonSprite.Position, Color.Black);
            shopSpriteText.Position -= (shopSpriteText.MeasureSize * Vector2.UnitY) / 3;
            inventorySpriteText.Position -= (inventorySpriteText.MeasureSize * Vector2.UnitY) / 3;
            spriteTextList.Add(shopSpriteText);
            spriteTextList.Add(inventorySpriteText);
        }

        public void AddSearchTextToScene()
        {
            SpriteText spriteText = new SpriteText(FontTextType.FontAwesome10, "" + (char)0xf002, Color.White, Alignment.Left, DepthParameter.InterfaceButtonText,
                Parameter.ScreenCenter + new Vector2(242, -253), Color.Black);
            spriteTextList.Add(spriteText);

            searchTextField = new TextField(Parameter.ScreenCenter + new Vector2(260, -252), 120, 16, 16, FontTextType.Consolas10, Color.White, DepthParameter.InterfaceButtonText, Color.Black);
            searchTextField.ActivateElement();
            searchTextField.OnTextChange = OnFilterTextChange;
        }

        public void AddFilterAnimatedButtonsToScene()
        {
            Vector2 position = Parameter.ScreenCenter - new Vector2(130, 250);
            Vector2 offset = new Vector2(45, 0);
            int i = 0;
            filterButtonList.Add(filterHatButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Hat,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Hat); }));
            filterButtonList.Add(filterBodyButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Body,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Body); }));
            filterButtonList.Add(filterGogglesButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Goggles,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Goggles); }));
            filterButtonList.Add(filterFlagButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Flag,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Flag); }));
            filterButtonList.Add(filterExItemButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExItem,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.ExItem); }));
            filterButtonList.Add(filterPetButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Pet,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Pet); }));
            filterButtonList.Add(filterMiscButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Necklace,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Misc); }));
            filterButtonList.Add(filterExtraButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Ring,
                position + offset * i++, (o) => { FilterButtonAction(o, AvatarCategory.Extra); }));
        }

        public void AddBottomAnimatedButtonsToScene()
        {
            //Left Side
            int i = 0;

            Vector2 basePosition = Parameter.ScreenCenter - new Vector2(360, -265);
            Vector2 offset = new Vector2(72, 0);

            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Buddy,    basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Options,  basePosition + offset * i++, OptionsAction));

            /*
            basePosition += new Vector2(0, -55);
            i = 0;

            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));
            animatedButtonList.Add(AnimatedButtonBuilder.BuildButton(AnimatedButtonType.ExitDoor, basePosition + offset * i++, ExitDoorAction));*/

            //Right Side - Left
            basePosition = Parameter.ScreenCenter - new Vector2(129, -265);
            animatedButtonList.Add(cashChargeButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.CashCharge, basePosition, ExitDoorAction));

            //Right Side - Right
            offset = new Vector2(62, 0);
            basePosition = Parameter.ScreenCenter - new Vector2(-370, -265) - 2 * offset;
            i = 0;

            animatedButtonList.Add(tryButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Try, basePosition + offset * i++, TryButtonAction));
            animatedButtonList.Add(buyButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Buy, basePosition + offset * i++, BuyButtonAction));
            animatedButtonList.Add(giftButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.Gift, basePosition + offset * i++, ExitDoorAction));

            animatedButtonList.Add(filterLeftButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.LeftArrow,
                Parameter.ScreenCenter - new Vector2(-141 + 40 + 30, -245), (o) => { UpdateFilter(searchFilter.AvatarCategory, searchFilter.CurrentPage - 1); }));
            animatedButtonList.Add(filterRightButton = AnimatedButtonBuilder.BuildButton(AnimatedButtonType.RightArrow,
                Parameter.ScreenCenter - new Vector2(-141 - 40, -245), (o) => { UpdateFilter(searchFilter.AvatarCategory, searchFilter.CurrentPage + 1); }));
        }
        #endregion

        #region Networking
        public void OnAcquireAvatarAsyncCallback(object avatar)
        {
            AvatarMetadata avatarMetadata = ((AvatarMetadata)avatar);
            GameInformation.Instance.PlayerInformation.OwnedAvatar[avatarMetadata.Category].Add(avatarMetadata.ID);
            UpdateFilter(searchFilter.AvatarCategory, searchFilter.CurrentPage);
        }
        #endregion

        #region Button Actions
        private void OptionsCloseAction(object sender)
        {
            //roomButtonList.ForEach((x) => x.ShouldUpdate = true);
            animatedButtonList.ForEach((x) => x.ShouldUpdate = true);
            ((Button)sender).Enable();

            EnableInterfaceButtons();
        }

        private void OptionsAction(object sender)
        {
            //roomButtonList.ForEach((x) => x.ShouldUpdate = false);
            animatedButtonList.ForEach((x) => x.ShouldUpdate = false);
            PopupHandler.PopupGameOptions.ShouldRender = true;
            ((AnimatedButton)sender).Disable(true);

            DisableInterfaceButtons();
        }

        private void ExitDoorAction(object sender)
        {
            //ServerInformationBroker.Instance.DisconnectFromGameServer();
            SceneHandler.Instance.RequestSceneChange(SceneType.GameList, TransitionEffectType.RotatingRectangles);
            ((AnimatedButton)sender).Disable(true);
        }

        private void AvatarButtonAction(object sender)
        {
            AvatarButton avatarButton = ((AvatarButton)sender);
            selectedAvatarMetadata = avatarButton.AvatarMetadata;

            // If it is a inventory button
            if (searchFilter.IsRenderingInventory)
            {
                // Equip
                GameInformation.Instance.PlayerInformation
                    .EquipAvatar(selectedAvatarMetadata.Category, selectedAvatarMetadata.ID);

                // Update Equipped
                avatarButtonList.ForEach((x) => x.HideEquippedIndicator());
                avatarButton.ShowEquippedIndicator();

                inventoryRiderPreview.ReplaceAvatar(selectedAvatarMetadata);
                inventoryInGamePreview.ReplaceAvatar(selectedAvatarMetadata);

                tryButton.Disable(true);
                buyButton.Disable(true);

                return;
            }

            // If it isn't an inventory button
            avatarButtonList.ForEach((x) => { x.Disable(); x.Enable(); });
            avatarButton.ChangeButtonState(ButtonAnimationState.Activated, true);

            if (shopRiderPreview.GetEquippedAvatarID(selectedAvatarMetadata.Category) != selectedAvatarMetadata.ID)
                tryButton.Enable();
            else
                tryButton.Disable(true);

            if (GameInformation.Instance.PlayerInformation.Gold >= selectedAvatarMetadata.GoldPrice ||
                GameInformation.Instance.PlayerInformation.Cash >= selectedAvatarMetadata.CashPrice)
            {
                if (!GameInformation.Instance.PlayerInformation
                    .OwnedAvatar[selectedAvatarMetadata.Category].Contains(selectedAvatarMetadata.ID))
                    buyButton.Enable();
                else
                    buyButton.Disable(true);
            }
        }

        private void TryButtonAction(object sender)
        {
            if (selectedAvatarMetadata != null)
            {
                shopRiderPreview.ReplaceAvatar(selectedAvatarMetadata);
                shopInGamePreview.ReplaceAvatar(selectedAvatarMetadata);

                foreach (AvatarButton button in avatarButtonList)
                {
                    button.HideEquippedIndicator();

                    if (button.AvatarMetadata.ID == shopRiderPreview.GetEquippedAvatarID(button.AvatarMetadata.Category))
                        button.ShowEquippedIndicator();
                }
            }

            tryButton.Disable(true);
        }

        private void BuyButtonAction(object sender)
        {
            popupBuyAvatar = new PopupBuyAvatar(selectedAvatarMetadata,
                OnClosePopupDialog,
                OnBuyCash,
                OnBuyGold);

            PopupHandler.Add(popupBuyAvatar);

            buyButton.Disable(true);
            tryButton.Disable(true);

            DisableInterfaceButtons();
        }

        private void ShopButtonAction(object sender)
        {
            shopSpriteText.BaseColor = shopSpriteText.Color = Color.White;

            tabInventoryButton.Enable();
            tabShopButton.Disable();

            foreground1.SetTransparency(1);
            foreground2.SetTransparency(0);

            shopRiderPreview.Show();
            inventoryRiderPreview.Hide();

            shopInGamePreview.Show();
            inventoryInGamePreview.Hide();

            avatarPreviewSpriteText.Text = Parameter.PreviewTextAvatarShop;
            inGamePreviewSpriteText.Text = Parameter.InGamePreviewTextAvatarShop;

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

            shopRiderPreview.Hide();
            inventoryRiderPreview.Show();

            shopInGamePreview.Hide();
            inventoryInGamePreview.Show();

            avatarPreviewSpriteText.Text = Parameter.PreviewTextAvatarShopEquipped;
            inGamePreviewSpriteText.Text = Parameter.InGamePreviewTextAvatarShopEquipped;

            searchFilter.IsRenderingInventory = true;
            UpdateFilter(searchFilter.AvatarCategory, 0);

            buyButton.Disable(true);
            tryButton.Disable(true);
        }

        public void FilterButtonAction(object sender, AvatarCategory category)
        {
            filterButtonList.ForEach((x) =>
            {
                if (x != sender) x.Enable();
            });

            ((AnimatedButton)sender).ChangeButtonState(ButtonAnimationState.Activated);

            UpdateFilter(category, 0);
        }

        public void OnClosePopupDialog(object sender)
        {
            EnableInterfaceButtons();

            buyButton.Enable();
            tryButton.Enable();

        }

        public void OnBuyCash(object sender)
        {
            //send request
            GameInformation.Instance.PlayerInformation.Cash -= selectedAvatarMetadata.CashPrice;
            OnAcquireAvatarAsyncCallback(selectedAvatarMetadata);
            PopupHandler.Remove(popupBuyAvatar);

            attributeMenu.RefreshCurrencyValues();

            EnableInterfaceButtons();

            buyButton.Disable(true);
            tryButton.Disable(true);
        }

        public void OnBuyGold(object sender)
        {
            //send request
            GameInformation.Instance.PlayerInformation.Gold -= selectedAvatarMetadata.GoldPrice;
            OnAcquireAvatarAsyncCallback(selectedAvatarMetadata);
            PopupHandler.Remove(popupBuyAvatar);

            attributeMenu.RefreshCurrencyValues();

            EnableInterfaceButtons();

            buyButton.Disable(true);
            tryButton.Disable(true);
        }
        #endregion

        #region Action handlers
        public void OnFilterTextChange(string newText)
        {
            searchFilter.AvatarName = newText;
            UpdateFilter(searchFilter.AvatarCategory, searchFilter.CurrentPage);
        }
        #endregion

        private void DisableInterfaceButtons()
        {
            animatedButtonList.ForEach((x) => x.Disable(true));
            filterButtonList.ForEach((x) => x.Disable(true));
            avatarButtonList.ForEach((x) => x.ShouldUpdate = false);
            tabInventoryButton.Disable();
            tabShopButton.Disable();
            searchTextField.Disable();
            searchTextField.DeactivateElement();
        }

        private void EnableInterfaceButtons()
        {
            animatedButtonList.ForEach((x) => x.Enable());
            filterButtonList.ForEach((x) => x.Enable());
            avatarButtonList.ForEach((x) => x.ShouldUpdate = true);

            if (searchFilter.IsRenderingInventory)
                tabShopButton.Enable();
            else
                tabInventoryButton.Enable();

            AnimatedButton button;

            switch (searchFilter.AvatarCategory)
            {
                case AvatarCategory.Hat: button = filterHatButton; break;
                case AvatarCategory.Body: button = filterBodyButton; break;
                case AvatarCategory.Goggles: button = filterGogglesButton; break;
                case AvatarCategory.Flag: button = filterFlagButton; break;
                case AvatarCategory.ExItem: button = filterExItemButton; break;
                case AvatarCategory.Pet: button = filterPetButton; break;
                case AvatarCategory.Misc: button = filterMiscButton; break;
                default: button = filterExtraButton; break;
            }

            button.ChangeButtonState(ButtonAnimationState.Activated);

            giftButton.Disable(true);
            cashChargeButton.Disable(true);

            //Update filter buttons
            if (searchFilter.CurrentPage == 0) filterLeftButton.Disable(true);
            else if (filterLeftButton.IsDisabled) filterLeftButton.Enable();

            if (searchFilter.CurrentPage + 1 == searchFilter.LastPage) filterRightButton.Disable(true);
            else if (filterRightButton.IsDisabled) filterRightButton.Enable();

            //Search text field
            searchTextField.Enable();
            searchTextField.ActivateElement();
        }

        public void UpdateFilter(AvatarCategory category, int currentPage)
        {
            searchFilter.AvatarCategory = category;
            searchFilter.CurrentPage = currentPage;

            IEnumerable<AvatarMetadata> metadataList = (IEnumerable<AvatarMetadata>)MetadataManager.ElementMetadata[$@"Avatar/{Gender.Male}/{searchFilter.AvatarCategory}/Metadata"];

            //Text filter
            metadataList = metadataList.Where((x) => x.Name.ToLower().Contains(searchFilter.AvatarName));

            if (searchFilter.IsRenderingInventory)
                metadataList = metadataList.Where((x) => GameInformation.Instance.PlayerInformation.OwnedAvatar[category].Contains(x.ID));

            searchFilter.LastPage = Math.Max((int)Math.Ceiling(metadataList.Count() / 25f), 1);

            metadataList = metadataList.Skip(searchFilter.CurrentPage * 25).Take(searchFilter.CurrentPage * 25 + 25).ToList();

            RenderAvatarList(metadataList.ToList());

            filterCurrentPage.Text = (searchFilter.CurrentPage + 1).ToString();
            filterLastPage.Text = searchFilter.LastPage.ToString();

            //Update filter buttons
            if (currentPage == 0) filterLeftButton.Disable(true);
            else if (filterLeftButton.IsDisabled) filterLeftButton.Enable();

            if (currentPage + 1 == searchFilter.LastPage) filterRightButton.Disable(true);
            else if (filterRightButton.IsDisabled) filterRightButton.Enable();
        }

        public void RenderAvatarList(List<AvatarMetadata> metadataList)
        {
            avatarButtonList.Clear();

            const int offsetX = 108;
            const int offsetY = 86;

            for (int i = 0; i < 25 && i < metadataList.Count(); i++)
            {
                AvatarButton button = new AvatarButton(
                    new Vector2(-95, -175) + new Vector2(offsetX * (i % 5), offsetY * (i / 5)),
                    metadataList[i], AvatarButtonAction, DepthParameter.InterfaceButton);

                button.HideEquippedIndicator();

                if (searchFilter.IsRenderingInventory)
                {
                    if (inventoryRiderPreview.GetEquippedAvatarID(metadataList[i].Category) == metadataList[i].ID)
                        button.ShowEquippedIndicator();
                }
                else
                {
                    if (shopRiderPreview.GetEquippedAvatarID(metadataList[i].Category) == metadataList[i].ID)
                        button.ShowEquippedIndicator();
                }

                avatarButtonList.Add(button);
            }
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

            shopInGamePreview.Update();
            inventoryInGamePreview.Update();
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
            shopRiderPreview.Draw(gameTime, spriteBatch);
            inventoryRiderPreview.Draw(gameTime, spriteBatch);

            avatarPreviewSpriteText.Draw(spriteBatch);
            inGamePreviewSpriteText.Draw(spriteBatch);
            
            attributeMenu.Draw(gameTime, spriteBatch);

            shopInGamePreview.Draw(gameTime, spriteBatch);
            inventoryInGamePreview.Draw(gameTime, spriteBatch);
        }
    }
}
