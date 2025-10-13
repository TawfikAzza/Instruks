// Instruks.UnitTests/Category/CategoryServiceTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Services;
using AutoMapper;
using Domain;
using FluentAssertions;
using Moq;
using InstruksTests.TestHelpers;

namespace InstruksTests.Category;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private readonly IMapper _mapper = MapperFactory.Create();
    private readonly ICategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repo.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_returns_mapped_list()
    {
        var entities = new List<Domain.Category>
        {
            new() { Id = Guid.NewGuid(), Name = "General" },
            new() { Id = Guid.NewGuid(), Name = "Emergency" }
        };

        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        var result = await _sut.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().BeEquivalentTo("General", "Emergency");
        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Category?)null);

        var dto = await _sut.GetByIdAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_mapped_dto()
    {
        var entity = new Domain.Category { Id = Guid.NewGuid(), Name = "Diagnostics" };
        _repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);

        var dto = await _sut.GetByIdAsync(entity.Id);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(entity.Id);
        dto.Name.Should().Be("Diagnostics");
    }

    [Fact]
    public async Task CreateAsync_maps_generates_id_and_persists()
    {
        var dto = new CategoryDto { Name = "Radiology" };

        _repo.Setup(r => r.AddAsync(It.IsAny<Domain.Category>()))
             .Returns(Task.CompletedTask);

        var created = await _sut.CreateAsync(dto);

        created.Should().NotBeNull();
        created.Id.Should().NotBe(Guid.Empty);
        created.Name.Should().Be("Radiology");

        _repo.Verify(r => r.AddAsync(It.Is<Domain.Category>(c => c.Name == "Radiology" && c.Id != Guid.Empty)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_returns_false_when_not_found()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Category?)null);

        var ok = await _sut.UpdateAsync(Guid.NewGuid(), new CategoryDto { Name = "Updated" });

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_maps_onto_entity_and_updates()
    {
        var id = Guid.NewGuid();
        var entity = new Domain.Category { Id = id, Name = "Old" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity)).Returns(Task.CompletedTask);

        var ok = await _sut.UpdateAsync(id, new CategoryDto { Id = id, Name = "New" });

        ok.Should().BeTrue();
        entity.Name.Should().Be("New");
        _repo.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_missing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Category?)null);

        var ok = await _sut.DeleteAsync(Guid.NewGuid());

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_deletes_and_returns_true()
    {
        var entity = new Domain.Category { Id = Guid.NewGuid(), Name = "ToRemove" };

        _repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        _repo.Setup(r => r.DeleteAsync(entity)).Returns(Task.CompletedTask);

        var ok = await _sut.DeleteAsync(entity.Id);

        ok.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(entity), Times.Once);
    }
}
