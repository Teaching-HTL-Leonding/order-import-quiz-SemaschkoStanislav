using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

Console.WriteLine("Hello World!");

var factory = new OrderImportContextFactory();
using var context = factory.CreateDbContext(args);

switch (args[0])
{
    case "import":

        IEnumerable<string> customers = await File.ReadAllLinesAsync(args[1]);
        IEnumerable<string> orders = await File.ReadAllLinesAsync(args[2]);
        customers = customers.Skip(1);
        orders = orders.Skip(1);
        var splittedLines = customers.Select(l => l.Split('\t'));
        foreach (var item in splittedLines)
        {
            Customer customer = new Customer { Name = item[0], CreditLimit = Convert.ToDecimal(item[1])};
            context.Customers.Add(customer);
        }
        splittedLines = orders.Select(l => l.Split('\t'));
        foreach (var item in splittedLines)
        {
            //Order order = new Order { OrderDate = Convert.ToDateTime(item[1]), OrderValue = Convert.ToDecimal(item[2])};
            //context.Orders.Add(order);
        }
        await context.SaveChangesAsync();
        break;
    case "clean":
        break;
    case "check":
        break;
    case "full":
        break;
    default:
        break;
}

class Customer
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(8,2)")]
    public decimal CreditLimit { get; set; }

    public List<Order> Orders { get; set; } = new();
}

class Order
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal OrderValue { get; set; }

    public Customer? Customer { get; set; }

    public int CustomerId { get; set; }
}

class OrderImportContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public OrderImportContext(DbContextOptions<OrderImportContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : base(options)
    { }

}

class OrderImportContextFactory : IDesignTimeDbContextFactory<OrderImportContext>
{
    public OrderImportContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<OrderImportContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new OrderImportContext(optionsBuilder.Options);
    }
}