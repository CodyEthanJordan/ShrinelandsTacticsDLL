using Lidgren.Network;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrineMind;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ShrinelandsServer
{
    class Program
    {
        public static DungeonMaster DM;
        public static int Port = 6356;
        public static NetServer Server;
        public static List<NetConnection> Connections = new List<NetConnection>();
        public static Dictionary<Guid, PlayerType> SideControlledBy = new Dictionary<Guid, PlayerType>();
        public static Dictionary<Guid, AIPlayer> AIControllers = new Dictionary<Guid, AIPlayer>();

        static void Main(string[] args)
        {
            HostGame();
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

        public static void SetupAI()
        {
            foreach (var side in SideControlledBy.Where(kvp => kvp.Value == PlayerType.AI)
                                                    .Select(kvp => kvp.Key))
            {
                var AI = new AIPlayer(side);
                AIControllers.Add(side, AI);
            }
        }

        public static void HostGame()
        {
            Console.WriteLine("Hosting a new game");
            DM = LoadEncounter();
            DM.OnTurnPassed += TurnPassed;
            Console.WriteLine("Loaded encounter");

            SideControlledBy.Add(DM.Sides.First(s => s.Name == "Heros").ID, PlayerType.Player);
            SideControlledBy.Add(DM.Sides.First(s => s.Name == "The Foe").ID, PlayerType.AI);
            SetupAI();

            var config = new NetPeerConfiguration("Shrinelands")
            { Port = Program.Port };
            Server = new NetServer(config);
            Server.Start();
            while (true)
            {
                NetIncomingMessage message;
                while ((message = Server.ReadMessage()) != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            var originalMessage = message.ReadString();
                            StringReader data = new StringReader(originalMessage);
                            var toDo = data.ReadLine();

                            if (toDo == "Send DM")
                            {
                                Console.WriteLine("Sending DM to client");
                                string json = JsonConvert.SerializeObject(DM);
                                var response = Server.CreateMessage("DM\n" + json);
                                Server.SendMessage(response, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if(toDo == "Take action")
                            {
                                Console.WriteLine("Client has taken an action");
                                var outcome = JsonConvert.DeserializeObject<Outcome>(data.ReadToEnd());
                                DM.ApplyOutcome(outcome);
                                //TODO: make function
                                foreach (var connection in Connections)
                                {
                                    var response = Server.CreateMessage(originalMessage);
                                    if(connection != message.SenderConnection)
                                    {
                                        Server.SendMessage(response, connection, NetDeliveryMethod.ReliableOrdered);
                                    }
                                }

                            }
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            switch (message.SenderConnection.Status)
                            {
                                case NetConnectionStatus.Connected:
                                    Connections.Add(message.SenderConnection);
                                    break;
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

        private static void TurnPassed(object sender, Guid e)
        {
            if(SideControlledBy[e] == PlayerType.Player)
            {
                return; //don't need to do anything
            }

            var AI = AIControllers[e];

            var outcomes = AI.TakeTurn(DM);

            InformPlayers(outcomes);
        }

        private static void InformPlayers(List<Outcome> outcomes)
        {
            foreach (var outcome in outcomes)
            {
                string json = JsonConvert.SerializeObject(outcome);
                foreach (var connection in Connections)
                {
                    
                    var response = Server.CreateMessage("Take action\n" + json);
                    Server.SendMessage(response, connection, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        private static void DM_OnTurnPassed(object sender, Guid e)
        {
            throw new NotImplementedException();
        }

        public enum PlayerType
        {
            Player,
            AI
        }
    }
}
