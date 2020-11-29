using System.Collections.Generic;

namespace stock_cli
{
  public class Stock
  {
    public string Id { get; set; }
    public string Name { get; set; }

    public virtual List<StockHistory> Histories { get; set; }
  }
}
