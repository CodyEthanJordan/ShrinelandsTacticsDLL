using ShrinelandsTactics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            while(gameRunning)
            {
                Console.WriteLine(DM.VisualizeWorld());
                var line = Console.ReadLine();

            }
        }
    }
}
