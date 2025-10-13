// Instruks.UnitTests/TestHelpers/MapperFactory.cs
using AutoMapper;
using Application.Mapping;

namespace InstruksTests.TestHelpers;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<AutoMapperProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }
}