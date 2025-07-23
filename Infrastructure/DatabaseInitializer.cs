using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class DatabaseInitializer {
    public static void EnsureCreated(AppDbContext context) {
       // context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}