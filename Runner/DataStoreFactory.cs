using System;
using System.Collections.Generic;
using Gauge.Messages;

namespace Gauge.CSharp.Runner
{
    public static class DataStoreFactory
    {
        private static readonly Dictionary<IFormattable, DataStore> DataStores = new Dictionary<IFormattable, DataStore>
        {
            {Message.Types.MessageType.ScenarioDataStoreInit, new DataStore()},
            {Message.Types.MessageType.SpecDataStoreInit, new DataStore()},
            {Message.Types.MessageType.SuiteDataStoreInit, new DataStore()}
        };

        public static DataStore GetDataStoreFor(IFormattable messageType)
        {
            return DataStores[messageType];
        }
    }
}