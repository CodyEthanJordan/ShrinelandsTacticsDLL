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
using Lidgren.Network;
using Newtonsoft.Json;

namespace ShrinelandsASCI
{
    class Program
    {
        static DungeonMaster DM;
        static bool gameRunning = true;
        static StringBuilder output = new StringBuilder();
        static NetPeer peer;
        static int Port = 3456;

        static void Main(string[] args)
        {
            Console.WriteLine("Shrinelands");
            Console.WriteLine("Server or client");

            string line = Console.ReadLine();

            var result = Parser.Default.ParseArguments<
                HostOptions, JoinOptions
                >(line.Split(' '))
                .WithParsed<HostOptions>(HostGame)
                .WithParsed<JoinOptions>(JoinGame);

            

            GameLoop();
        }

        public static DungeonMaster LoadEncounter()
        {
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
            return DM;
        }

        static void HostGame(HostOptions h)
        {
            Console.WriteLine("Hosting a new game");
            DM = LoadEncounter();
            Console.WriteLine("Loaded encounter");

            var config = new NetPeerConfiguration("Shrinelands")
            { Port = Program.Port };
            peer = new NetServer(config);
            peer.Start();
            while (true)
            {


                NetIncomingMessage message;
                while ((message = peer.ReadMessage()) != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            // handle custom messages
                            var data = message.ReadString();
                            if(data == "Send DM")
                            {
                                Console.WriteLine("Sending DM to client");
                                string json = JsonConvert.SerializeObject(DM);
                                var response = peer.CreateMessage("DM\n" + json);
                                peer.SendMessage(response, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                            }
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            switch (message.SenderConnection.Status)
                            {
                                /* .. */
                            }
                            break;

                        case NetIncomingMessageType.DebugMessage:
                            // handle debug messages
                            // (only received when compiled in DEBUG mode)
                            Console.WriteLine(message.ReadString());
                            break;

                        /* .. */
                        default:
                            Console.WriteLine("unhandled message with type: "
                                + message.MessageType);
                            break;
                    }
                }
            }
        }

        static void JoinGame(JoinOptions j)
        {
            Console.WriteLine("Attempting to connect to a game at " + j.IP);
            var config = new NetPeerConfiguration("Shrinelands");
            var client = new NetClient(config);
            client.Start();
            var x = client.Connect(host: j.IP, port: Program.Port);

            var r = NetSendResult.FailedNotConnected;
            while (r == NetSendResult.FailedNotConnected)
            {
                var message = client.CreateMessage();
                message.Write("Send DM");
                r = client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            }

            while(true)
            {
                NetIncomingMessage message;
                while ((message = client.ReadMessage()) != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.Error:
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            break;
                        case NetIncomingMessageType.UnconnectedData:
                            break;
                        case NetIncomingMessageType.ConnectionApproval:
                            break;
                        case NetIncomingMessageType.Data:
                            StringReader sr = new StringReader(message.ReadString());
                            var toDo = sr.ReadLine();
                            switch (toDo)
                            {
                                case "DM":
                                    DM = JsonConvert.DeserializeObject<DungeonMaster>(sr.ReadToEnd());
                                    Console.WriteLine("Loaded level from server");
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case NetIncomingMessageType.Receipt:
                            break;
                        case NetIncomingMessageType.DiscoveryRequest:
                            break;
                        case NetIncomingMessageType.DiscoveryResponse:
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                            break;
                        case NetIncomingMessageType.DebugMessage:
                            break;
                        case NetIncomingMessageType.WarningMessage:
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            break;
                        case NetIncomingMessageType.NatIntroductionSuccess:
                            break;
                        case NetIncomingMessageType.ConnectionLatencyUpdated:
                            break;
                        default:
                            break;
                    }
                }
            }
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
                    .WithParsed<EndTurnOptions>(opts => DM.EndTurn(DM.currentSideID));
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

    [Verb("host", HelpText = "begin hosting a server")]
    class HostOptions
    {

    }

    [Verb("join", HelpText = "join a server")]
    class JoinOptions
    {
        [Value(0)]
        public string IP { get; set; }
    }
}
