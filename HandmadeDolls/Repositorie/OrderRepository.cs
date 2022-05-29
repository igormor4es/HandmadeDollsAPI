using HandmadeDolls.Data;
using HandmadeDolls.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace HandmadeDolls.Repositorie;

public class OrderRepository
{
    public async Task<int> PostOrder(MinimalContextDb context, Order order, Object? customer, string connectionString)
    {
        var result = 1;

        result = await VerifyItensOrder(context, order);
        if (result != 1)
            return result;

        result = await VerifyUserAndCustomerClain(context, customer, connectionString);
        if (result != 1)
            return result;

        var saveOrder = await SaveOrder(context, order, customer, connectionString);

        if (order.OrderLists != null)
        {
            foreach (var productOrder in order.OrderLists)
            {
                var product = await new ProductRepository().GetProductById(productOrder.ProductId, context);

                await SaveOrderList(context, product, productOrder, saveOrder, connectionString);
            }
        }
        
        return result;
    }

    public async Task<int> VerifyItensOrder(MinimalContextDb context, Order order)
    {
        if (order.OrderLists != null)
        {
            foreach (var product in order.OrderLists)
            {
                var verifyStock = await new ProductRepository().GetProductById(product.ProductId, context);

                if (verifyStock?.Stock <= 0)
                {
                    return 2;
                }

                if (verifyStock?.Accessories != null)
                {
                    foreach (var gifts in verifyStock.Accessories)
                    {
                        var verifyStockGift = await context.Products.FindAsync(gifts.Id);

                        if (verifyStockGift?.Stock <= 0)
                        {
                            return 3;
                        }
                    }
                }
            }
        }

        return 1;
    }

    public async Task<int> VerifyUserAndCustomerClain(MinimalContextDb context, object? customer, string connectionString)
    {
        if (customer != null)
        {
            var verifyUser = await new UserRepository().VerifyUser(context, customer, connectionString);
            var verifyCustomerClain = await new UserRepository().VerifyCustomerClain(context, customer, connectionString);

            if (verifyUser == 0)
                return 4;

            if (verifyCustomerClain == 0)
                return 5;
        }

        return 1;
    }
    
    public async Task<int> SaveOrder(MinimalContextDb context, Order order, Object? customer, string connectionString)
    {
        Order newOrder = new()
        {
            CustomerId = await new UserRepository().GetUserId(context, customer, connectionString),
            OrderDate = DateTime.Now,
            OrderStatus = order.OrderStatus,
        };

        using (var connection = context.Database.GetDbConnection())
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();
            }
            
            DbCommand cmd = connection.CreateCommand();

            cmd.CommandText = $"INSERT Orders (CustomerId, OrderDate, OrderStatus) VALUES('{newOrder.CustomerId}', '{newOrder.OrderDate:yyyy-MM-dd HH:mm:ss}', {(int)(OrderStatus)Enum.Parse(typeof(OrderStatus), order.OrderStatus.ToString())})";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = $"SELECT Id FROM Orders WHERE CustomerId = '{newOrder.CustomerId}' AND OrderDate = '{newOrder.OrderDate:yyyy-MM-dd HH:mm:ss}'";
            var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                newOrder.Id = reader.GetInt32(0);
            }
            
            await connection.CloseAsync();
        }

        return newOrder.Id;
    }

    public async Task SaveOrderList(MinimalContextDb context, Product? product, OrderList? productOrder, int saveOrder, string connectionString)
    {
        if (product != null)
        {
            OrderList listOrder = new()
            {
                Price = product.Price,
                Quantity = productOrder.Quantity,
                OrderId = saveOrder,
                ProductId = product.Id,
            };

            //using (var connection = context.Database.GetDbConnection())
            //{
            //    if (connection.State != ConnectionState.Open)
            //    {
            //        connection.ConnectionString = connectionString;
            //        await connection.OpenAsync();
            //    }

            //    DbCommand cmd = connection.CreateCommand();

            //    cmd.CommandText = $"INSERT Orders (CustomerId, OrderDate, OrderStatus) VALUES('{newOrder.CustomerId}', '{newOrder.OrderDate:yyyy-MM-dd HH:mm:ss}', {(int)(OrderStatus)Enum.Parse(typeof(OrderStatus), order.OrderStatus.ToString())})";
            //    await cmd.ExecuteNonQueryAsync();

            //    cmd.CommandText = $"SELECT Id FROM Orders WHERE CustomerId = '{newOrder.CustomerId}' AND OrderDate = '{newOrder.OrderDate:yyyy-MM-dd HH:mm:ss}'";
            //    var reader = await cmd.ExecuteReaderAsync();

            //    while (reader.Read())
            //    {
            //        newOrder.Id = reader.GetInt32(0);
            //    }
            //}
        }
    }
}
