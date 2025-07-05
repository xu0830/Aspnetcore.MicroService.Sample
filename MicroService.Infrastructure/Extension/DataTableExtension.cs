using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Extension
{
    public static class DataTableExtension
    {
        public static List<T> ToList<T>(this DataTable dataTable)
        {
            List<T> list = new();

            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            foreach (DataRow row in dataTable.Rows)
            {
                var obj = (T)Activator.CreateInstance(typeof(T));
                foreach (var columnName in columnNames)
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(columnName);
                    if (propertyInfo != null && row[columnName] != DBNull.Value)
                    {
                        propertyInfo.SetValue(obj, row[columnName]);
                    }
                }
                list.Add(obj);
            }

            return list;
        }

    }
}
