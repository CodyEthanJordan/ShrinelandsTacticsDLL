﻿using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics;

namespace ShrinelandsTactics
{
    public class DungeonMaster
    {
        public Map map;
        public List<Side> Sides = new List<Side>();
        public List<Character> Characters = new List<Character>();
        public int TurnCount = 0;

        private GameData data;
        public Side currentSide { get; private set; }
        public Character activatedCharacter = null;

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
            return false; //TODO: implement
            throw new NotImplementedException();
        }

        public Outcome UseAbility(string abilityIdentifier, List<string> target)
        {
            var outcome = new Outcome();
            if(activatedCharacter == null)
            {
                outcome.Message.AppendLine("No activated character");
                return outcome;
            }

            var ability = activatedCharacter.Actions.FirstOrDefault(a => a.Name.Equals(abilityIdentifier, StringComparison.OrdinalIgnoreCase));
            if(ability == null)
            {
                //try parsing as int
                int i;
                if(int.TryParse(abilityIdentifier, out i))
                {
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
                    //possibly direction
                    try
                    {
                        var dir = Map.ParseDirection(target[0]);
                        var pos = activatedCharacter.Pos + Map.DirectionToPosition[dir];
                        targetCharacter = Characters.FirstOrDefault(c => c.Pos == pos);
                        outcome = ability.ResolveAction(this, activatedCharacter, pos, targetCharacter, "");
                        return outcome; //TODO: get from resolve action
                    }
                    catch
                    {
                        outcome.Message.AppendLine("Cannot target " + target[0]);
                        return outcome;
                    }
                }
                else
                {
                    //target isn't null
                    outcome = ability.ResolveAction(this, activatedCharacter, null, targetCharacter, "");
                    return outcome; //TODO: get from resolve action
                }
            }
            else
            {
                throw new NotImplementedException();
            }
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

            var robby = DebugData.GetDebugCharacter();
            robby.InitializeIndividual("Robby", new Position(1, 1), DM.Sides[0].ID);
            DM.Characters.Add(robby);

            var zach = DebugData.GetDebugCharacter();
            zach.InitializeIndividual("Zach", new Position(1, 3), DM.Sides[1].ID);
            DM.Characters.Add(zach);

            return DM;
        }

    }
}
