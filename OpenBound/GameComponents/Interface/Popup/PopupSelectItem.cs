using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.Common;
using OpenBound.Extension;
using OpenBound.GameComponents.Animation;
using OpenBound.GameComponents.Interface.Interactive;
using OpenBound.GameComponents.Interface.Text;
using OpenBound.GameComponents.Level.Scene;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Entity.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Language = OpenBound.Common.Language;

namespace OpenBound.GameComponents.Interface.Popup
{
    public enum ItemCategory
    {
        Red, Green, Blue, Purple
    }

    public struct PopupMenuItemButtonPreset
    {
        public string ItemName, ItemDescription;
        public int ItemCost;
        public ItemCategory ItemCategory;
        public ButtonType ButtonType;
        public ItemType ItemType;
        public short Slots;

        public PopupMenuItemButtonPreset(string itemName, string itemDescription, int itemCost, ItemCategory itemCategory, ButtonType buttonType, ItemType itemType, short slots)
        {
            ItemName = itemName;
            ItemDescription = itemDescription;
            ItemCost = itemCost;
            ItemCategory = itemCategory;
            ButtonType = buttonType;
            ItemType = itemType;
            Slots = slots;
        }
    }

    public class ItemFilter
    {
        public int Slots;
        public ItemFilter(int slots)
        {
            Slots = slots;
        }
    }

    public class PopupSelectItem : PopupMenu
    {
        private static List<PopupMenuItemButtonPreset> popupMenuItemPresets = new List<PopupMenuItemButtonPreset>()
        {
            //2 Slots item
            new PopupMenuItemButtonPreset(Language.InGameItemDual,         Language.InGameItemDualDescription,         NetworkObjectParameters.InGameItemDualCost,         ItemCategory.Red,    ButtonType.SelectItemDual,         ItemType.Dual,         2),
            new PopupMenuItemButtonPreset(Language.InGameItemDualPlus,     Language.InGameItemDualPlusDescription,     NetworkObjectParameters.InGameItemDualPlusCost,     ItemCategory.Red,    ButtonType.SelectItemDualPlus,     ItemType.DualPlus,     2),
            new PopupMenuItemButtonPreset(Language.InGameItemThunder,      Language.InGameItemThunderDescription,      NetworkObjectParameters.InGameItemThunder,          ItemCategory.Red,    ButtonType.SelectItemThunder,      ItemType.Thunder,      2),
            new PopupMenuItemButtonPreset(Language.InGameItemEnergyUp2,    Language.InGameItemEnergyUp2Description,    NetworkObjectParameters.InGameItemEnergyUp2Cost,    ItemCategory.Green,  ButtonType.SelectItemEnergyUp2,    ItemType.EnergyUp2,    2),
            new PopupMenuItemButtonPreset(Language.InGameItemTeamTeleport, Language.InGameItemTeamTeleportDescription, NetworkObjectParameters.InGameItemTeamTeleportCost, ItemCategory.Purple, ButtonType.SelectItemTeamTeleport, ItemType.TeamTeleport, 2),
            new PopupMenuItemButtonPreset(Language.InGameItemTeleport,     Language.InGameItemTeleportDescription,     NetworkObjectParameters.InGameItemTeleportCost,     ItemCategory.Purple, ButtonType.SelectItemTeleport,     ItemType.Teleport,     2),

            //1 Slot item
            new PopupMenuItemButtonPreset(Language.InGameItemBlood,        Language.InGameItemBloodDescription,        NetworkObjectParameters.InGameItemBloodCost,        ItemCategory.Red,    ButtonType.SelectItemBlood,        ItemType.Blood,        1),
            new PopupMenuItemButtonPreset(Language.InGameItemBungeShot,    Language.InGameItemBungeShotDescription,    NetworkObjectParameters.InGameItemBungeShotCost,    ItemCategory.Red,    ButtonType.SelectItemBungeShot,    ItemType.BungeShot,    1),
            new PopupMenuItemButtonPreset(Language.InGameItemPowerUp,      Language.InGameItemPowerUpDescription,      NetworkObjectParameters.InGameItemPowerUpCost,      ItemCategory.Red,    ButtonType.SelectItemPowerUp,      ItemType.PowerUp,      1),
            new PopupMenuItemButtonPreset(Language.InGameItemChangeWind,   Language.InGameItemChangeWindDescription,   NetworkObjectParameters.InGameItemChangeWindCost,   ItemCategory.Blue,   ButtonType.SelectItemChangeWind,   ItemType.ChangeWind,   1),
            new PopupMenuItemButtonPreset(Language.InGameItemEnergyUp1,    Language.InGameItemEnergyUp1Description,    NetworkObjectParameters.InGameItemEnergyUp1Cost,    ItemCategory.Green,  ButtonType.SelectItemEnergyUp1,    ItemType.EnergyUp1,    1),//1 Slot item
        };

