using System;
using System.Collections.Generic;
using System.Text;

namespace Sales.MessageBus.Messages.InventoryService
{
    public class ReleasedStockEvent
    {
        public Guid OrderId { get; set; }
    }
}
