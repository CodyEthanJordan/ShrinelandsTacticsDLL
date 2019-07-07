using ShrinelandsTactics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using YamlDotNet.RepresentationModel;
using System.IO;
using System.Drawing;

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

            var data = GameData.ReadDatafilesInDirectory("GameData");

            var yaml = new YamlStream();
            using (StreamReader r = new StreamReader("GameData/SlimeCave.yaml"))
            {
                yaml.Load(r);
            }

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var mapFile = (mapping[new YamlScalarNode("map_file")] as YamlScalarNode).Value;

            Bitmap bitmap = new Bitmap("GameData/" + mapFile);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Test Map");
            sb.AppendLine(bitmap.Width + " " + bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Height; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    sb.Append(data.GetIconByColor(color.R, color.G, color.B).ToString());
                }
                sb.AppendLine();
            }


            DM = DungeonMaster.LoadEncounter(mapping, sb.ToString(), data);

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
                    ActivateOptions, MoveOptions, UseOptions, QuitOptions, StatusOptions, EndTurnOptions
                    >(line.Split(' '))
                    .WithParsed<ActivateOptions>(opts => DM.ImplicitActivation(opts.UnitName))
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
            if(DM.activatedCharacter == null)
            {
                output.AppendLine("No active character");
            }
            var outcome = DM.UseAbility(opt.Ability, opt.Target.ToList(), null);
            output.Append(outcome.Message.ToString());
        }

        static void Move(MoveOptions opt)
        {
            if(DM.activatedCharacter == null)
            {
                output.AppendLine("No active character");
                return;
            }
            var outcome = DM.MoveCharacter(DM.activatedCharacter.Name, opt.Directions);
            output.Append(outcome.Message.ToString());
        }

        static void ShowStatus(StatusOptions opt)
        {
            if(opt.Thing == null || opt.Thing.Length == 0)
            {
                int i = 0;
                foreach (var side in DM.Sides)
                {
                    output.AppendLine(side.Name);
                    foreach (var guy in DM.Characters.FindAll(c => c.SideID == side.ID))
                    {
                        output.AppendLine("   " + i + ":" + guy.GetInfo(0));
                        i++;
                    }
                }
            }
            else
            {
                var character = DM.Characters.FirstOrDefault(c => c.Name.Equals(opt.Thing, StringComparison.OrdinalIgnoreCase));
                if(character != null)
                {
                    output.AppendLine(character.GetInfo(1));
                    return;
                }
            }
           
        }
    }

    //TODO: think verb to see deck for ability?
    //TODO: "status" verb to show sides, units, and general status

    [Verb("activate", HelpText = "activate unitName")]
    class ActivateOptions
    {
        [Value(0)]
        public string UnitName { get; set; }
    }

    //TODO: why rquire unit name if it has to be activated anyway
    [Verb("move", HelpText = "move s ne n")]
    class MoveOptions
    {
        [Value(0)]
        public IEnumerable<string> Directions { get; set; }
    }

    [Verb("use", HelpText = "use AbilityName ")]
    class UseOptions
    {
        [Value(0)]
        public string Ability { get; set; } //can be ability name (possibly with quotes) or number on activated character

        [Value(1)]
        public IEnumerable<string> Target { get; set; }

        //[Value(2)]
        //public string Options { get; set; }

    }

    [Verb("status", HelpText="show game status summary")]
    class StatusOptions
    {
        [Value(0)]
        public string Thing { get; set; }
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
