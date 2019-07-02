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

        public Outcome ImplicitActivation(string unitName)
        {
            //implicit version of activate unit for convenience
            var outcome = new Outcome();
            var guy = Characters.FirstOrDefault(c => c.Name.Equals(unitName, StringComparison.OrdinalIgnoreCase));

            if(guy == null)
            {
                outcome.Message.AppendLine("No such character as " + unitName);
                return outcome;
            }

            if (activatedCharacter != null)
            {
                if(activatedCharacter.ID == guy.ID)
                {
                    return outcome; //already active, do nothing
                }
                outcome.Message.AppendLine("Currently " + activatedCharacter.Name + "'s activation, needs to end first");
                return outcome;
            }

            return Activate(guy);
        }

        public bool IsActiveAndControllable(Character guy)
        {
            if(activatedCharacter == null || currentSide == null)
            {
                return false;
            }
            return guy.SideID == currentSide.ID && guy.ID == activatedCharacter.ID;
        }

        public Outcome Activate(Character guy)
        {
            var outcome = new Outcome();
            if(guy.HasBeenActivated)
            {
                outcome.Message.AppendLine(guy.Name + " has already been activated this turn");
                return outcome;
            }

            if(guy.SideID != currentSide.ID)
            {
                outcome.Message.AppendLine(guy.Name + " does not belong to current side, " + currentSide.Name);
                return outcome;
            }

            guy.HasBeenActivated = true;
            activatedCharacter = guy;
            outcome.Message.AppendLine("Starting activation for " + guy.Name);
            return outcome;
        }

        public Outcome MoveCharacter(string characterName, IEnumerable<string> directions)
        {
            var outcome = new Outcome();
            Character guy = Characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if(guy == null)
            {
                outcome.Message.AppendLine("No such character as " + characterName);
                return outcome;
            }

            foreach (var dir in directions)
            {
                Map.Direction direction = Map.ParseDirection(dir);
                var o = MoveCharacter(guy, direction);
                outcome.Message.AppendLine(o.Message.ToString());
            }

            return outcome;
        }

        public Outcome MoveCharacter(Character guy, Map.Direction dir)
        {
            var outcome = new Outcome();

            if(!IsActiveAndControllable(guy))
            {
                outcome.Message.AppendLine(guy.Name + " cannot act at this time, is not activated");
                return outcome;
            }

            var destination = guy.Pos + Map.DirectionToPosition[dir];
            if(!IsOpen(destination))
            {
                outcome.Message.AppendLine(guy.Name + " tried to move to " + destination +
                    " but there is something there already");
                return outcome; //blocked TODO: error
            }

            //has enough movement
            var moveCost = map.GetTile(guy.Pos).MoveCost;
            if(moveCost > guy.Move.Value)
            {
                outcome.Message.AppendLine(guy.Name + " only has " + guy.Move.Value + " movement" +
                    " remaining, but needs " + moveCost + " to move");
                return outcome; //TODO: error
            }

            guy.Move.Value -= moveCost;
            guy.Pos = destination;
            outcome.Message.AppendLine(guy.Name + " moved to " + destination);
            return outcome;
        }

        public bool IsOpen(Position pos)
        {
            if(Characters.Any(c => c.Pos == pos))
            {
                return false;
            }

            return map.IsPassable(pos);
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
