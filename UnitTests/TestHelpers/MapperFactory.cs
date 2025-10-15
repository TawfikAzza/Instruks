using AutoMapper;

namespace InstruksTests.TestHelpers
{
    /// <summary>
    /// Provides a single AutoMapper instance for all tests, registering
    /// only the application's AutoMapperProfile exactly once.
    /// </summary>
    public static class MapperFactory
    {
        private static readonly Lazy<IMapper> Lazy = new(() =>
        {
            var cfg = new MapperConfiguration(c =>
            {
                // Register exactly one profile — avoid scanning all assemblies
                c.AddProfile<Application.Mapping.AutoMapperProfile>();
            });

            // Fail fast if mappings are broken
            cfg.AssertConfigurationIsValid();

            return cfg.CreateMapper();
        });

        /// <summary>Get the shared IMapper for tests.</summary>
        public static IMapper Create() => Lazy.Value;
    }
}