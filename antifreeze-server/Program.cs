using System;


namespace antifreeze_server
{
    class Program
    {

        static void Main(string[] args)
        {

            GameParameters parameters = new GameParameters(args);
            parameters.PrintOutParameters();


            ServerListener server = new ServerListener();
            server.Start(parameters.port);


            Game game = new Game(parameters.grid, parameters.units, parameters.speed);
            game.server = server;
            game.Start(parameters.tps);

        }
    }
}
