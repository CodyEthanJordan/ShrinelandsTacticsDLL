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
        public Guid currentSideID { get;  set; }
        [JsonProperty]
        public Guid activatedCharacterID;
        [JsonProperty]
        public Queue<Guid> GuidsInWaiting = new Queue<Guid>();

        [JsonIgnore]
        public Character activatedCharacter { get
            {
                return Characters.FirstOrDefault(c => c.ID == activatedCharacterID);
            } }

        [JsonIgnore]
        public Side currentSide
        {
            get
            {
                return Sides.FirstOrDefault(s => s.ID == currentSideID);
            }
        }

        public event CharacterMovedEventHandler OnCharacterMoved;
        public event EventHandler<Character> OnCharacterCreated;
        public event EventHandler<Guid> OnTurnPassed;
        public event CardDrawnEventHandler OnCardDrawn;
        public event StatChnagedEventHandler OnStatChanged;
        public event EventHandler<Position> OnTileChanged;

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

        public void CardDrawn(object sender, CardDrawnEventArgs e)
        {
            if(OnCardDrawn != null)
            {
                OnCardDrawn(this, e);
            }
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

        public void ApplyOutcome(Outcome outcome)
        {
            if(outcome.ActionTaken == "Move")
            {
                var guy = Characters.First(c => c.ID == outcome.UserID);
                MoveCharacter(guy, outcome.PosTarget);
            }
            else if(outcome.ActionTaken == "Activate")
            {
                var guy = Characters.First(c => c.ID == outcome.UserID);
                Activate(guy);
            }
            else if(outcome.ActionTaken == "Deactivate")
            {
                var guy = Characters.First(c => c.ID == outcome.UserID);
                Deactivate(guy);
            }
            else if(outcome.ActionTaken == "End Turn")
            {
                EndTurn(outcome.SideID);
            }
            else if(outcome.ActionTaken == null || outcome.ActionTaken == "")
            {
                //TODO: log error? do nothing for now
            }
            else
            {
                Deck.SetFate(outcome.CardsDrawn.Select(c => c.Name)); //TODO: terrible hack
                UseAbility(outcome.ActionTaken, outcome.UserID, outcome.TargetID, outcome.PosTarget);
            }
        }

        public void TeleportTo(Character guy, Position posTarget)
        {
            var oldPos = guy.Pos;
            guy.Pos = posTarget;
            var steppedOn = map.GetTile(posTarget);
            steppedOn.CharacterEntered(this, guy); //TODO: better?
            if(OnCharacterMoved != null) //TODO: make this an event from Character like stats
            {
                OnCharacterMoved(this, new CharacterMovedEventArgs(guy.Name, guy.ID, oldPos, posTarget));
            }
        }

        private void MoveCharacter(Character guy, Position posTarget)
        {
            var offset = posTarget - guy.Pos;
            var dir = Map.DirectionToPosition.FirstOrDefault(x => x.Value == offset).Key;
            MoveCharacter(guy, dir);
        }

        public Outcome EndTurn(Guid SideId)
        {
            var outcome = new Outcome();
            outcome.SideID = SideId;

            if(SideId != currentSideID)
            {
                outcome.Message.AppendLine("Not currently your turn, so cannot end");
                outcome.Illegal = true;
                return outcome;

            }

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
            currentSideID = Sides[i].ID;

            foreach (var guy in Characters.FindAll(c => c.SideID == currentSide.ID))
            {
                guy.NewTurn();
            }

            if(OnTurnPassed != null)
            {
                OnTurnPassed(this, currentSide.ID);
            }

            outcome.ActionTaken = "End Turn";
            return outcome;
        }

        public static DungeonMaster CreateFromMap(string level, GameData data)
        {
            var DM = new DungeonMaster(data);
            DM.map = Map.CreateFromText(level, data);
            DM.SetupEvents();
            return DM;
        }

        public static DungeonMaster LoadEncounter(YamlMappingNode yaml, string level, GameData data)
        {
            var DM = DungeonMaster.CreateFromMap(level, data);

            var sides = (YamlSequenceNode)yaml.Children[new YamlScalarNode("sides")];
            foreach (var side in sides)
            {
                var name = (side as YamlScalarNode).Value;
                DM.Sides.Add(new Side(name));
            }
            DM.currentSideID = DM.Sides[0].ID;

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
                newCharacter.InitializeIndividual(name, pos, side.ID, Guid.NewGuid());

                DM.CreateCharacter(newCharacter);
            }

            for (int i = 0; i < 100; i++)
            {
                DM.GuidsInWaiting.Enqueue(Guid.NewGuid());
            }

            return DM;
        }

        public void SetupEvents()
        {
            foreach (var guy in Characters)
            {
                guy.OnStatChanged += StatChanged;
                guy.SetupEvents();
            }
            map.OnTileChanged += TileChanged;
        }

        private void TileChanged(object sender, Position e)
        {
            if(OnTileChanged != null)
            {
                OnTileChanged(this, e);
            }
        }

        public void CreateCharacter(Character cloneCharacter)
        {
            cloneCharacter.OnStatChanged += StatChanged;
            Characters.Add(cloneCharacter);
            if(OnCharacterCreated != null)
            {
                OnCharacterCreated(this, cloneCharacter);
            }
        }

        private void StatChanged(object sender, StatChangedEventArgs a)
        {
            if(OnStatChanged != null)
            {
                OnStatChanged(this, a);
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
            outcome.ActionTaken = "Activate";
            outcome.UserID = guy.ID;

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
            var tile = map.GetTile(guy.Pos);
            tile.CharacterActivated(this, guy);
            activatedCharacterID = guy.ID;
            outcome.Message.AppendLine("Starting activation for " + guy.Name);
            return outcome;
        }

        public Outcome Deactivate(Character guy)
        {
            var outcome = new Outcome();
            outcome.ActionTaken = "Deactivate";
            outcome.UserID = guy.ID;
            if(guy != activatedCharacter)
            {
                return outcome; //TODO: error
            }


            //automatically use dodge as a convencience factor
            var dodge = guy.Actions.FirstOrDefault(a => a.Name == "Dodge");
            if(dodge != null)
            {
                while(guy.CanPay(dodge))
                {
                    dodge.ResolveAction(this, guy, null, null, "", outcome);
                }
            }

            guy.EndActivation(); //TODO: add to outcome?

            var deactivatedOn = map.GetTile(guy.Pos);
            deactivatedOn.CharacterDeactivated(guy);

            activatedCharacterID = Guid.Empty;
            return outcome;
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

        public Outcome UseAbility(string abilityName, Guid UserID, Guid TargetID, Position PosTarget)
        {
            var outcome = new Outcome();
            outcome.ActionTaken = abilityName;
            outcome.UserID = UserID;
            outcome.TargetID = TargetID;
            outcome.PosTarget = PosTarget;
            Deck.ClearTracking();
            var user = Characters.First(c => c.ID == UserID);
            var ability = user.Actions.First(a => a.Name == abilityName);
            var targetCharacter = Characters.FirstOrDefault(c => c.ID == TargetID);
            ability.ResolveAction(this, user, PosTarget, targetCharacter, "", outcome);
            return outcome;
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
                            return UseAbility(abilityIdentifier, activatedCharacter.ID, targetCharacter.ID, null);
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
                            return UseAbility(abilityIdentifier, activatedCharacter.ID, targetCharacter.ID, pos);
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
                    return UseAbility(abilityIdentifier, activatedCharacter.ID, targetCharacter.ID, null);
                }
            }
            else if(target.Count == 0) 
            {
                return UseAbility(abilityIdentifier, activatedCharacter.ID, Guid.Empty, null);
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
            outcome.SideID = guy.SideID;

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

            int moveCost = map.GetTile(guy.Pos).MoveCostFor(guy);
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

            moveCost = map.GetTile(guy.Pos).MoveCostFor(guy);
            int staminaCost = adjacentToOpponent ? 1 : 0;
            guy.PayMovement(moveCost, staminaCost);

            var oldPos = guy.Pos;
            guy.Pos = destination;
            if(OnCharacterMoved != null)
            {
                OnCharacterMoved(this, new CharacterMovedEventArgs(guy.Name, guy.ID, oldPos, destination));
            }

            var steppedOn = map.GetTile(destination);
            steppedOn.CharacterEntered(this, guy); //TODO: better?

            outcome.Message.AppendLine(guy.Name + " moved to " + destination);
            outcome.UserID = guy.ID;
            outcome.ActionTaken = "Move";
            outcome.PosTarget = destination;
            return outcome;
        }

        public Dictionary<Character.StatType, int> CostToMove(Character guy, Position from, Position to)
        {
            //TODO: use this to do actual move calculations?
            var cost = new Dictionary<Character.StatType, int>();
            if(IsAdjacentToOpponent(from, guy.SideID))
            {
                //TODO: more elegant, check for swarm tactics
                if(guy.HasTrait("Sneaky"))
                {
                    cost.Add(Character.StatType.Stamina, 0);
                }
                else
                {
                    cost.Add(Character.StatType.Stamina, 1);
                }
            }
            else
            {
                cost.Add(Character.StatType.Stamina, 0);
            }
            var tile = map.GetTile(from);
            cost.Add(Character.StatType.Move, tile.MoveCostFor(guy));
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
            DM.currentSideID = DM.Sides[0].ID;

            var robby = DebugData.GetDebugCharacter();
            robby.InitializeIndividual("Robby", new Position(1, 1), DM.Sides[0].ID, Guid.NewGuid());
            DM.Characters.Add(robby);

            var zach = DebugData.GetDebugCharacter();
            zach.InitializeIndividual("Zach", new Position(1, 3), DM.Sides[1].ID, Guid.NewGuid());
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
        public delegate void CardDrawnEventHandler(object sender, CardDrawnEventArgs a);
        public delegate void StatChnagedEventHandler(object sender, StatChangedEventArgs a);
    }
}
