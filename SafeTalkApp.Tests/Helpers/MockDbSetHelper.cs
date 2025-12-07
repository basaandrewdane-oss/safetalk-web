using Moq;
using System.Data.Entity;

namespace SafeTalkApp.Tests.Helpers
{
    public static class MockDbSetHelper
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(IQueryable<T> data, Func<object[], T>? find = null) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            var dataList = data.ToList();
            var queryable = dataList.AsQueryable();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            // ✅ Support for Add
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(t =>
            {
                dataList.Add(t);
                queryable = dataList.AsQueryable(); // ✅ rebuild
            });


            // ✅ Support for Remove (optional)
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(t =>
            {
                dataList.Remove(t);
                queryable = dataList.AsQueryable(); // ✅ rebuild
            });

            if (find != null)
            {
                mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns(find);
            }
            else
            {
                // Auto-detect a primary key property (NameID or ID)
                var type = typeof(T);

                var keyProp =
                    type.GetProperties().FirstOrDefault(p =>
                        p.Name.Equals($"{type.Name}ID", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase)
                    );

                if (keyProp != null)
                {
                    mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns((object[] keys) =>
                    {
                        var id = keys.First();
                        return dataList.FirstOrDefault(x =>
                            keyProp.GetValue(x)?.Equals(id) == true);
                    });
                }
            }
            return mockSet;
        }
    }
}
