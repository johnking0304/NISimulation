using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pickering.Pilpxi.Interop;


namespace Equipment.PickeringController
{
    public class PickeringController
    {
        public void RTDWirte(int bus,int solt,int channel ,double value)
        {
            int card = 0;
            try
            {
                int status = PilpxiModule.OpenSpecifiedCard(bus, solt, ref card);         // initializing card, returning the session handle
                if (status == 0)
                {
                   status = PilpxiModule.ResSetResistance(card, channel,0, value);                  
                }
                PilpxiModule.CloseSpecifiedCard(card);
            }
            catch
            { 
                        
            }

        }

        public void TCWirte(int bus, int solt, int channel, double value)
        {
            int card = 0;
            try
            {
                int status = PilpxiModule.OpenSpecifiedCard(bus, solt, ref card);         // initializing card, returning the session handle
                if (status == 0)
                {
                    status = PilpxiModule.VsourceSetVoltage(card, channel, value);
                    
                }
                PilpxiModule.CloseSpecifiedCard(card);
            }
            catch
            {

            }
        }

    }
}
