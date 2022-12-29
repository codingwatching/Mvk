﻿namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Базовый объект задачи искусственного интеллекта моба
    /// </summary>
    public abstract class EntityAIBase
    {
        /// <summary>
        /// Сущность которая наблюдает, т.е. эта
        /// </summary>
        protected EntityLiving entity;

        /// <summary>
        /// Устанавливает битовую маску, указывающую, какие другие задачи не могут выполняться одновременно.
        /// Тест представляет собой простое побитовое И — если он дает ноль, две задачи могут
        /// выполняться одновременно, если нет — они должны запускаться исключительно друг от друга.
        /// </summary>
        private int mutexBits;

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public virtual void ResetTask() { }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public virtual void UpdateTask() { }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public abstract bool ShouldExecute();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public virtual void StartExecuting() { }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public virtual bool ContinueExecuting() => ShouldExecute();

        /// <summary>
        /// Определите, может ли эта задача ИИ быть прервана задачей с более высоким приоритетом. 
        /// У всех ванильных AITask это значение равно true.
        /// </summary>
        public virtual bool IsInterruptible() => true;

        /// <summary>
        /// Устанавливает битовую маску, указывающую, какие другие задачи не могут выполняться одновременно.
        /// Тест представляет собой простое побитовое И — если он дает ноль, две задачи могут
        /// выполняться одновременно, если нет — они должны запускаться исключительно друг от друга.
        /// </summary>
        public void SetMutexBits(int mutexBits) => this.mutexBits = mutexBits;

        /// <summary>
        /// Получите битовую маску, указывающую, какие другие задачи не могут выполняться одновременно. 
        /// Тест представляет собой простое побитовое И — если он дает ноль, две задачи могут 
        /// выполняться одновременно, если нет — они должны запускаться исключительно друг от друга.
        /// </summary>
        public int GetMutexBits() => mutexBits;
    }
}