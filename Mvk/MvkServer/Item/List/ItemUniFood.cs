﻿using MvkServer.Entity.List;
using MvkServer.Network.Packets.Server;
using MvkServer.World;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет еда, или то что можно есть
    /// </summary>
    public class ItemUniFood : ItemBase
    {
        /// <summary>
        /// Количество сколько востановим сытости (сейчас хп)
        /// </summary>
        private readonly int amount;
        /// <summary>
        /// Продолжительность сколько тиков надо на еду
        /// </summary>
        private readonly int duration;

        /// <summary>
        /// Если это поле истинно, еду можно есть, даже если игроку не нужно есть
        /// </summary>
        protected bool alwaysEdible = true;

        public ItemUniFood(EnumItem enumItem, int numberTexture, int amount = 1, int duration = 32, int maxStackSize = 16)
            : base(enumItem, numberTexture, maxStackSize)
        {
            this.amount = amount;
            this.duration = duration;
            ItemUseAction = EnumItemAction.Eat;
        }

        /// <summary>
        /// Сколько времени требуется, чтобы использовать или потреблять предмет
        /// </summary>
        public override int GetMaxItemUseDuration(ItemStack stack) => 32;

        /// <summary>
        /// Вызывается всякий раз, когда этот предмет экипирован и нажата правая кнопка мыши.
        /// </summary>
        public override ItemStack OnItemRightClick(ItemStack itemStackIn, WorldBase worldIn, EntityPlayer playerIn)
        {
            if (playerIn.CanEat(alwaysEdible))
            {
                playerIn.SetItemInUse(itemStackIn, GetMaxItemUseDuration(itemStackIn));
            }

            return itemStackIn;
        }

        /// <summary>
        /// Вызывается, когда игрок заканчивает использовать этот предмет (например, заканчивает есть). 
        /// Не вызывается, когда игрок перестает использовать предмет до завершения действия.
        /// </summary>
        public override ItemStack OnItemUseFinish(ItemStack stack, WorldBase worldIn, EntityPlayer playerIn)
        {
            stack.ReduceAmount(1);
            if (playerIn.HealthAdd(amount))
            {
                if (!worldIn.IsRemote && worldIn is WorldServer worldServer)
                {
                    // Анимация выздаровления урона
                    worldServer.Tracker.SendToAllTrackingEntityCurrent(playerIn, 
                        new PacketS0BAnimation(playerIn.Id, PacketS0BAnimation.EnumAnimation.Recovery));
                }
            }
            return stack;
        }
    }
}
