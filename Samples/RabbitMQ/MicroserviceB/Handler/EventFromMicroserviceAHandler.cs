using ApolloBus.InterfacesAbstraction;
using MicroserviceB.Event;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroserviceB.Handler
{
    public class EventFromMicroserviceAHandler : IEventHandler<EventFromMicroserviceA>
    {
        public async Task Handler(EventFromMicroserviceA _event)
        {
            string eventReceive = JsonConvert.SerializeObject(_event);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"..\MicroserviceB\EventMessage.txt", true))
            {
                await file.WriteLineAsync(eventReceive);
            }
           
        }
    }
}