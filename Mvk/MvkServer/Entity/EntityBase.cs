﻿using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity
{
    /// <summary>
    /// Базовый объект сущности
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// Объект мира
        /// </summary>
        public WorldBase World { get; protected set; }
        /// <summary>
        /// В каком чанке находится
        /// </summary>
        public vec2i ChunkPos { get; protected set; } = new vec2i();
        /// <summary>
        /// Позиция псевдо чанка
        /// </summary>
        public int ChunkY { get; protected set; }
        /// <summary>
        /// В каком блоке находится
        /// </summary>
        public vec3i BlockPos { get; protected set; } = new vec3i();
        /// <summary>
        /// На каком стоим блоке
        /// </summary>
        public vec3i BlockPosDown { get; protected set; } = new vec3i();
        /// <summary>
        /// В каком чанке было обработка чанков
        /// </summary>
        public vec2i ChunkPosManaged { get; protected set; } = new vec2i();
        /// <summary>
        /// Позиция объекта
        /// </summary>
        public vec3 Position { get; protected set; }
        /// <summary>
        /// Поворот вокруг своей оси
        /// </summary>
        public float RotationYaw { get; protected set; }
        /// <summary>
        /// Поворот вверх вниз
        /// </summary>
        public float RotationPitch { get; protected set; }
        /// <summary>
        /// Хитбок сущьности
        /// </summary>
        public HitBox Hitbox { get; protected set; }

        /// <summary>
        /// Перемещение объекта
        /// </summary>
        public vec3 Motion { get; protected set; }
        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround { get; protected set; } = true;
        /// <summary>
        /// Бежим
        /// </summary>
        public bool IsSprinting { get; protected set; } = false;
        /// <summary>
        /// Прыгаем
        /// </summary>
        public bool IsJumping { get; protected set; } = false;
        /// <summary>
        /// Летает ли сущность
        /// </summary>
        public bool IsFlying { get; protected set; } = false;

        #region PervLast

        /// <summary>
        /// Последнее значение поворота вокруг своей оси
        /// </summary>
        public float RotationYawLast { get; protected set; }
        /// <summary>
        /// Последнее значение поворота вверх вниз
        /// </summary>
        public float RotationPitchLast { get; protected set; }
        /// <summary>
        /// Координата объекта на предыдущем тике, используемая для расчета позиции во время процедур рендеринга
        /// </summary>
        public vec3 PositionLast { get; protected set; }

        #endregion

        /// <summary>
        /// Результат сидеть
        /// </summary>
        protected EnumSneaking sneaking = EnumSneaking.DonSit;

        /// <summary>
        /// Задать вращение
        /// </summary>
        public void SetRotation(float yaw, float pitch)
        {
            RotationYaw = yaw;
            RotationPitch = pitch;
        }

        /// <summary>
        /// Задать позицию
        /// </summary>
        public bool SetPosition(vec3 pos)
        {
            if (!Position.Equals(pos))
            {
                Position = pos;
                BlockPos = new vec3i(Position);
                BlockPosDown = new vec3i(new vec3(pos.x, pos.y - 1, pos.z));
                ChunkPos = new vec2i((BlockPos.x) >> 4, (BlockPos.z) >> 4);
                ChunkY = (BlockPos.y) >> 4;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Задать чанк обработки
        /// </summary>
        public void SetChunkPosManaged(vec2i pos) => ChunkPosManaged = pos;

        /// <summary>
        /// Проверка смещения чанка на выбранное положение
        /// </summary>
        public bool CheckPosManaged(int bias)
            => Mth.Abs(ChunkPos.x - ChunkPosManaged.x) >= bias || Mth.Abs(ChunkPos.y - ChunkPosManaged.y) >= bias;

        ///// <summary>
        ///// Присел ли
        ///// </summary>
        //public bool IsSneaking() => sneaking != EnumSneaking.DonSit;
        ///// <summary>
        ///// Встаём ли
        ///// </summary>
        //public bool IsSneakingNearly() => sneaking == EnumSneaking.GetUp;

        /// <summary>
        /// Заменить хзитбокс
        /// </summary>
        protected void SetHeightEyes(float height, float eyes) => Hitbox = Hitbox.SetHeightEyes(height, eyes);

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public virtual void Update()
        {
            PositionLast = Position;
            RotationYawLast = RotationYaw;
            RotationPitchLast = RotationPitch;
        }
    }
}