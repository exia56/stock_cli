using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace stock_cli
{
  class Program
  {
    static ServiceProvider InitDI()
    {
      return new ServiceCollection()
        .AddTransient<IStockRepo, StockRepo>()
        .AddTransient<StockService>()
        .AddDbContext<MyContext>((option) => option.UseInMemoryDatabase(databaseName: "test"))
        .BuildServiceProvider();
    }

    static void LoadRange(StockService stockService, DateTime baseDate, int range)
    {
      foreach (int idx in Enumerable.Range(0, range))
      {

      }
    }

    static async Task Main(string[] args)
    {
      var serviceProvide = InitDI();
      var stockService = serviceProvide.GetService<StockService>();

      var cacheDays = 1;
      Console.WriteLine($"Cache last {cacheDays} day(s) data default, waiting.....");
      var ranges = Enumerable.Range(1, cacheDays).Select(async (idx) =>
      {
        var date = DateTime.Now.AddDays(-1 * idx);
        await stockService.Load(new DateTime(date.Year, date.Month, date.Day));
      });
      Task.WaitAll(ranges.ToArray(), -1);
      Console.WriteLine("Cache done");

      var command = "";

      var commandLoad = "Load ";
      var commandQueryById = "QueryById ";
      var commandQueryByDate = "QueryByDate ";
      var commandQueryByDateBetween = "QueryByDateBetween ";
      var commandQueryByIdAndDate = "QueryByIdAndDate ";
      var commandQueryByIdAndDateBetween = "QueryByIdAndDateBetween ";
      do
      {
        try
        {
          Console.WriteLine("");
          Console.WriteLine("=================== Example ======================");
          Console.WriteLine("example: Load <date in format: yyyy/MM/dd>");
          Console.WriteLine("example: QueryById <Id>");
          Console.WriteLine("example: QueryByDate <yyyy/MM/dd>");
          Console.WriteLine("example: QueryByDateBetween <yyyy/MM/dd> <yyyy/MM/dd>");
          Console.WriteLine("example: QueryByIdAndDate <Id> <yyyy/MM/dd>");
          Console.WriteLine("example: QueryByIdAndDateBetween <Id> <yyyy/MM/dd> <yyyy/MM/dd>");
          Console.WriteLine("=================== Example END ===================");
          Console.WriteLine("");
          Console.Write("Command: ");
          command = Console.ReadLine().Trim();
          List<StockHistory> result = null;
          Console.WriteLine("=================== Command Start ===================");
          if (command.StartsWith(commandQueryByIdAndDateBetween))
          {
            var arguments = command.Replace(commandQueryByIdAndDateBetween, "").Trim().Split(" ");
            result = await stockService.QueryByIdAndDateBetween(arguments[0], Convert.ToDateTime(arguments[1]), Convert.ToDateTime(arguments[2]));
          }
          else if (command.StartsWith(commandQueryByDateBetween))
          {
            var dateString = command.Replace(commandQueryByDateBetween, "").Trim().Split(" ");
            result = await stockService.QueryByDateBetween(Convert.ToDateTime(dateString[0]), Convert.ToDateTime(dateString[1]));
          }
          else if (command.StartsWith(commandQueryByIdAndDate))
          {
            var arguments = command.Replace(commandQueryByIdAndDate, "").Trim().Split(" ");
            result = await stockService.QueryByIdAndDate(arguments[0], Convert.ToDateTime(arguments[1]));
          }
          else if (command.StartsWith(commandQueryByDate))
          {
            var dateString = command.Replace(commandQueryByDate, "").Trim();
            result = await stockService.QueryByDate(Convert.ToDateTime(dateString));
          }
          else if (command.StartsWith(commandQueryById))
          {
            var id = command.Replace(commandQueryById, "").Trim();
            result = await stockService.QueryById(id);
          }
          else if (command.StartsWith(commandLoad))
          {
            var dateTemp = command.Replace(commandLoad, "").Trim();
            Console.WriteLine($"Start Loading {dateTemp} stock data...");
            await stockService.Load(Convert.ToDateTime(dateTemp));
            Console.WriteLine("Load Done");
          }
          else
          {
            Console.WriteLine("Command Not Matched");
          }

          if (result != null)
          {
            Console.WriteLine($"Command result:");
            Console.WriteLine(String.Join("\n", result));
          }

          Console.WriteLine("=================== Command END ===================");
        }
        catch (Exception err)
        {
          Console.WriteLine(err);
        }
      } while (command != "exit");
    }
  }
}
