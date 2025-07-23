using Application.DTOs;
using Domain;

namespace Application.Interfaces;

public interface IOrderService
{
    OrderResultDTO Create(PostOrderDTO dto);
    OrderResultDTO Update(UpdateOrderDTO dto);
    void Delete(int id);
    OrderResultDTO GetById(int id);
    List<OrderResultDTO> GetAll();
}