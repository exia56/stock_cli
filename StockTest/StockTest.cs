using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using stock_cli;

namespace StockTest
{
  [TestClass]
  public class StockServiceTest
  {
    static List<Stock> stockStore;
    static List<StockHistory> stockHistoryStore;
    static Mock<IStockRepo> mockStockRepo;
    static MyContext dbContext;
    static StockService stockService;
    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
      stockStore = new List<Stock>();
      stockHistoryStore = new List<StockHistory>();
      mockStockRepo = new Mock<IStockRepo>();
      dbContext = new MyContext(new DbContextOptionsBuilder<MyContext>().UseInMemoryDatabase(databaseName: "test").Options);
      stockService = new StockService(dbContext, mockStockRepo.Object);
    }

    [TestCategory("Load")]
    [TestMethod]
    public async Task TestLoadWeekendData()
    {
      var testDate = new DateTime(2020, 11, 14);
      await Assert.ThrowsExceptionAsync<Exception>(async () => await stockService.Load(testDate), "not support weekend loading");
    }

    [TestCategory("Load")]
    [TestMethod]
    public async Task TestBasicLoad()
    {
      var testDate = new DateTime(2020, 11, 11);
      mockStockRepo.Setup(repo => repo.getStockList(testDate)).ReturnsAsync(new StockResponse<StockData>
      {
        stat = "OK",
        selectType = "ALL",
        date = "20201111",
        data = Enumerable.Range(1, 9).Select(idx => new StockData
        {
          id = $"110{idx}",
          name = $"test-stock-{idx}",
          year = "2020",
          rate = $"0.{idx}",
          ratio = $"0.{idx}",
          netRatio = $"0.{idx}",
          reportSeason = "109/9",
        }).ToList(),
      });

      await stockService.Load(testDate);

      Assert.IsTrue(dbContext.Stocks.Count() == 9, "stock count should be 9");
      Assert.IsTrue(dbContext.StockHistories.Count() == 9, "stock history count should be 9");

      mockStockRepo.VerifyAll();
      mockStockRepo.Reset();
    }

    [TestCategory("Load")]
    [TestMethod]
    public async Task TestLoadSecond()
    {
      var testDate = new DateTime(2020, 10, 12);
      mockStockRepo.Setup(repo => repo.getStockList(testDate)).ReturnsAsync(new StockResponse<StockData>
      {
        stat = "OK",
        selectType = "ALL",
        date = "20201111",
        data = Enumerable.Range(1, 9).Select(idx => new StockData
        {
          id = $"110{idx}",
          name = $"test-stock-{idx}",
          year = "2020",
          rate = $"0.{idx}",
          ratio = $"0.{idx}",
          netRatio = $"0.{idx}",
          reportSeason = "109/9",
        }).ToList(),
      });

      await stockService.Load(testDate);

      Assert.IsTrue(dbContext.Stocks.Count() == 9, "stock count should be 9");
      Assert.IsTrue(dbContext.StockHistories.Count() == 18, "stock history count should be 18");

      mockStockRepo.VerifyAll();
      mockStockRepo.Reset();
    }

    [TestCategory("QueryById")]
    [TestMethod]
    public async Task TestQueryById()
    {
      var id = "1102";
      var result = await stockService.QueryById(id);

      Assert.IsTrue(result.Count() == 2, $"id of {id} count should be 2");
      Assert.IsTrue(result.All(sh => sh.StockId == id), $"all stock id should be {id}");

    }

    [TestCategory("QueryByDate")]
    [TestMethod]
    public async Task TestQueryByDate()
    {
      var testDate = new DateTime(2020, 11, 11);
      var result = await stockService.QueryByDate(testDate);

      Assert.IsTrue(result.Count() == 9, $"result count should be 9");
      Assert.IsTrue(result.All(sh => sh.Date == testDate), $"all result's date should be {testDate}");
    }

    [TestCategory("QueryByDateBetween")]
    [TestMethod]
    public async Task TestQueryByDateBetween()
    {
      var startDate = new DateTime(2020, 11, 01);
      var endDate = new DateTime(2020, 12, 01);
      var result = await stockService.QueryByDateBetween(startDate, endDate);

      Assert.IsTrue(result.Count() == 9, $"result count should be 9");
      Assert.IsTrue(result.All(sh => sh.Date >= startDate && sh.Date <= endDate), $"all result's date should be between {startDate} and {endDate}");
    }

    [TestCategory("QueryByIdAndDate")]
    [TestMethod]
    public async Task TestQueryByIdAndDate()
    {
      var id = "1102";
      var testDate = new DateTime(2020, 11, 11);
      var result = await stockService.QueryByIdAndDate(id, testDate);

      Assert.IsTrue(result.Count() == 1, $"result count should be 1");
      Assert.IsTrue(result.All(sh => sh.Date == testDate), $"all result's date should be {testDate}");
    }

    [TestCategory("QueryByIdAndDateBetween")]
    [TestMethod]
    public async Task TestQueryByIdAndDateBetween()
    {
      var id = "1102";
      var startDate = new DateTime(2020, 10, 01);
      var endDate = new DateTime(2020, 12, 01);
      var result = await stockService.QueryByIdAndDateBetween(id, startDate, endDate);

      Assert.IsTrue(result.Count() == 2, $"result count should be 2");
      Assert.IsTrue(result.All(sh => sh.Date >= startDate && sh.Date <= endDate), $"all result's date should be between {startDate} and {endDate}");
    }
  }
}
