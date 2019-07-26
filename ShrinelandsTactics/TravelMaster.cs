using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.BasicStructures.Events;
using ShrinelandsTactics.World;
using ShrinelandsTactics.World.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShrinelandsTactics.DungeonMaster;

namespace ShrinelandsTactics
{
    public class TravelMaster
    {
        public int Day = 0;
        public Region region;

        private int nextWeatherDraw = 1;
        private Weather currentWeather;

        public int Condition = 6; //TODO: have individual parties

        public event EventHandler<Weather> OnWeatherChanged;
        public event CardDrawnEventHandler OnCardDrawn;
        public event EventHandler<string> OnEncounterOutcome;

        public TravelMaster()
        {
            SetupDebugData();
        }

        private void SetupDebugData()
        {
            region = new Region("Acaton Forest");
        }

        private int ResolveSource(string source)
        {
            switch (source)
            {
                case "Condition":
                    return Condition;
                default:
                    return int.Parse(source);
            }
        }

        public void ChooseOption(Encounter encounter, int option)
        {
            var optionChosen = encounter.Options[option];
            switch (optionChosen["Outcome"] as string)
            {
                case "SkillCheck":
                    var recipie = optionChosen["SkillCheck"]["Cards"];
                    Deck deck = new Deck();
                    var originalDeck = deck.Clone() as Deck;

                    foreach (var ingredient in recipie)
                    {
                        var name = ingredient["Name"] as string;
                        var source = ingredient["Number"] as string;
                        int number = ResolveSource(source);
                        var card = new Card(name, Card.CardType.Encounter);
                        deck.AddCards(card, number);
                    }
                    var outcome = deck.Draw();
                    var result = optionChosen["SkillCheck"]["Result"][outcome.Name];

                    if(OnCardDrawn != null)
                    {
                        OnCardDrawn(this, new CardDrawnEventArgs(originalDeck, outcome));
                    }

                    ApplyEncounterEffect(result["Effect"]);

                    if(OnEncounterOutcome != null)
                    {
                        OnEncounterOutcome(this, result["Prompt"]);
                    }
                    //TODO: apply effects
                    break;
                default:
                    break;
            }
        }

        private void ApplyEncounterEffect(string effect)
        {
            if(effect.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return; //do nothing
            }

            //TODO: other effects

        }

        public void NewDay()
        {
            //update day counter
            Day++;


            //check if need to draw new weather card
            if(nextWeatherDraw == Day)
            {
                Weather newWeather = region.RollWeather();
                nextWeatherDraw = Day + newWeather.Duration;
                currentWeather = newWeather;
                if(OnWeatherChanged != null)
                {
                    OnWeatherChanged(this, newWeather);
                }
            }
        }

        public void Travel(string caravan)
        {
            NewDay();


        }
    }
}
