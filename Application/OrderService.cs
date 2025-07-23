using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain;

namespace Application;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public OrderResultDTO Create(PostOrderDTO dto)
    {
        var order = _mapper.Map<Order>(dto);
        var created = _orderRepository.Create(order);
        return _mapper.Map<OrderResultDTO>(created);
    }

    public OrderResultDTO Update(UpdateOrderDTO dto)
    {
        var existing = _orderRepository.GetById(dto.Id);
        if (existing == null)
            throw new KeyNotFoundException("Order not found.");
        _orderRepository.Detach(existing);
        var updated = _mapper.Map<Order>(dto);
        var result = _orderRepository.Update(updated);
        return _mapper.Map<OrderResultDTO>(result);
    }

    public void Delete(int id)
    {
        var existing = _orderRepository.GetById(id);
        if (existing == null)
            throw new KeyNotFoundException("Order not found.");

        _orderRepository.Delete(id);
    }

    public OrderResultDTO GetById(int id)
    {
        var order = _orderRepository.GetById(id);
        if (order == null)
            throw new KeyNotFoundException("Order not found.");

        return _mapper.Map<OrderResultDTO>(order);
    }

    public List<OrderResultDTO> GetAll()
    {
        var orders = _orderRepository.GetAll();
        return _mapper.Map<List<OrderResultDTO>>(orders);
    }
}