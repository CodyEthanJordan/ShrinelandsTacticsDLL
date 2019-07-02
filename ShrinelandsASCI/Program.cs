using ShrinelandsTactics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ShrinelandsASCI
{
    class Program
    {
        static DungeonMaster DM;
        static bool gameRunning = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Shrinelands");
            Console.WriteLine("Press Any Key to Start");
            Console.ReadKey();

            var data = GameData.ReadDatafilesInDirectory("GameData");
            DM = DungeonMaster.GetDebugDM(data);

            GameLoop();
        }

        static void GameLoop()
        {
            string line = "";
            while(gameRunning)
            {
                Console.Clear();
                Console.WriteLine(DM.VisualizeWorld());
                var result = Parser.Default.ParseArguments<MoveOptions, UseOptions, QuitOptions>(line.Split(' '))
                    .WithParsed<QuitOptions>(opts => Environment.Exit(0));

                line = Console.ReadLine();
            }
        }
    }

    [Verb("move", HelpText = "move unit direction")]
    class MoveOptions
    {
        [Value(0)]
        public string UnitName { get; set; }
        [Value(1)]
        public IEnumerable<string> Directions { get; set; }
    }

    [Verb("use", HelpText = "use ability")]
    class UseOptions
    {

    }

    [Verb("quit", HelpText = "exit the program")]
    class QuitOptions
    {

    }
}
