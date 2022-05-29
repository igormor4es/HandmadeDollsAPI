using HandmadeDolls.DAL;
using HandmadeDolls.Data;
using HandmadeDolls.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using static HandmadeDolls.Data.MinimalContextDb;

namespace HandmadeDolls.Repositorie;

public class OrderRepository
{
    public async Task<List<Order>> GetAllOrders(MinimalContextDb context)
    {
        List<Order> orders = new();

        try
        {
            orders = await context.Orders.Include("OrderLists").Include("OrderLists.Product").ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }

        return orders;
    }

    public async Task<Order?> GetOrderById(int id, MinimalContextDb context)
    {
        Order? orderById = new();

        try
        {
            orderById = await context.Orders.Include("OrderLists").Include("OrderLists.Product").FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception)
        {
            throw;
        }

        return orderById;
    }
    public async Task<int> PostOrder(MinimalContextDb context, Order order, Object? customer)
    {
        var result = 1;

        try
        {
            result = await VerifyItensOrder(context, order);
            if (result != 1)
                return result;

            result = await VerifyUserAndCustomerClain(customer);
            if (result != 1)
                return result;


            var saveOrder = await SaveOrder(order, customer);

            if (order.OrderLists != null)
            {
                foreach (var productOrder in order.OrderLists)
                {
                    var product = await new ProductRepository().GetProductById(productOrder.ProductId, context);
                    await SaveOrderList(context, product, productOrder, saveOrder);
                }
            }
        }
        catch (Exception)
        {
            throw;
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

    public async Task<int> VerifyUserAndCustomerClain(object? customer)
    {
        if (customer != null)
        {
            var verifyUser = await new UserRepository().VerifyUser(customer);
            var verifyCustomerClain = await new UserRepository().VerifyCustomerClain(customer);

            if (verifyUser == 0)
                return 4;

            if (verifyCustomerClain == 0)
                return 5;
        }

        return 1;
    }

    public async Task<int> SaveOrder(Order order, Object? customer)
    {
        Order newOrder = new()
        {
            CustomerId = await new UserRepository().GetUserId(customer),
            OrderDate = DateTime.Now,
            OrderStatus = order.OrderStatus,
        };


        if (customer != null)
        {
            await using (Context ctx = new())
            {
                ctx.Connection.Open();
                DbCommand cmd = ctx.Connection.CreateCommand();
                cmd.CommandText = $"INSERT Orders (CustomerId, OrderDate, OrderStatus) VALUES('{newOrder.CustomerId}', '{newOrder.OrderDate:yyyy-MM-dd HH:mm:ss:fff}', @OrderStatus)";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(SQLUteis.GetSqlParameter("@OrderStatus", SqlDbType.Int, (int)(OrderStatus)Enum.Parse(typeof(OrderStatus), order.OrderStatus.ToString())));
                cmd.ExecuteNonQuery();
            }

            //Esse Select não é uma boa prática, o correto caso quisesse usar o banco, era criar uma Procedure que retorna o Id do INSERT acima.
            //Ocorreu um erro no EF e eu não iria conseguir resolver a tempo, por isso criei essa medida paliativa.
            await using (Context ctx = new())
            {
                ctx.Connection.Open();
                DbCommand cmd = ctx.Connection.CreateCommand();
                cmd.CommandText = $"SELECT Id FROM Orders WHERE CustomerId = '{newOrder.CustomerId}' AND OrderDate = '{newOrder.OrderDate:yyyy-MM-dd HH:mm:ss:fff}'";
                cmd.CommandType = CommandType.Text;

                using (SqlDataReader reader = (SqlDataReader)cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        newOrder.Id = reader.GetValue<int>("Id");
                    }
                }
            }
        }

        return newOrder.Id;
    }

    public async Task SaveOrderList(MinimalContextDb context, Product? product, OrderList? productOrder, int saveOrder)
    {
        if (product != null)
        {
            if (productOrder != null)
            {
                await using (Context ctx = new())
                {
                    ctx.Connection.Open();
                    DbCommand cmd = ctx.Connection.CreateCommand();
                    cmd.CommandText = $"INSERT OrderLists (Price, Quantity, OrderId, ProductId) VALUES(@Price, @Quantity, @OrderId, @ProductId)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(SQLUteis.GetSqlParameter("@Price", SqlDbType.Decimal, product.Price));
                    cmd.Parameters.Add(SQLUteis.GetSqlParameter("@Quantity", SqlDbType.Decimal, productOrder.Quantity));
                    cmd.Parameters.Add(SQLUteis.GetSqlParameter("@OrderId", SqlDbType.Int, saveOrder));
                    cmd.Parameters.Add(SQLUteis.GetSqlParameter("@ProductId", SqlDbType.Int, product.Id));
                    cmd.ExecuteNonQuery();
                }

                await new ProductRepository().UpdateStockProduct(context, product.Id, productOrder.Quantity);
            }

            if (product.Accessories != null)
            {
                foreach (var gifts in product.Accessories)
                {
                    await using (Context ctx = new())
                    {
                        ctx.Connection.Open();
                        DbCommand cmd = ctx.Connection.CreateCommand();
                        cmd.CommandText = $"INSERT OrderLists (Price, Quantity, OrderId, ProductId) VALUES(@Price, @Quantity, @OrderId, @ProductId)";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(SQLUteis.GetSqlParameter("@Price", SqlDbType.Decimal, 0));
                        cmd.Parameters.Add(SQLUteis.GetSqlParameter("@Quantity", SqlDbType.Decimal, 1));
                        cmd.Parameters.Add(SQLUteis.GetSqlParameter("@OrderId", SqlDbType.Int, saveOrder));
                        cmd.Parameters.Add(SQLUteis.GetSqlParameter("@ProductId", SqlDbType.Int, gifts.Id));
                        cmd.ExecuteNonQuery();
                    }

                    await new ProductRepository().UpdateStockProduct(context, gifts.Id, 1);
                }
            }
        }
    }
}
