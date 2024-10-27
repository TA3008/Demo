using Demo.Application.Repositories;
using Demo.Core.Models;
using Demo.Core.ValueObjects;

namespace Demo.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // public List<Orders> GetOrdersByUsername(string username)
        // {
        //     return _orderRepository.Find(x => x.Username == username).ToList();
        // }

        // public List<Orders> GetActiveOrdersByUsername(string username)
        // {
        //     return _orderRepository.Find(x => x.Username == username && x.Status == OrderStatus.Active).ToList();
        // }

        // public List<Orders> GetActivePendingPaidOrdersByUsername(string username)
        // {
        //     return _orderRepository.Find(x => x.Username == username && (x.Status == OrderStatus.Active
        //     || x.Status == OrderStatus.Pendding
        //     || x.Status == OrderStatus.Paid)).ToList();
        // }

        // public List<Orders> GetByFarmer(Guid farmerId)
        // {
        //     return _orderRepository.Find(x => x.FarmerId == farmerId).ToList();
        // }

        List<Order> IOrderService.GetOrdersByUsername(string username)
        {
            return _orderRepository.Find(x => x.Username == username).ToList();
        }

        List<Order> IOrderService.GetActiveOrdersByUsername(string username)
        {
            return _orderRepository.Find(x => x.Username == username && x.Status == OrderStatus.Active).ToList();
        }

        List<Order> IOrderService.GetActivePendingPaidOrdersByUsername(string username)
        {
            return _orderRepository.Find(x => x.Username == username &&
                                          (x.Status == OrderStatus.Active ||
                                           x.Status == OrderStatus.Pending ||
                                           x.Status == OrderStatus.Paid)).ToList();
        }
    }
}
