using Microsoft.EntityFrameworkCore;

namespace stock_cli
{
  class MyContext : DbContext
  {
    public MyContext(DbContextOptions<MyContext> options)
        : base(options)
    { }

    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockHistory> StockHistories { get; set; }

  }
}
