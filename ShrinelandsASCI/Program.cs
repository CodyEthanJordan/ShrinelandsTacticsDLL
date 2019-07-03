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
                var result = Parser.Default.ParseArguments<
                    MoveOptions, UseOptions, QuitOptions, StatusOptions, EndTurnOptions
                    >(line.Split(' '))
                    .WithParsed<MoveOptions>(Move)
                    .WithParsed<UseOptions>(Use)
                    .WithParsed<QuitOptions>(opts => Environment.Exit(0))
                    .WithParsed<StatusOptions>(ShowStatus)
                    .WithParsed<EndTurnOptions>(opts => DM.EndTurn());
                Console.WriteLine(DM.VisualizeWorld());
                if(DM.activatedCharacter != null)
                {
                    Console.WriteLine(DM.activatedCharacter.GetInfo(1));
                }
                Console.WriteLine(output.ToString());
                line = Console.ReadLine();

            }
        }

        static void Use(UseOptions opt)
        {
            throw new NotImplementedException();
        }

        static void Move(MoveOptions opt)
        {
            var outcome = DM.ImplicitActivation(opt.UnitName);
            output.Append(outcome.Message.ToString());
            outcome = DM.MoveCharacter(opt.UnitName, opt.Directions);
            output.Append(outcome.Message.ToString());
        }

        static void ShowStatus(StatusOptions opt)
        {
            foreach (var side in DM.Sides)
            {
                output.AppendLine(side.Name);
                foreach (var guy in DM.Characters.FindAll(c => c.SideID == side.ID))
                {
                    output.AppendLine("   " + guy.GetInfo(0));
                }
            }
        }
    }

    //TODO: think verb to see deck for ability?
    //TODO: "status" verb to show sides, units, and general status

    //TODO: why rquire unit name if it has to be activated anyway
    [Verb("move", HelpText = "move unitName s ne n")]
    class MoveOptions
    {
        [Value(0)]
        public string UnitName { get; set; }
        [Value(1)]
        public IEnumerable<string> Directions { get; set; }
    }

    [Verb("use", HelpText = "use AbilityName ")]
    class UseOptions
    {
        [Value(0)]
        public string Ability { get; set; } //can be ability name (possibly with quotes) or number on activated character

    }

    [Verb("status", HelpText="show game status summary")]
    class StatusOptions
    {
        //TODO: option for detailed status on specific unit
    }

    [Verb("endturn", HelpText="ends your turn")]
    class EndTurnOptions
    {

    }

    [Verb("quit", HelpText = "exit the program")]
    class QuitOptions
    {

    }
}
