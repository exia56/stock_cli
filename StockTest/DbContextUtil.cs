using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using System.Linq;
using Moq;
using System;

namespace StockTest
{
  internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
  {
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
      _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
      return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
      return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
      Console.WriteLine(expression);
      return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
      return Execute<TResult>(expression);
    }
  }

  internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
  {
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetEnumerator()
    {
      return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider
    {
      get { return new TestAsyncQueryProvider<T>(this); }
    }
  }

  internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
  {
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
      _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
      return new ValueTask(new Task(() => _inner.Dispose()));
    }

    public T Current
    {
      get
      {
        return _inner.Current;
      }
    }

    public ValueTask<bool> MoveNextAsync()
    {
      return new ValueTask<bool>(Task.FromResult(_inner.MoveNext()));
    }
  }

  public static class DbContextMock
  {
    public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
    {
      var queryable = sourceList.AsQueryable();
      var dbSet = new Mock<DbSet<T>>();
      dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
      dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
      dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
      dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
      dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
      return dbSet.Object;
    }
  }
}
