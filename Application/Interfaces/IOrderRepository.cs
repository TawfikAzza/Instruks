using Domain;

namespace Application.Interfaces;

public interface IOrderRepository
{
    Order Create(Order order);
    Order Update(Order order);
    void Delete(int id);
    Order GetById(int id);
    List<Order> GetAll();
    void Detach(Order existing);
}