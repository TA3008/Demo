﻿using Demo.Application.Repositories;
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

        List<Order> IOrderService.GetOrdersByUsername(string username)
        {
            return _orderRepository.Find(x => x.Username == username).ToList();
        }

        List<Order> IOrderService.GetActivePendingPaidOrdersByUsername(string username)
        {
            return _orderRepository.Find(x => x.Username == username &&
                                          (x.Status == OrderStatus.Pending ||
                                           x.Status == OrderStatus.Paid)).ToList();
        }
    }
}
