using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShrinelandsTactics.BasicStructures;

namespace ShrinelandsTactics
{
    public class DungeonMaster
    {
        public Map map;
        public List<Side> Sides = new List<Side>();
        public List<Character> Characters = new List<Character>();

        private GameData data;
        public Side currentSide { get; private set; }
        private Character activatedCharacter = null;

        public DungeonMaster(GameData data)
        {
            this.data = data;
        }

        public void ResolveAction()
        {
             
            // validate action
        }


        public string VisualizeWorld()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Turn 0");

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var pos = new Position(x, y);
                    var character = Characters.FirstOrDefault(c => c.Pos == pos);
                    if(character != null)
                    {
                        sb.Append(character.Name[0]);
                    }
                    else
                    {
                        var tile = map.GetTile(x, y);
                        sb.Append(tile.Icon);
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static DungeonMaster GetDebugDM(GameData data)
        {
            var DM = new DungeonMaster(data);

            DM.map = DebugData.GetFlatlandMap(data);

            DM.Sides.Add(new Side("Heros"));
            DM.Sides.Add(new Side("The Foe"));
            DM.currentSide = DM.Sides[0];

            var robby = data.GetCharacterByName("Debug Guy");
            robby.InitializeIndividual("Robby", new Position(1, 1), DM.Sides[0].ID);
            DM.Characters.Add(robby);

            var zach = data.GetCharacterByName("Debug Guy");
            zach.InitializeIndividual("Zach", new Position(1, 3), DM.Sides[1].ID);
            DM.Characters.Add(zach);

            return DM;
        }

    }
}
