using Domain;

namespace Application.Interfaces;

public interface IProductRepository
{
    Product Create(Product product);
    Product Update(Product product);
    void Delete(int id);
    Product GetById(int id);
    List<Product> GetAll();
    void Detach(Product existing);
}