        List<Button> itemButtonList;
        List<Button> equippedButtonList;
        List<Button> unusedEquippedButtonList;
        int currentUsedSlots;

        ItemFilter itemFilter;

        SpriteText itemName, itemCost, itemDelayCost, itemDescription;

        //Tab Indexing
        TabButtonList tabButtonList;

        public PopupSelectItem() : base(true)
        {
            Background = new Sprite("Interface/Popup/Blue/SelectItem/Background", layerDepth: DepthParameter.InterfacePopupBackground);

            itemButtonList = new List<Button>();
            equippedButtonList = new List<Button>();
            unusedEquippedButtonList = new List<Button>();

            itemName        = new SpriteText(FontTextType.Consolas11, Language.InGameItemName,      Color.White, Alignment.Left,  DepthParameter.InterfacePopupText, default, outlineColor: Color.Black);
            itemDescription = new SpriteText(FontTextType.Consolas10, "",                           Color.White, Alignment.Left,  DepthParameter.InterfacePopupText, default, outlineColor: Color.Black);
            itemCost        = new SpriteText(FontTextType.Consolas11, Language.InGameItemCost,      Color.White, Alignment.Right, DepthParameter.InterfacePopupText, default, outlineColor: Color.Black);
            itemDelayCost   = new SpriteText(FontTextType.Consolas11, Language.InGameItemDelayCost, Color.White, Alignment.Left,  DepthParameter.InterfacePopupText, default, outlineColor: Color.Black);

            itemName.PositionOffset        = background.PositionOffset + new Vector2(-182, 68);
            itemDescription.PositionOffset = background.PositionOffset + new Vector2(-182, 88);
            itemCost.PositionOffset        = background.PositionOffset + new Vector2(182, 68);
            itemDelayCost.PositionOffset   = background.PositionOffset + new Vector2(60, 68);

            spriteTextList.Add(itemName);
            spriteTextList.Add(itemCost);
            spriteTextList.Add(itemDescription);
            spriteTextList.Add(itemDelayCost);

            LoadEquippedItemList();

            //Item tabs
            tabButtonList = new TabButtonList(background.PositionOffset - new Vector2(145, 148),
                new List<TabButtonParameter>()
                {
                    new TabButtonParameter(Language.InGameItemTab1, TabList2SlotsAction),
                    new TabButtonParameter(Language.InGameItemTab2, TabList1SlotAction),
                });

            //Item tab filtering
            itemFilter = new ItemFilter(2);
            CreateItemList(2);

            buttonList.Add(
                new Button(
                    ButtonType.Accept, 
                    DepthParameter.InterfacePopupButtons, 
                    CloseAction, 
                    buttonOffset: Background.PositionOffset + new Vector2(175, 178)));

            PopupHandler.Add(this);

            ShouldRender = false;
        }

        public void LoadEquippedItemList()
        {
            List<ItemType> iList = GameInformation.Instance.PlayerInformation.SelectedItemTypeList;
            GameInformation.Instance.PlayerInformation.SelectedItemTypeList = new List<ItemType>();

            foreach (ItemType it in iList)
            {
                AddItemIntoBarAction(popupMenuItemPresets.Find((x) => x.ItemType == it));
            }
        }

        public void TabList2SlotsAction(object sender)
        {
            CreateItemList(2);
        }

        public void TabList1SlotAction(object sender)
        {
            CreateItemList(1);
        }

        public void CreateItemList(int slots)
        {
            Vector2 basePosition;
            Vector2 positionOffset;
            int elementsPerLine;

            itemFilter.Slots = slots;
            
            if (slots == 2)
            {
                basePosition = background.PositionOffset + new Vector2(-79f, -78);
                positionOffset = new Vector2(85, 40);
                elementsPerLine = 4;
            }
            else
            {
                basePosition = background.PositionOffset + new Vector2(-40f * 3 - 18f, -78);
                positionOffset = new Vector2(40, 40);
                elementsPerLine = 9;
            }

            itemButtonList.ForEach((x) => buttonList.Remove(x));
            itemButtonList.Clear();

            List<PopupMenuItemButtonPreset> filteredPopupMenuItems =
                popupMenuItemPresets.Where((x) => x.Slots == itemFilter.Slots).ToList();

            for (int i = 0; i < filteredPopupMenuItems.Count(); i++)
            {
                PopupMenuItemButtonPreset preset = filteredPopupMenuItems[i];

                itemButtonList.Add(
                    BuildButton(
                        basePosition - positionOffset + positionOffset * new Vector2(i % elementsPerLine, i / elementsPerLine),
                        preset,
                        (o) => AddItemIntoBarAction(preset)));

                itemButtonList[i].Update();
            }

            buttonList.AddRange(itemButtonList);
        }

