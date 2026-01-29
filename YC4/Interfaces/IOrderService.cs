using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YC4.Interfaces;

public interface IOrderService
{
    Task<Guid> PlaceOrderAsync(int userId, Guid eventId, List<Guid> seatIds);
}