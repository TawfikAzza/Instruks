using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Services;
using AutoMapper;
using Domain;
using FluentAssertions;
using Moq;
using InstruksTests.TestHelpers; 

namespace InstruksTests.Instruks;

public class InstruksServiceTests
{
    private readonly Mock<IInstruksRepository> _repo = new();
    private readonly IMapper _mapper = MapperFactory.Create();
    private readonly IInstruksService _sut;

    public InstruksServiceTests()
    {
        _sut = new InstruksService(_repo.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_maps_list()
    {
        var items = new List<Domain.Instruks>
        {
            new() { Id = Guid.NewGuid(), Title = "A", CategoryId = Guid.NewGuid(), Content = "<p>a</p>", VersionNumber = 1, IsLatest = true },
            new() { Id = Guid.NewGuid(), Title = "B", CategoryId = Guid.NewGuid(), Content = "<p>b</p>", VersionNumber = 1, IsLatest = true }
        };

        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result.Select(x => x.Title).Should().BeEquivalentTo("A", "B");
        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByCategoryAsync_returns_only_requested_category()
    {
        var catId = Guid.NewGuid();
        var other = Guid.NewGuid();

        var items = new List<Domain.Instruks>
        {
            new() { Id = Guid.NewGuid(), Title = "InCat1", CategoryId = catId, Content = "<p>x</p>", VersionNumber = 1, IsLatest = true },
            new() { Id = Guid.NewGuid(), Title = "InCat2", CategoryId = catId, Content = "<p>y</p>", VersionNumber = 1, IsLatest = true },
            new() { Id = Guid.NewGuid(), Title = "OtherCat", CategoryId = other, Content = "<p>z</p>", VersionNumber = 1, IsLatest = true },
        };

        _repo.Setup(r => r.GetByCategoryAsync(catId))
             .ReturnsAsync(items.Where(i => i.CategoryId == catId).ToList());

        var result = await _sut.GetByCategoryAsync(catId);

        result.Should().HaveCount(2);
        result.Select(r => r.Title).Should().BeEquivalentTo("InCat1", "InCat2");
        _repo.Verify(r => r.GetByCategoryAsync(catId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Instruks?)null);

        var dto = await _sut.GetByIdAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_maps_entity_to_dto()
    {
        var entity = new Domain.Instruks
        {
            Id = Guid.NewGuid(),
            Title = "Doc",
            Description = "D",
            Content = "<p>c</p>",
            CategoryId = Guid.NewGuid(),
            VersionNumber = 1,
            IsLatest = true
        };

        _repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);

        var dto = await _sut.GetByIdAsync(entity.Id);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(entity.Id);
        dto.Title.Should().Be("Doc");
        _repo.Verify(r => r.GetByIdAsync(entity.Id), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_sets_versioning_fields_and_persists()
    {
        var dto = new InstruksDto
        {
            Title = "New",
            Description = "Desc",
            Content = "<p>html</p>",
            CategoryId = Guid.NewGuid()
        };

        Domain.Instruks? captured = null;

        _repo.Setup(r => r.AddAsync(It.IsAny<Domain.Instruks>()))
             .Callback<Domain.Instruks>(e => captured = e)
             .Returns(Task.CompletedTask);

        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var created = await _sut.CreateAsync(dto);

        created.Should().NotBeNull();
        created.Id.Should().NotBe(Guid.Empty);
        created.Title.Should().Be("New");

        captured.Should().NotBeNull();
        captured!.Id.Should().NotBe(Guid.Empty);
        captured.DocumentId.Should().NotBe(Guid.Empty);
        captured.VersionNumber.Should().Be(1);
        captured.IsLatest.Should().BeTrue();
        captured.CreatedAt.Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));
        captured.Content.Should().Be("<p>html</p>");
        captured.CategoryId.Should().Be(dto.CategoryId);

        _repo.Verify(r => r.AddAsync(It.IsAny<Domain.Instruks>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_returns_false_when_missing_or_not_latest()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Instruks?)null);

        var ok1 = await _sut.UpdateAsync(Guid.NewGuid(), new InstruksDto());
        ok1.Should().BeFalse();

        var existing = new Domain.Instruks { Id = Guid.NewGuid(), IsLatest = false };
        _repo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);

        var ok2 = await _sut.UpdateAsync(existing.Id, new InstruksDto());
        ok2.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_modifies_in_place_and_saves()
    {
        var existing = new Domain.Instruks
        {
            Id = Guid.NewGuid(),
            Title = "Old",
            Description = "OldD",
            Content = "<p>old</p>",
            CategoryId = Guid.NewGuid(),
            IsLatest = true
        };

        _repo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new InstruksDto
        {
            Title = "New",
            Description = "NewD",
            Content = "<p>new</p>",
            CategoryId = Guid.NewGuid()
        };

        var ok = await _sut.UpdateAsync(existing.Id, dto);

        ok.Should().BeTrue();
        existing.Title.Should().Be("New");
        existing.Description.Should().Be("NewD");
        existing.Content.Should().Contain("new");
        existing.CategoryId.Should().Be(dto.CategoryId);
        existing.UpdatedAt.Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));

        _repo.Verify(r => r.UpdateAsync(existing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateNewVersionAsync_returns_null_when_missing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Instruks?)null);

        var result = await _sut.CreateNewVersionAsync(Guid.NewGuid(), new InstruksDto());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateNewVersionAsync_demotes_current_and_inserts_next_with_incremented_version()
    {
        var current = new Domain.Instruks
        {
            Id = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            VersionNumber = 2,
            IsLatest = true,
            Title = "v2",
            Description = "old",
            Content = "<p>old</p>",
            CategoryId = Guid.NewGuid()
        };

        _repo.Setup(r => r.GetByIdAsync(current.Id)).ReturnsAsync(current);
        _repo.Setup(r => r.UpdateAsync(current)).Returns(Task.CompletedTask);

        Domain.Instruks? added = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Domain.Instruks>()))
             .Callback<Domain.Instruks>(e => added = e)
             .Returns(Task.CompletedTask);

        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new InstruksDto
        {
            Title = "v3",
            Description = "revised",
            Content = "<p>updated</p>",
            CategoryId = Guid.NewGuid()
        };

        var nextDto = await _sut.CreateNewVersionAsync(current.Id, dto);

        // old version demoted
        current.IsLatest.Should().BeFalse();

        // new version assertions
        added.Should().NotBeNull();
        added!.Id.Should().NotBe(Guid.Empty);
        added.DocumentId.Should().Be(current.DocumentId);
        added.VersionNumber.Should().Be(3);
        added.IsLatest.Should().BeTrue();
        added.PreviousVersionId.Should().Be(current.Id);
        added.Title.Should().Be("v3");
        added.Description.Should().Be("revised");
        added.Content.Should().Contain("updated");
        added.CategoryId.Should().Be(dto.CategoryId);
        added.CreatedAt.Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));

        // returned dto mirrors the new version
        nextDto.Should().NotBeNull();
        nextDto!.Title.Should().Be("v3");

        _repo.Verify(r => r.UpdateAsync(current), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<Domain.Instruks>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_missing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Domain.Instruks?)null);

        var ok = await _sut.DeleteAsync(Guid.NewGuid());

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_deletes_and_returns_true()
    {
        var entity = new Domain.Instruks { Id = Guid.NewGuid() };

        _repo.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        _repo.Setup(r => r.DeleteAsync(entity)).Returns(Task.CompletedTask);

        var ok = await _sut.DeleteAsync(entity.Id);

        ok.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(entity), Times.Once);
    }
}
