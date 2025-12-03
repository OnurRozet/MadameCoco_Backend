using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Shared.IntegrationEvents
{
    public record OrderCreatedEvent(
        Guid OrderId,
        Guid CustomerId,
        Guid ProductId,
        string ProdcutName,
        int Quantity,
        decimal TotalPrice,
        DateTime CreatedAt
    );

    public record OrderStatusChangedEvent(
        Guid OrderId,
        Guid CustomerId,
        string NewStatus,
        DateTime UpdatedAt
     );
}
