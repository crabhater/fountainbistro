using FountainBistro.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FountainBistro.Web.Infrastructure.Data.SeedData;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Проверяем, есть ли уже продукты
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var products = new List<Product>
        {
            // Десерты
            new() { Id = Guid.NewGuid(), Name = "Чизкейк Сан-Себастьян", Price = 550m, Category = "Десерты", IsAvailable = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Медовик классический", Price = 390m, Category = "Десерты", IsAvailable = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Трио пирожное картошка", Price = 590m, Category = "Десерты", IsAvailable = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Мороженое пломбир", Price = 250m, Category = "Десерты", IsAvailable = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Мороженое шоколад", Price = 250m, Category = "Десерты", IsAvailable = true, SortOrder = 5, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Мороженое клубничное", Price = 250m, Category = "Десерты", IsAvailable = true, SortOrder = 6, CreatedAt = DateTime.UtcNow },
            
            // Завтраки
            new() { Id = Guid.NewGuid(), Name = "Английский завтрак", Description = "300г", Price = 520m, Category = "Завтраки", IsAvailable = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Гречка с курицей и сливочным муссом", Description = "250г", Price = 490m, Category = "Завтраки", IsAvailable = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Зеленые оладьи с лососем", Description = "230г", Price = 610m, Category = "Завтраки", IsAvailable = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Рисовая каша с персиком", Description = "250г", Price = 490m, Category = "Завтраки", IsAvailable = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Сырники с маком и ягодным конфи", Description = "170г", Price = 390m, Category = "Завтраки", IsAvailable = true, SortOrder = 5, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Бутер с моцареллой и ягодным конфи", Price = 310m, Category = "Завтраки", IsAvailable = true, SortOrder = 6, CreatedAt = DateTime.UtcNow },
            
            // Меню
            new() { Id = Guid.NewGuid(), Name = "Бутер с вялеными томатами", Description = "300г", Price = 610m, Category = "Меню", IsAvailable = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Бутер с креветками и манго-соусом", Description = "340г", Price = 650m, Category = "Меню", IsAvailable = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Бутер с лососем и соусом песто", Description = "320г", Price = 650m, Category = "Меню", IsAvailable = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Паштет из куриной печени с луком конфи", Description = "250г", Price = 480m, Category = "Меню", IsAvailable = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Хумус с вялеными томатами", Description = "250г", Price = 450m, Category = "Меню", IsAvailable = true, SortOrder = 5, CreatedAt = DateTime.UtcNow }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
