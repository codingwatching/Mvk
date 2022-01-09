﻿using MvkServer.Entity.Player;
using MvkServer.Management;
using MvkServer.World.Chunk;

namespace MvkServer.World
{
    /// <summary>
    /// Серверный объект мира
    /// </summary>
    public class WorldServer : WorldBase
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }
        /// <summary>
        /// Объект клиентов
        /// </summary>
        public PlayerManager Players { get; protected set; }
        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        public ChunkProviderServer ChunkPrServ => ChunkPr as ChunkProviderServer;

        public WorldServer(Server server) : base()
        {
            ServerMain = server;
            ChunkPr = new ChunkProviderServer(this);
            Players = new PlayerManager(this);
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            ChunkPrServ.UnloadQueuedChunks();
        }

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            try
            {
                EntityPlayerServer playerServer = Players.GetEntityPlayerMain();
                return string.Format("Ch {0}-{2} Pl {1}\r\n{3}",
                    ChunkPr.Count, Players.PlayerCount, Players.chunkCoordPlayers.Count, playerServer != null ? playerServer.ToString() : "");
            }
            catch
            {
                return "error";
            }
        }
    }
}