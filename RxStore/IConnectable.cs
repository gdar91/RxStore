using System;

namespace RxStore
{
    internal interface IConnectable : IDisposable
    {
        void Connect();
    }

    internal interface IConnectableStore : IConnectable
    { }
    
    internal interface IConnectableEffects : IConnectable
    { }
}
