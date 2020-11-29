using System;
using System.Collections.Generic;

namespace stock_cli
{
  public class StockHistory
  {
    public Guid Id { get; set; }
    public string Rate { get; set; }
    public string Year { get; set; }
    public string Ratio { get; set; }
    public string NetRatio { get; set; }
    public string ReportYearMonth { get; set; }
    public DateTime Date { get; set; }

    public string StockId { get; set; }
    public virtual Stock Stock { get; set; }

    public StockHistory() { }
    public StockHistory(StockData data, DateTime date)
    {
      Id = Guid.NewGuid();
      Rate = data.rate;
      Year = data.year;
      Ratio = data.ratio;
      NetRatio = data.netRatio;
      ReportYearMonth = data.reportSeason;
      Date = date;
      StockId = data.id;
    }

    public override string ToString()
    {
      return $"證券代號={StockId}, 證券名稱={Stock.Name}, 殖利率(%)={Rate}, 股利年度={Year}, 本益比={Ratio}, 股價淨值比={NetRatio}, 財報年/季={ReportYearMonth}, 日期={Date.ToString("yyyy/MM/dd")}";
    }

  }
}
