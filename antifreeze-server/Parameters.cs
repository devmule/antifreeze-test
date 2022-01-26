﻿using System;

namespace AntifreezeServer
{

    /// <summary>
    /// parsing args given to Program
    /// implements default parameter values ​​and validation
    /// </summary>
    class Parameters
    {
        public int port { get; set; } = 8888;
        public int grid { get; set; } = 10;
        public int units { get; set; } = 3;
        public int tps { get; set; } = 20;
        public float speed { get; set; } = 0.5f;

        public Parameters(string[] args)
        {

            int len = args.Length / 2;

            for (int i = 0; i < len; i++)
            {

                string key = args[i * 2];
                string val = args[i * 2 + 1];

                switch (key)
                {
                    case "port":
                        port = int.Parse(val);
                        break;
                    case "grid":
                        grid = int.Parse(val);
                        if (grid < 7) grid = 7;
                        if (grid > 12) grid = 12;
                        break;
                    case "units":
                        units = int.Parse(val);
                        if (units < 0) units = 0;
                        if (units > 5) units = 5;
                        break;
                    case "tps":
                        tps = int.Parse(val);
                        if (tps < 1) tps = 1;
                        if (tps > 100) tps = 100;
                        break;
                    case "speed":
                        speed = float.Parse(val);
                        if (speed < 0.1f) speed = 0.1f;
                        if (speed > 10f) speed = 10f;
                        break;
                }

            }

        }

        public void PrintOutParameters()
        {

            Console.WriteLine();
            Console.WriteLine("           ===    Game parameters are:    ===    ");
            Console.WriteLine();
            Console.WriteLine("                       port : {0}", port);
            Console.WriteLine("                       grid : {0}x{0}", grid);
            Console.WriteLine("                      units : {0}", units);
            Console.WriteLine("     tps (ticks per second) : {0}", tps);
            Console.WriteLine("        speed (tick factor) : {0}", units);
            Console.WriteLine();
            Console.WriteLine();

        }

    }
}