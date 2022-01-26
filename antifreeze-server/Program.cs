namespace AntifreezeServer
{
    class Program
    {

        private static ServerListener server;
        private static Game game;

        static void Main(string[] args)
        {

            Parameters parameters = new Parameters(args);
            parameters.PrintOutParameters();


            server = new ServerListener();
            server.OnClientConnected += Server_OnClientConnected;
            server.OnMessageReceived += Server_OnMessageReceived;


            game = new Game(parameters.grid, parameters.units, parameters.speed);
            game.OnTick += Game_OnTick;


            server.Start(parameters.port);
            game.Start(parameters.tps);

        }

        private static void Server_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            game.ApplyUserUnput(e.Message);
        }

        private static void Server_OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            server.Send(e.Client, game.GetGameState());
        }

        private static void Game_OnTick(object sender, OnTickEventArgs e)
        {
            server.Broadcast(e.Message);
        }
    }
}
