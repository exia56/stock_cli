using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Linq;

namespace stock_cli
{
  public struct StockData
  {
    public string id;
    public string name;
    public string rate;
    public string year;
    public string ratio;
    public string netRatio;
    public string reportSeason;
  }

  public class StockResponse<T>
  {
    public List<T> data;
    public List<string> fields;
    public List<string> notes;
    public string date;
    public string selectType;
    public string stat;
    public string title;

  }

  public class StockQuery
  {
    public string date;
    string response = "json";
    string selectType = "ALL";

    public string toQueryString()
    {
      var queryString = $"date={date}&response={response}&selectType={selectType}&_={DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
      return queryString;
    }
  }

  public interface IStockRepo
  {
    Task<StockResponse<StockData>> getStockList(DateTime date);
  }

  public class StockRepo : IStockRepo
  {
    private readonly string urlTemplate = "https://www.twse.com.tw/exchangeReport/BWIBBU_d?{0}";

    public async Task<StockResponse<StockData>> getStockList(DateTime date)
    {
      var parameters = new StockQuery()
      {
        date = date.ToString("yyyyMMdd")
      };
      var url = String.Format(urlTemplate, parameters.toQueryString());
      var request = new HttpRequestMessage(HttpMethod.Get, url);
      var client = HttpClientFactory.Create();
      var response = await client.SendAsync(request);

      var str = await response.Content.ReadAsStringAsync();

      var result = JsonConvert.DeserializeObject<StockResponse<List<object>>>(str);

      return new StockResponse<StockData>
      {
        fields = result.fields,
        notes = result.notes,
        date = result.date,
        selectType = result.selectType,
        stat = result.stat,
        title = result.title,
        data = result.data?.Select((data) => new StockData
        {
          id = data[0] as string,
          name = data[1] as string,
          rate = data[2] as string,
          year = data[3] as string,
          ratio = data[4] as string,
          netRatio = data[5] as string,
          reportSeason = data[6] as string,
        }).ToList(),
      };
    }
  }
}
