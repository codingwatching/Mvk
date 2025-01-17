﻿using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет блок
    /// </summary>
    public class ItemBlock : ItemBase
    {
        public BlockBase Block { get; private set; }

        public ItemBlock(BlockBase block, EnumItem enumItem, int numberTexture, int maxStackSize) : base(numberTexture, maxStackSize)
        {
            Block = block;
            SetEnumItem(enumItem);
        }

        public override string GetName() => "block." + Block.EBlock;

        /// <summary>
        /// Объект крафта
        /// </summary>
        public override CraftItem GetCraft() => Block.Craft;

        /// <summary>
        ///  Вызывается, когда блок щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool OnItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            if (!worldIn.GetBlockState(blockPos).GetBlock().IsReplaceable)
            {
                blockPos = blockPos.Offset(side);
            }
            if (CanPlaceBlockOnSide(stack, playerIn, worldIn, blockPos, Block, side, facing))
            {
                BlockState blockState = Block.OnBlockPlaced(worldIn, blockPos, new BlockState(Block.EBlock), side, facing);
                if (Block.CanBlockStay(worldIn, blockPos, blockState.met))
                {
                    BlockState blockStateOld = worldIn.GetBlockState(blockPos);
                    bool result = worldIn.SetBlockState(blockPos, blockState, 15);
                    if (result)
                    {
                        InstallAdditionalBlocks(worldIn, blockPos);

                        if (!playerIn.IsCreativeMode)
                        {
                            blockStateOld.GetBlock().DropBlockAsItem(worldIn, blockPos, blockStateOld);
                            if (stack.Item != null)
                            {
                                playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
                            }
                        }
                        worldIn.PlaySound(playerIn, Block.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                    }
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Установить дополнительные блоки
        /// </summary>
        protected virtual void InstallAdditionalBlocks(WorldBase worldIn, BlockPos blockPos) { }
    }
}
