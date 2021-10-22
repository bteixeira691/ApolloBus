using ApolloBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.InterfacesAbstraction
{
    public interface IApolloBus
    {
        Task Publish(Event _event);

        Task Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>;
    }
}
