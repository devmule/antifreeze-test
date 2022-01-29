using System;
using System.Collections.Generic;

namespace AntifreezeServer
{
    class Program
    {

        private static Networking.SocketServer server;
        private static AntiGame.Game game;

        static void Main(string[] args)
        {

            Random rnd = new Random();

            int tps = 30;                       // ticks per second
            int gridSize = rnd.Next(7, 13);     // NxN grid, 7 <= N <= 12
            int unitsCount = rnd.Next(1, 6);    // 1 <= units <= 5

            Console.WriteLine("AntiGame generated values:");
            Console.WriteLine("Grid size : {0}x{0}", gridSize);
            Console.WriteLine("UnitsCount: {0}", unitsCount);
            Console.WriteLine();


            server = new Networking.SocketServer();
            server.OnClientConnected += Server_OnClientConnected;


            game = new AntiGame.Game(gridSize, unitsCount);
            game.OnTick += Game_OnTick;


            server.Start("localhost", 8080);
            game.Start(tps);

        }

        private static void Server_OnClientConnected(Networking.SocketClientConnection clientConnection)
        {
            clientConnection.OnMessageReceived += ClientConnection_OnMessageReceived;
            var gameStateString = game.GetSerializedGameState();
            clientConnection.Send(gameStateString);
        }

        private static void ClientConnection_OnMessageReceived(string msg)
        {
            game.ApplyClientMessage(msg);
        }

        private static void Game_OnTick(string message)
        {
            server.Broadcast(message);
        }
    }
}
