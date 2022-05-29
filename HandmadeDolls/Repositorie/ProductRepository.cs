using HandmadeDolls.Data;
using HandmadeDolls.Models;
using Microsoft.EntityFrameworkCore;

namespace HandmadeDolls.Repositorie;

public class ProductRepository
{
    public async Task<List<Product>> GetAllProducts(MinimalContextDb context)
    {
        var products = await context.Products.Where(p => p.Active).ToListAsync();

        foreach (var item in products)
        {
            if (item.ProductType.ToString() == "DOLL")
            {
                var accessories = await context.DollsAccessories.AsNoTracking().Where(a => a.DollId == item.Id).ToListAsync();

                if (accessories.Count > 0)
                {
                    item.Accessories = new List<DollAcessory>();

                    foreach (var itens in accessories)
                    {
                        var prod = await context.Products.Where(p => p.Active).FirstOrDefaultAsync(p => p.Id == itens.AccessoryId);

                        var newProduct = new DollAcessory
                        {
                            Id = prod.Id,
                            Description = prod.Description
                        };

                        item.Accessories.Add(newProduct);
                    }
                }
            }
        }

        return products;
    }

    public async Task<Product?> GetProductById(int id, MinimalContextDb context)
    {
        var productById = await context.Products.Where(p => p.Active).FirstOrDefaultAsync(p => p.Id == id);

        if (productById?.ProductType.ToString() == "DOLL")
        {
            var accessories = await context.DollsAccessories
                                           .AsNoTracking()
                                           .Where(a => a.DollId == productById.Id)
                                           .ToListAsync();
            if (accessories.Count > 0)
            {
                productById.Accessories = new List<DollAcessory>();

                foreach (var item in accessories)
                {
                    var prod = await context.Products.Where(p => p.Active).FirstOrDefaultAsync(p => p.Id == item.AccessoryId);

                    var newProduct = new DollAcessory
                    {
                        Id = prod.Id,
                        Description = prod.Description
                    };

                    productById.Accessories.Add(newProduct);
                }
            }
        }

        return productById;
    }

    public async Task<int> PostProduct(MinimalContextDb context, Product product)
    {
        var result = 0;

        if (product.ProductType.ToString() == "DOLL")
        {
            var accessories = product.Accessories?.ToList();

            if (accessories != null && accessories.Count > 0)
            {
                var newProduct = new Product
                {
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    Image = product.Image,
                    Active = product.Active,
                    ProductType = product.ProductType,
                };

                context.Products.Add(newProduct);
                result = await context.SaveChangesAsync();

                foreach (var item in accessories)
                {
                    var accessory = await context.Products.FirstOrDefaultAsync(p => p.Id == item.AccessoryId);
                    var accessoryCount = context.DollsAccessories.Count(p => p.AccessoryId == item.AccessoryId);

                    if (accessory != null)
                    {
                        if (accessory.ProductType == ProductType.DOLL)
                        {
                            await DeleteProduct(context, newProduct);
                            return result = 2;
                        }

                        if (!accessory.Active)
                        {
                            await DeleteProduct(context, newProduct);
                            return result = 3;
                        }

                        if (accessory.Stock <= 0)
                        {
                            await DeleteProduct(context, newProduct);
                            return result = 4; 
                        }

                        if (accessory.Stock - accessoryCount <= 0)
                        {
                            await DeleteProduct(context, newProduct);
                            return result = 5;
                        }

                        DollAcessory dollAcessory = new()
                        {
                            DollId = newProduct.Id,
                            AccessoryId = accessory.Id
                        };

                        context.DollsAccessories.Add(dollAcessory);
                        result = await context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                context.Products.Add(product);
                result = await context.SaveChangesAsync();
            }
        }
        else
        {
            context.Products.Add(product);
            result = await context.SaveChangesAsync();
        }

        return result;
    }

    public async Task DeleteProduct(MinimalContextDb context, Product newProduct)
    {
        using (var connection = context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"DELETE DollsAccessories WHERE DollId = {newProduct.Id}";
                await command.ExecuteNonQueryAsync();

                command.CommandText = $"DELETE Products WHERE Id = {newProduct.Id}";
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