        public Button BuildButton(Vector2 position, PopupMenuItemButtonPreset preset, Action<object> action)
        {
            Button b = new Button(preset.ButtonType, DepthParameter.InterfacePopupButtons, action, buttonOffset: position);
            b.OnBeginHoover = (x) =>
            {
                itemName.Text = preset.ItemName;
                itemDescription.Text = preset.ItemDescription;
                itemCost.Text = preset.ItemCost + "";

                switch (preset.ItemCategory) {
                    case ItemCategory.Red:
                        itemName.Color = itemName.BaseColor = Parameter.TextColorItemRed;    break;
                    case ItemCategory.Blue:
                        itemName.Color = itemName.BaseColor = Parameter.TextColorItemBlue;   break;
                    case ItemCategory.Green:
                        itemName.Color = itemName.BaseColor = Parameter.TextColorItemGreen;  break;
                    case ItemCategory.Purple:
                        itemName.Color = itemName.BaseColor = Parameter.TextColorItemPurple; break;
                }
            };
            b.ButtonSprite.Pivot = Vector2.Zero;
            return b;
        }

        public void AddItemIntoBarAction(PopupMenuItemButtonPreset preset)
        {
            if (currentUsedSlots + preset.Slots > 6)
                return;

            equippedButtonList.Add(BuildButton(default, preset, (o) => RemoveItemFromBarAction(o, preset)));
            currentUsedSlots += preset.Slots;
            GameInformation.Instance.PlayerInformation.SelectedItemTypeList.Add(preset.ItemType);
            RealocateEquippedItems();
        }

        public void RemoveItemFromBarAction(object sender, PopupMenuItemButtonPreset preset)
        {
            unusedEquippedButtonList.Add((Button)sender);
            currentUsedSlots -= preset.Slots;
            GameInformation.Instance.PlayerInformation.SelectedItemTypeList.Remove(preset.ItemType);
            RealocateEquippedItems();
        }

        public override void RealocateElements(Vector2 delta)
        {
            delta = delta.ToIntegerDomain();

            base.RealocateElements(delta);
            tabButtonList.RealocateElements(delta);

            equippedButtonList.ForEach((x) => x.ButtonOffset += delta);

            UpdateAttatchmentPosition();
        }

        protected override void UpdateAttatchmentPosition()
        {
            base.UpdateAttatchmentPosition();

            tabButtonList.UpdateAttatchedPosition();
            equippedButtonList.ForEach((x) => x.UpdateAttatchedPosition());
        }

        public void RealocateEquippedItems()
        {
            List<Button> bList = equippedButtonList.Except(unusedEquippedButtonList).ToList();

            if (bList.Count == 0) return;

            bList[0].ButtonOffset = (background.PositionOffset + new Vector2(-182f, 157f)).ToIntegerDomain();

            for (int i = 1; i < bList.Count; i++)
                bList[i].ButtonOffset = (bList[i - 1].ButtonOffset + new Vector2(bList[i - 1].ButtonSprite.SourceRectangle.Width - 1, 0)).ToIntegerDomain();
        }

        public override void Update(GameTime gameTime)
        {
            if (!ShouldRender) return;

            base.Update(gameTime);
            equippedButtonList.ForEach((x) => x.Update());
            unusedEquippedButtonList.ForEach((x) => equippedButtonList.Remove(x));

            //ToBeRemoved
            //itemButtonList.ForEach((x) => x.UpdateAttatchedPosition());

            tabButtonList.Update();
            UpdateAttatchmentPosition();

            Console.WriteLine(background.PositionOffset);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!ShouldRender) return;

            base.Draw(gameTime, spriteBatch);
            equippedButtonList.ForEach((x) => x.Draw(gameTime, spriteBatch));
            tabButtonList.Draw(gameTime, spriteBatch);
        }
    }
}
