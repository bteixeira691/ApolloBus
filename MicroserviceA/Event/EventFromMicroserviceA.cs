using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroserviceA.Event
{
    public class EventFromMicroserviceA : ApolloBus.Events.Event
    {
        public string Name { get; set; }

        public string Message { get; set; }
    }
}
