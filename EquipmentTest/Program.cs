using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Equipment.PickeringController;

namespace EquipmentTest
{
    class Program
    {

        static void Main(string[] args)
        {
            PickeringController controller = new PickeringController();

            controller.RTDWirte(6, 13, 1, 120);

            controller.TCWirte(6, 15, 1, 90);

            while (true)
            {
                Thread.Sleep(1);
            }


        }
    }
}
