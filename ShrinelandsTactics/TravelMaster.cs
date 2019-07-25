using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics
{
    public class TravelMaster
    {
        public int Day = 0;
        public Region region;

        private int nextWeatherDraw = 1;
        private Weather currentWeather;

        public event EventHandler<Weather> OnWeatherChanged;

        public TravelMaster()
        {
            SetupDebugData();
        }

        private void SetupDebugData()
        {
            region = new Region("Acaton Forest");
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
