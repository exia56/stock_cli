using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace stock_cli
{
  public class StockService
  {
    private readonly IStockRepo stockRepo;
    private readonly MyContext dbContext;

    public StockService(MyContext dbContext, IStockRepo stockRepo)
    {
      this.stockRepo = stockRepo;
      this.dbContext = dbContext;
    }

    private IQueryable<StockHistory> SearchById(IQueryable<StockHistory> query, string id)
    {
      return query.Where((s) => s.StockId == id);
    }

    private IQueryable<StockHistory> SearchByDate(IQueryable<StockHistory> query, DateTime date)
    {
      return query.Where((s) => s.Date == date);
    }

    private IQueryable<StockHistory> SearchByDateBetween(IQueryable<StockHistory> query, DateTime startDate, DateTime endDate)
    {
      return query.Where((s) => s.Date >= startDate && s.Date <= endDate);
    }

    public async Task Load(DateTime date)
    {
      if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
      {
        throw new Exception($"not support weekend loading");
      }
      var stock = await stockRepo.getStockList(date);
      var tasks = stock.data?.Select(async (data) =>
       {
         var exists = await dbContext.Stocks.AnyAsync((s) => s.Id == data.id);
         if (!exists)
         {
           await dbContext.Stocks.AddAsync(new Stock { Id = data.id, Name = data.name });
         }
         await dbContext.StockHistories.AddAsync(new StockHistory(data, date));
         await dbContext.SaveChangesAsync();
       });
      Task.WaitAll(tasks.ToArray(), -1);
    }

    public async Task<List<StockHistory>> QueryById(string id)
    {
      return await SearchById(dbContext.StockHistories, id).ToListAsync();
    }

    public async Task<List<StockHistory>> QueryByDate(DateTime date)
    {
      return await SearchByDate(dbContext.StockHistories, date).ToListAsync();
    }

    public async Task<List<StockHistory>> QueryByDateBetween(DateTime startDate, DateTime endDate)
    {
      return await SearchByDateBetween(dbContext.StockHistories, startDate, endDate).ToListAsync();
    }

    public async Task<List<StockHistory>> QueryByIdAndDate(string id, DateTime date)
    {
      var query = SearchById(dbContext.StockHistories, id);
      return await SearchByDate(query, date).ToListAsync();
    }

    public async Task<List<StockHistory>> QueryByIdAndDateBetween(string id, DateTime startDate, DateTime endDate)
    {
      var query = SearchById(dbContext.StockHistories, id);
      return await SearchByDateBetween(query, startDate, endDate).ToListAsync();
    }
  }
}
