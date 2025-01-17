﻿using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок огня
    /// </summary>
    public class BlockFire : BlockBase
    {
        /***
         * Met
         * 0 - 3 bit - Age (0 - 15)
         * 4 bit - East
         * 5 bit - West
         * 6 bit - North
         * 7 bit - South
         * 8 bit - Up
         * 
         * age = met & 0xF
         * pole = met >> 4
         *  South = pole & 1 != 0
         *  East = pole & 2 != 0
         *  North = pole & 4 != 0
         *  West = pole & 8 != 0
         *  Up = pole & 16 != 0
         * met = pole << 4 | age & 0xF
         */

        private AxisAlignedBB[][] collisions;

        /// <summary>
        /// Блок стоячей лавы
        /// </summary>
        public BlockFire()
        {
            //NeedsRandomTick = true;
            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            //Translucent = true; // Наложение на древесину, декали
            IsAddMet = true;
            SetUnique(true);
            IsCollidable = false;
            IsReplaceable = true;
            LightValue = 15;
            IsParticle = false;
            canDropPresent = false;
            Material = Materials.GetMaterialCache(EnumMaterial.Fire);
            samplesStep = new AssetsSample[0];
            samplesBreak = new AssetsSample[] { AssetsSample.FireFizz };
            InitBoxs();
        }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        /// <summary>
        /// Тон сэмпла сломанного блока,
        /// </summary>
        public override float SampleBreakPitch(Rand random) => 2.6f + (random.NextFloat() - random.NextFloat()) * .8f;

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            BlockPos blockPosDown = blockPos.OffsetDown();
            return worldIn.DoesBlockHaveSolidTopSurface(blockPosDown) 
                || BlockThatCanBurn(worldIn, blockPosDown) 
                || SideBlockCombustibility(worldIn, blockPos) != 0;
        }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            return state.NewMet(MetUpdate(worldIn, blockPos, 0));
        }

        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            // запустить мгновенный тик
            worldIn.SetBlockTick(blockPos, (uint)(worldIn.Rnd.Next(10) + 5));
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (CanBlockStay(worldIn, blockPos))
            {
                ushort newMet = MetUpdate(worldIn, blockPos, neighborState.met);
                if (newMet != neighborState.met) worldIn.SetBlockStateMet(blockPos, newMet);
            }
            else
            {
                worldIn.SetBlockToAir(blockPos);
            }
        }

        private ushort MetUpdate(WorldBase worldIn, BlockPos blockPos, ushort met)
        {
            BlockPos blockPosDown = blockPos.OffsetDown();
            if (worldIn.DoesBlockHaveSolidTopSurface(blockPosDown)
                || BlockThatCanBurn(worldIn, blockPosDown)) return (ushort)(met & 0xF);
            return (ushort)(SideBlockCombustibility(worldIn, blockPos) << 4 | met & 0xF);
        }

        private ushort SideBlockCombustibility(WorldBase worldIn, BlockPos blockPos)
        {
            ushort pole = 0;
            if (BlockThatCanBurn(worldIn, blockPos.OffsetSouth())) pole |= 1;
            if (BlockThatCanBurn(worldIn, blockPos.OffsetEast())) pole |= 2;
            if (BlockThatCanBurn(worldIn, blockPos.OffsetNorth())) pole |= 4;
            if (BlockThatCanBurn(worldIn, blockPos.OffsetWest())) pole |= 8;
            if (BlockThatCanBurn(worldIn, blockPos.OffsetUp())) pole |= 16;
            return pole;
        }

        /// <summary>
        /// Блок который может гореть
        /// </summary>
        private bool BlockThatCanBurn(WorldBase worldIn, BlockPos blockPos)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return block.Combustibility;
        }

        /// <summary>
        /// Увеличить шансы загорания 0-100 %
        /// </summary>
        private byte BlockIgniteOddsSunbathing(WorldBase worldIn, BlockPos blockPos)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return block.IgniteOddsSunbathing;
        }

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met >> 4];

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            int pole = met >> 4;
            int count = collisions[pole].Length;
            AxisAlignedBB[] axis0 = collisions[pole];
            AxisAlignedBB[] axis = new AxisAlignedBB[count];
            vec3 min, max;

            for (int i = 0; i < count; i++)
            {
                min = axis0[i].Min;
                max = axis0[i].Max;
                axis[i] = new AxisAlignedBB(
                    min.x + pos.X, min.y + pos.Y, min.z + pos.Z,
                    max.x + pos.X, max.y + pos.Y, max.z + pos.Z
                );
            }
            return axis;
        }

        #region Boxs

        /// <summary>
        /// Сторона West
        /// </summary>
        private QuadSide QuadWest() 
            => new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.West, true, 15, 0, 0, 15, 20, 16);

        /// <summary>
        /// Сторона East
        /// </summary>
        private QuadSide QuadEast()
            => new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.East, true, 1, 0, 0, 1, 20, 16);

        /// <summary>
        /// Сторона South
        /// </summary>
        private QuadSide QuadSouth()
            => new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.South, true, 0, 0, 1, 16, 20, 1);

        /// <summary>
        /// Сторона North
        /// </summary>
        private QuadSide QuadNorth()
            => new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.North, true, 0, 0, 15, 16, 20, 15);

        /// <summary>
        /// Сторона Up y=0 p=.1 || y=pi p=.1
        /// </summary>
        private QuadSide QuadUp(float yaw, float pitch)
            => new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.Down, true, 0, 15, -1, 16, 15, 18).SetRotate(yaw, pitch);
        
        #endregion

        #region Collisions

        /// <summary>
        /// Сторона West
        /// </summary>
        private AxisAlignedBB[] CollisionsWest() 
            => new AxisAlignedBB[] { new AxisAlignedBB(1, 0, 0, 1, 1, 1) };
        /// <summary>
        /// Сторона East
        /// </summary>
        private AxisAlignedBB[] CollisionsEast()
            => new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 0, 1, 1) };
        /// <summary>
        /// Сторона South
        /// </summary>
        private AxisAlignedBB[] CollisionsSouth()
            => new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 1, 1, 0) };
        /// <summary>
        /// Сторона North
        /// </summary>
        private AxisAlignedBB[] CollisionsNorth()
            => new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 1, 1, 1, 1) };
        /// <summary>
        /// Сторона Up
        /// </summary>
        private AxisAlignedBB[] CollisionsUp()
            => new AxisAlignedBB[] { new AxisAlignedBB(0, 1, 0, 1, 1, 1) };

        #endregion

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            InitQuads(Particle);
            collisions = new AxisAlignedBB[32][];
            quads = new QuadSide[32][];

            // На весь блок
            collisions[0] = new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 1, 0, 1) };
            quads[0] = new QuadSide[] {
                new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.East, true),
                new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.West, true),
                new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.North, true),
                new QuadSide(0, 32, 1).SetTexture(54).SetSide(Pole.South, true),

                new QuadSide(0, 32, 1).SetTexture(2102).SetSide(Pole.North, true, 0, -1, 8, 16, 20, 8).SetRotate(0, .5f),
                new QuadSide(0, 32, 1).SetTexture(2102).SetSide(Pole.South, true, 0, -1, 8, 16, 20, 8).SetRotate(0, -.5f),
                new QuadSide(0, 32, 1).SetTexture(2102).SetSide(Pole.North, true, 0, -1, 8, 16, 20, 8).SetRotate(glm.pi90, .5f),
                new QuadSide(0, 32, 1).SetTexture(2102).SetSide(Pole.South, true, 0, -1, 8, 16, 20, 8).SetRotate(glm.pi90, -.5f)
            };
            
            ListMvk<QuadSide> listB = new ListMvk<QuadSide>(6);
            ListMvk<AxisAlignedBB> listC = new ListMvk<AxisAlignedBB>(5);
            for (int i = 1; i < 32; i++)
            {
                if ((i & 1) != 0)
                {
                    listB.Add(QuadNorth());
                    listC.AddRange(CollisionsNorth());
                }
                if ((i & 2) != 0)
                {
                    listB.Add(QuadWest());
                    listC.AddRange(CollisionsWest());
                }
                if ((i & 4) != 0)
                {
                    listB.Add(QuadSouth());
                    listC.AddRange(CollisionsSouth());

                }
                if ((i & 8) != 0)
                {
                    listB.Add(QuadEast());
                    listC.AddRange(CollisionsEast());
                }
                if ((i & 16) != 0)
                {
                    listB.Add(QuadUp(0, .1f));
                    listB.Add(QuadUp(glm.pi, .1f));
                    listC.AddRange(CollisionsUp());
                }
                quads[i] = listB.ToArray();
                listB.Clear();
                collisions[i] = listC.ToArray();
                listC.Clear();
            }
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (random.Next(24) == 0)
            {
                world.PlaySound(AssetsSample.Fire, blockPos.ToVec3() + .5f, 1f + random.NextFloat(), random.NextFloat() * .7f + .3f);
            }

            int pole = blockState.met >> 4;
            if (pole > 0)
            {
                if ((pole & 1) != 0) // South
                {
                    world.SpawnParticle(EnumParticle.Smoke, 2,
                        new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + 1f), new vec3(1f, 1f, .125f), 0, 40);
                }
                if ((pole & 2) != 0) // East
                {
                    world.SpawnParticle(EnumParticle.Smoke, 2,
                        new vec3(blockPos.X + 1f, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(.125f, 1f, 1f), 0, 40);
                }
                if ((pole & 4) != 0) // North
                {
                    world.SpawnParticle(EnumParticle.Smoke, 2,
                        new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z), new vec3(1f, 1f, .125f), 0, 40);
                }
                if ((pole & 8) != 0) // West
                {
                    world.SpawnParticle(EnumParticle.Smoke, 2,
                        new vec3(blockPos.X, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(.125f, 1f, 1f), 0, 40);
                }
                if ((pole & 16) != 0) // Up
                {
                    world.SpawnParticle(EnumParticle.Smoke, 2,
                        new vec3(blockPos.X + .5f, blockPos.Y + .75f, blockPos.Z + .5f), new vec3(1f, .5f, 1f), 0, 40);
                }
            }
            else
            {
                // Цельный
                world.SpawnParticle(EnumParticle.Smoke, 3,
                    new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(1f), 0, 40);
            }
        }

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            int age = blockState.met & 0xF;
            if (age == 15)
            {
                world.SetBlockToAir(blockPos);
            }
            else
            {
                age += random.Next(3);
                
                int pole = blockState.met >> 4;
                int bx = blockPos.X;
                int by = blockPos.Y;
                int bz = blockPos.Z;
                int x, y, z;
                BlockPos pos;
               // bool fire;
                int ignite, igniteAll;

                // Сгорел
                Burned(world, blockPos.OffsetUp(), random);
                Burned(world, blockPos.OffsetDown(), random);
                Burned(world, blockPos.OffsetSouth(), random);
                Burned(world, blockPos.OffsetEast(), random);
                Burned(world, blockPos.OffsetNorth(), random);
                Burned(world, blockPos.OffsetWest(), random);

                // Поджечь соседние
                for (x = -1; x <= 1; x++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (y = -1; y <= 4; y++)
                        {
                            if (x != 0 || y != 0 || z != 0)
                            {
                                pos = blockPos.Offset(x, y, z);
                                BlockState blockState2 = world.GetBlockState(pos);
                                BlockBase block = blockState2.GetBlock();
                                if (block.IsAir)
                                {
                                    igniteAll = 0;
                                    ignite = BlockIgniteOddsSunbathing(world, pos.OffsetUp());
                                    if (ignite > igniteAll) igniteAll = ignite;
                                    ignite = BlockIgniteOddsSunbathing(world, pos.OffsetDown());
                                    if (ignite > igniteAll) igniteAll = ignite;
                                    ignite = BlockIgniteOddsSunbathing(world, pos.OffsetSouth());
                                    if (ignite > igniteAll) igniteAll = ignite;
                                    ignite = BlockIgniteOddsSunbathing(world, pos.OffsetEast());
                                    if (ignite > igniteAll) igniteAll = ignite;
                                    ignite = BlockIgniteOddsSunbathing(world, pos.OffsetNorth());
                                    if (ignite > igniteAll) igniteAll = ignite;
                                    ignite = BlockIgniteOddsSunbathing(world, pos.OffsetWest());
                                    if (ignite > igniteAll) igniteAll = ignite;

                                    if (igniteAll == 100 || random.Next(1000) < igniteAll)
                                    {
                                        age /= 2;
                                        if (world.SetBlockState(pos, blockState.NewMet(MetUpdate(world, pos, 0)), 14))
                                        {
                                            // запустить мгновенный тик
                                            world.SetBlockTick(pos, (uint)(random.Next(10) + 30 - (igniteAll / 4)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (age > 15) age = 15; else if (age < 0) age = 0;
                world.SetBlockStateMet(blockPos, (ushort)(pole << 4 | age & 0xF), false);
                // Для продолжения жизни, и запуска соседних, можно дольше
                world.SetBlockTick(blockPos, (uint)(random.Next(10) + 30));
            }
        }

        /// <summary>
        /// Сгорел
        /// </summary>
        private void Burned(WorldBase world, BlockPos blockPos, Rand random)
        {
            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            if (block.EBlock == EnumBlock.Clay)
            {
                // Если глина, то превращается в керамику
                world.SetBlockState(blockPos, new BlockState(EnumBlock.Terracotta), 14);
            }
            else if (block.EBlock == EnumBlock.Turf)
            {
                BlockPos blockPosUp = blockPos.OffsetUp();
                BlockState blockStateUp = world.GetBlockState(blockPosUp);
                BlockBase blockUp = blockStateUp.GetBlock();
                if (blockUp.Material.EMaterial == EnumMaterial.Sapling)
                {
                    world.SetBlockToAir(blockPosUp);
                    if (CanBlockStay(world, blockPosUp))
                    {
                        if (world.SetBlockState(blockPosUp, new BlockState(EnumBlock.Fire).NewMet(MetUpdate(world, blockPosUp, 0)), 15))
                        {
                            // запустить мгновенный тик
                            world.SetBlockTick(blockPosUp, (uint)(world.Rnd.Next(10) + 30 - (blockUp.IgniteOddsSunbathing / 4)));
                        }
                    }
                }
                world.SetBlockState(blockPos, new BlockState(EnumBlock.Dirt), 14);
            }
            else
            {
                int burn = block.BurnOdds;
                if (burn == 100 || random.Next(1000) < burn)
                {
                    world.SetBlockToAir(blockPos);
                    if (CanBlockStay(world, blockPos))
                    {
                        if (world.SetBlockState(blockPos, new BlockState(EnumBlock.Fire).NewMet(MetUpdate(world, blockPos, 0)), 15))
                        {
                            // запустить мгновенный тик
                            world.SetBlockTick(blockPos, (uint)(world.Rnd.Next(10) + 30 - (block.IgniteOddsSunbathing / 4)));
                        }
                    }
                }
            }
        }
    }
}
