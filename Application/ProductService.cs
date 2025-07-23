using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain;

namespace Application;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IOrderRepository orderRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public ProductResultDTO Create(PostProductDTO dto)
    {
        var order = _orderRepository.GetById(dto.OrderId);
        if (order == null)
            throw new InvalidOperationException("Order does not exist.");

        var product = _mapper.Map<Product>(dto);
        var created = _productRepository.Create(product);
        return _mapper.Map<ProductResultDTO>(created);
    }

    public ProductResultDTO Update(UpdateProductDTO dto)
    {
        var existing = _productRepository.GetById(dto.Id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found.");
        _productRepository.Detach(existing);
        var updated = _mapper.Map<Product>(dto);
        var result = _productRepository.Update(updated);
        return _mapper.Map<ProductResultDTO>(result);
    }

    public void Delete(int id)
    {
        var existing = _productRepository.GetById(id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found.");

        _productRepository.Delete(id);
    }

    public ProductResultDTO GetById(int id)
    {
        var product = _productRepository.GetById(id);
        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        return _mapper.Map<ProductResultDTO>(product);
    }

    public List<ProductResultDTO> GetAll()
    {
        var products = _productRepository.GetAll();
        return _mapper.Map<List<ProductResultDTO>>(products);
    }
}
