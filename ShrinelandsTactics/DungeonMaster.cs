using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics;
using System.Drawing;
using YamlDotNet.RepresentationModel;
using ShrinelandsTactics.BasicStructures.Events;
using Newtonsoft.Json;

namespace ShrinelandsTactics
{
    public class DungeonMaster : ICloneable
    {
        [JsonProperty]
        public Map map;
        [JsonProperty]
        public List<Side> Sides = new List<Side>();
        [JsonProperty]
        public List<Character> Characters = new List<Character>();
        [JsonProperty]
        public int TurnCount = 0;
        [JsonIgnore]
        public GameData data;
        [JsonProperty]
        public Side currentSide { get; private set; }
        [JsonProperty]
        public Character activatedCharacter = null;

        public event CharacterMovedEventHandler OnCharacterMoved;
        public event EventHandler<Character> OnCharacterCreated;

        public DungeonMaster(GameData data)
        {
            this.data = data;
        }

        public string VisualizeWorld()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Current Side: " + currentSide.Name);
            if(activatedCharacter == null)
            {
                sb.AppendLine("No Active Character");
            }
            else
            {
                sb.AppendLine("Active Character: " + activatedCharacter.Name);
            }

            sb.AppendLine("Turn: " + TurnCount);

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
                //might be number
                int i;
                if(int.TryParse(unitName, out i))
                {
                    if(i < Characters.Count)
                    {
                        guy = Characters[i];
                    }
                    else
                    {
                        outcome.Message.AppendLine("No such character as " + unitName);
                        return outcome;
                    }
                }
                else
                {
                    outcome.Message.AppendLine("No such character as " + unitName);
                    return outcome;
                }
            }

            if (activatedCharacter != null)
            {
                if(activatedCharacter.ID == guy.ID)
                {
                    return outcome; //already active, do nothing
                }
                Deactivate(activatedCharacter);
            }

