using ApolloBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.InterfacesAbstraction
{
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : Event
    {
        Task Handler(TEvent _event);
    }

    public interface IEventHandler
    {

    }

}
