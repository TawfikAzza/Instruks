using System;
using Domain; // <- your entities live here

namespace InstruksTests.TestHelpers; // <- NOT starting with "Instruks"

public static class Fakes
{
    public static Domain.Category Cat(Guid? id = null, string name = "General") =>
        new Domain.Category
        {
            Id = id ?? Guid.NewGuid(),
            Name = name
        };

    public static Domain.Instruks Instruks(
        Guid? id = null,
        Guid? categoryId = null,
        string title = "Hand Hygiene") =>
        new Domain.Instruks
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Description = "How to wash hands",
            Content = "<p>Soap + water</p>",
            CategoryId = categoryId ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            // set versioning fields here if you have them
            // RootId = ...; VersionNumber = 1; IsLatest = true;
        };
}