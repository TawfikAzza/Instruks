using Application.DTOs;
using Domain;

namespace Application.Interfaces;

public interface IProductService
{
    ProductResultDTO Create(PostProductDTO dto);
    ProductResultDTO Update(UpdateProductDTO dto);
    void Delete(int id);
    ProductResultDTO GetById(int id);
    List<ProductResultDTO> GetAll();
}