            return Activate(guy);
        }

        public Outcome EndTurn()
        {
            var outcome = new Outcome();
            // TODO: validate this 
            //need to quickly activate and de-activate remaining units

            if(activatedCharacter != null)
            {
                Deactivate(activatedCharacter);
            }

            foreach (var leftoverGuy in Characters.FindAll(c => c.SideID == currentSide.ID && 
                                                            c.HasBeenActivated == false))
            {
                Activate(leftoverGuy);
                Deactivate(leftoverGuy);
            }

            //update turn counter
            if(Sides.IndexOf(currentSide) == Sides.Count - 1)
            {
                TurnCount++; //we're going to the next turn now that all sides have acted
                //TODO: this assumes all characters on every side act before the side passes turn
                map.NewTurn();
            }

            //pass control to next side
            int i = Sides.IndexOf(currentSide);
            i = (i + 1) % Sides.Count;
            currentSide = Sides[i];

            foreach (var guy in Characters.FindAll(c => c.SideID == currentSide.ID))
            {
                guy.NewTurn();
            }

            return outcome;
        }

        public static DungeonMaster LoadEncounter(YamlMappingNode yaml, string level, GameData data)
        {
            var DM = new DungeonMaster(data);
            DM.map = Map.CreateFromText(level, data);

            var sides = (YamlSequenceNode)yaml.Children[new YamlScalarNode("sides")];
            foreach (var side in sides)
            {
                var name = (side as YamlScalarNode).Value;
                DM.Sides.Add(new Side(name));
            }
            DM.currentSide = DM.Sides[0];

            var characters = (YamlSequenceNode)yaml.Children[new YamlScalarNode("characters")];
            foreach (var node in characters)
            {
                var c = node as YamlMappingNode;
                var name = (c["name"] as YamlScalarNode).Value;
                var characterClass = (c["class"] as YamlScalarNode).Value;
                var posString = (c["pos"] as YamlScalarNode).Value;
                var sideName = (c["side"] as YamlScalarNode).Value;
                Position pos = Position.Parse(posString);
                var side = DM.Sides.FirstOrDefault(s => s.Name.Equals(sideName, StringComparison.OrdinalIgnoreCase));

                Character newCharacter = data.LoadCharacterByClass(characterClass);
                newCharacter.InitializeIndividual(name, pos, side.ID);

                DM.Characters.Add(newCharacter);
            }

            return DM;
        }

        public void CreateCharacter(Character cloneCharacter)
        {
            Characters.Add(cloneCharacter);
            if(OnCharacterCreated != null)
            {
                OnCharacterCreated(this, cloneCharacter);
            }
        }

        public List<Position> GetEmptyAdjacentSquares(Position pos)
        {
            var validPositions = new List<Position>();
            foreach (var adj in Map.GetAdjacent(pos))
            {
                if(IsOpen(adj))
                {
                    validPositions.Add(adj);
                }
            }
            return validPositions;
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
            if(activatedCharacter != null)
            {
                Deactivate(activatedCharacter); //TODO: add to outcome
            }

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

            guy.Activate();
            activatedCharacter = guy;
            outcome.Message.AppendLine("Starting activation for " + guy.Name);
            return outcome;
        }

        public void Deactivate(Character guy)
        {
            if(guy != activatedCharacter)
            {
                return; //TODO: error
            }

            //automatically use dodge as a convencience factor
            var dodge = guy.Actions.FirstOrDefault(a => a.Name == "Dodge");
            if(dodge != null)
            {
                while(guy.CanPay(dodge))
                {
                    dodge.ResolveAction(this, guy, null, null, "");
                }
            }

            guy.EndActivation();
            activatedCharacter = null;
        }

        public void AddSituationalModifiers(Deck deck, Mechanics.Action action, Character user, Position posTarget, Character charTarget)
        {
            foreach (var tag in action.Tags)
            {
                switch (tag)
                {
                    case Mechanics.Action.AbilityType.Attack:
                        if(IsFlanking(user, charTarget))
                        {
                            var flank = new Card("Flanking", Card.CardType.Hit);
                            deck.AddCards(flank, 2); //TODO: magic number
                        }
                        var userOn = map.GetTile(user.Pos);
                        userOn.AddSituationalModifiers(deck, action, user, posTarget, charTarget, false);
                        var targetOn = map.GetTile(charTarget.Pos);
                        targetOn.AddSituationalModifiers(deck, action, user, posTarget, charTarget, true);
                        break;
                    default:
                        break;
                }
            }
        }

        private bool IsFlanking(Character user, Character charTarget)
        {
            var dir = charTarget.Pos - user.Pos;
            var oppositeSquare = charTarget.Pos + dir;
            if(Characters.Any(c => c.Pos == oppositeSquare && c.SideID == user.SideID &&
                                !c.Incapacitated))
            {
                return true;
            }

            return false;
        }

        public Outcome UseAbility(string abilityIdentifier, List<string> target, string options=null)
        {
            var outcome = new Outcome();
            if(activatedCharacter == null)
            {
                outcome.Message.AppendLine("No activated character");
                return outcome;
            }

            if(activatedCharacter.Incapacitated)
            {
                outcome.Message.AppendLine(activatedCharacter.Name + " is incapacitated and cannot act");
                return outcome;
            }

            var ability = activatedCharacter.Actions.FirstOrDefault(a => a.Name.Equals(abilityIdentifier, StringComparison.OrdinalIgnoreCase));
            if(ability == null)
            {
                //try parsing as int
                int i;
                if(int.TryParse(abilityIdentifier, out i))
                {
                    if(i >= activatedCharacter.Actions.Count)
                    {
                        outcome.Message.AppendLine("Can't find ability " + i);
                        return outcome;
                    }
                    ability = activatedCharacter.Actions[i];
                }
                else
                {
                    outcome.Message.AppendLine("Can't find ability " + abilityIdentifier);
                    return outcome;
                }
            }

            if(target.Count() == 1)
            {
                //either direction or name
                var targetCharacter = Characters.FirstOrDefault(c => c.Name.Equals(target[0], StringComparison.OrdinalIgnoreCase));
                if(targetCharacter == null)
                {
                    int i;
                    if(int.TryParse(target[0], out i))
                    {
                        if (i < Characters.Count)
                        {
                            targetCharacter = Characters[i];
                            outcome = ability.ResolveAction(this, activatedCharacter, null, targetCharacter, options);
                            return outcome;
                        }
                        else
                        {
                            outcome.Message.AppendLine("No such character as " + target[0]);
                            return outcome;
                        }
                    }
                    else
                    {
                        try
                        {
                            var dir = Map.ParseDirection(target[0]);
                            var pos = activatedCharacter.Pos + Map.DirectionToPosition[dir];
                            targetCharacter = Characters.FirstOrDefault(c => c.Pos == pos);
                            outcome = ability.ResolveAction(this, activatedCharacter, pos, targetCharacter, options);
                            return outcome; //TODO: get from resolve action
                        }
                        catch
                        {
                            outcome.Message.AppendLine("Cannot target " + target[0]);
                            return outcome;
                        }
                    }
                    //possibly direction
                }
                else
                {
                    //target isn't null
                    outcome = ability.ResolveAction(this, activatedCharacter, null, targetCharacter, options);
                    return outcome; //TODO: get from resolve action
                }
            }
            else if(target.Count == 0)
            {
                outcome = ability.ResolveAction(this, activatedCharacter, null, null, options);
                return outcome;
            }

            throw new NotImplementedException();
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
                Map.Direction direction;
                try
                {
                    direction = Map.ParseDirection(dir);
                    var o = MoveCharacter(guy, direction);
                    outcome.Message.AppendLine(o.Message.ToString());
                }
                catch
                {
                    outcome.Message.AppendLine(dir + " is not a real direction, try N S E W NE NW SE SW");
                }
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

            if(guy.Incapacitated)
            {
                outcome.Message.AppendLine(guy.Name + " is incapacitated and cannot move");
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
            bool adjacentToOpponent = IsAdjacentToOpponent(guy.Pos, guy.SideID);

            if(adjacentToOpponent && guy.Stamina.Value < 1)
            {
                outcome.Message.AppendLine("Cannot move through threatened area without stamina");
                return outcome;
            }

            var moveCost = map.GetTile(guy.Pos).MoveCost;
            if(adjacentToOpponent)
            {
                moveCost++; //effective +1
            }
            if(moveCost > (guy.Move.Value + guy.Stamina.Value))
            {
                outcome.Message.AppendLine(guy.Name + " only has " + guy.Move.Value + " movement" +
                    " remaining, but needs " + moveCost + " to move");
                return outcome; //TODO: error
            }

            moveCost = map.GetTile(guy.Pos).MoveCost;
            int staminaCost = adjacentToOpponent ? 1 : 0;
            guy.PayMovement(moveCost, staminaCost);

            var oldPos = guy.Pos;
            guy.Pos = destination;
            if(OnCharacterMoved != null)
            {
                OnCharacterMoved(this, new CharacterMovedEventArgs(guy.Name, guy.ID, oldPos, destination));
            }
            outcome.Message.AppendLine(guy.Name + " moved to " + destination);
            return outcome;
        }

        public Dictionary<Character.StatType, int> CostToMove(Character guy, Position from, Position to)
        {
            //TODO: use this to do actual move calculations?
            var cost = new Dictionary<Character.StatType, int>();
            if(IsAdjacentToOpponent(from, guy.SideID))
            {
                cost.Add(Character.StatType.Stamina, 1);
            }
            else
            {
                cost.Add(Character.StatType.Stamina, 0);
            }
            var tile = map.GetTile(from);
            cost.Add(Character.StatType.Move, tile.MoveCost);
            return cost;
        }

        public Dictionary<Position, Dictionary<Character.StatType, int>> GetPossibleMoves(Character guy)
        {
            var moves = new Dictionary<Position, Dictionary<Character.StatType, int>>();

            var frontier = new Queue<Position>();
            frontier.Enqueue(guy.Pos);
            moves.Add(guy.Pos, new Dictionary<Character.StatType, int>()
            { { Character.StatType.Move, 0 }, {Character.StatType.Stamina, 0 } });

            while(frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                foreach (var neighbor in Map.GetAdjacent(current))
                {
                    if(moves.Keys.Contains(neighbor))
                    {
                        //TODO: check if shorter?
                    }
                    else
                    {
                        if(IsOpen(neighbor))
                        {
                            //hard coded to only cost Move and Stamina
                            var cost = CostToMove(guy, current, neighbor);
                            var costSoFar = moves[current];
                            cost[Character.StatType.Move] += costSoFar[Character.StatType.Move]; 
                            cost[Character.StatType.Stamina] += costSoFar[Character.StatType.Stamina]; 
                            if (cost[Character.StatType.Move] + cost[Character.StatType.Stamina] <=
                                guy.Move.Max + guy.Stamina.Max)
                            {
                                moves.Add(neighbor, cost);
                                frontier.Enqueue(neighbor);
                            }
                        }
                       
                    }
                }
            }

            return moves;
        }

        public List<Position> GetValidTargetsFor(Character guy, Mechanics.Action action)
        {
            return action.GetValidTargets(this, guy);
        }

        private bool IsAdjacentToOpponent(Position pos, Guid SideID)
        {
            if(Map.GetAdjacent(pos).Any(
                p => Characters.Where(c => c.SideID != SideID && !c.Incapacitated)
                .Select(c => c.Pos).Contains(p)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsOpen(Position pos)
        {
            if(Characters.Any(c => c.Pos == pos))
            {
                return false;
            }

            return map.IsPassable(pos);
        }

        public int GetGamestateHash()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ map.GetGamestateHash();
                foreach (var side in Sides)
                {
                    hash = (hash * 16777619) ^ side.GetGamestateHash();
                }
                //foreach (var guy in Characters)
                //{
                //    hash = (hash * 16777619) ^ guy.GetHashCode();
                //}
                //hash = (hash * 16777619) ^ TurnCount.GetHashCode();
                //hash = (hash * 16777619) ^ currentSide.GetGamestateHash();
                //if(activatedCharacter != null)
                //{
                //    hash = (hash * 16777619) ^ activatedCharacter.GetHashCode();
                //}

                return hash;
            }
        }

        public static DungeonMaster GetDebugDM(GameData data)
        {
            var DM = new DungeonMaster(data);

            DM.map = DebugData.GetFlatlandMap(data);

            DM.Sides.Add(new Side("Heros"));
            DM.Sides.Add(new Side("The Foe"));
            DM.currentSide = DM.Sides[0];

            var robby = DebugData.GetDebugCharacter();
            robby.InitializeIndividual("Robby", new Position(1, 1), DM.Sides[0].ID);
            DM.Characters.Add(robby);

            var zach = DebugData.GetDebugCharacter();
            zach.InitializeIndividual("Zach", new Position(1, 3), DM.Sides[1].ID);
            DM.Characters.Add(zach);

            return DM;
        }

        public object Clone()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<DungeonMaster>(json);
        }

        public static Random rand = new Random();

        public delegate void CharacterMovedEventHandler(object sender, CharacterMovedEventArgs a);
    }
}
