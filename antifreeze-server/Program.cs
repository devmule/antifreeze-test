using System;

namespace AntifreezeServer
{
    class Program
    {

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


            Networking.INetwork server = new Networking.SocketServer();
            AntiGame.AntiGame game = new AntiGame.AntiGame(gridSize, unitsCount);

            // when new client connected
            server.OnClientConnected += (sender, e) => {

                // subscribe game to messages from client
                e.ClientConnection.OnMessageReceived += (sender, e) => game.ApplyClientMessage(e.Message);

                // send game state to client
                var gameStateString = game.GetSerializedGameState();
                e.ClientConnection.Send(gameStateString);

            };


            // when game updated -> send changes to all clients
            game.OnGameUpdated += message => server.Broadcast(message);


            // start server and game
            server.Start("localhost", 8080);
            game.Start(tps);

        }
    }
}
