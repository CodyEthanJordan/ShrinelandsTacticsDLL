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
        static StringBuilder output = new StringBuilder();

        static void Main(string[] args)
        {
            Console.WriteLine("Shrinelands");
            Console.WriteLine("Press Any Key to Start");

            var data = GameData.ReadDatafilesInDirectory("GameData");
            DM = DungeonMaster.GetDebugDM(data);

            GameLoop();
        }

        static void GameLoop()
        {
            string line = "help";
            while(gameRunning)
            {
                Console.Clear();
                output.Clear();
                var result = Parser.Default.ParseArguments<MoveOptions, UseOptions, QuitOptions>(line.Split(' '))
                    .WithParsed<QuitOptions>(opts => Environment.Exit(0))
                    .WithParsed<MoveOptions>(Move);
                Console.WriteLine(DM.VisualizeWorld());
                Console.WriteLine(output.ToString());
                line = Console.ReadLine();

            }
        }

        static void Move(MoveOptions opt)
        {
            var outcome = DM.ImplicitActivation(opt.UnitName);
            output.Append(outcome.Message.ToString());
            outcome = DM.MoveCharacter(opt.UnitName, opt.Directions);
            output.Append(outcome.Message.ToString());
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
