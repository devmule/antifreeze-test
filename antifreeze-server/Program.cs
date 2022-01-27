using System;

namespace AntifreezeServer
{
    class Program
    {

        private static Networking.INetwork server;
        private static AntiGame.Game game;

        static void Main(string[] args)
        {


            Random rnd = new Random();

            int tps = 30;                       // ticks per second
            int gridSize = rnd.Next(7, 13);     // NxN grid, 7 <= N <= 12
            int unitsCount = rnd.Next(1, 6);    // 1 <= units <= 5


            server = new Networking.TcpServer();
            server.OnClientConnected += Server_OnClientConnected;
            server.OnMessageReceived += Server_OnMessageReceived;


            game = new AntiGame.Game(gridSize, unitsCount);
            game.OnTick += Game_OnTick;


            server.Start(8080);
            game.Start(tps);

        }

        private static void Server_OnMessageReceived(object sender, MessageEventArgs e)
        {
            game.ApplyUserUnput(e.Message);
        }

        private static void Server_OnClientConnected(object sender, Networking.ClientConnectedEventArgs e)
        {
            server.Send(e.Client, game.GetGameState());
        }

        private static void Game_OnTick(object sender, MessageEventArgs e)
        {
            server.Broadcast(e.Message);
        }
    }
}
