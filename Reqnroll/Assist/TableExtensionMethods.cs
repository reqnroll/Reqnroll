using System;
using System.Collections.Generic;
using Reqnroll.Assist;

// ReSharper disable once CheckNamespace
namespace Reqnroll
{
    public static class TableExtensionMethods
    {
        [Obsolete("Use TableHelpers instead")]
        public static T CreateInstance<T>(this Table table)
        {
            return CreateInstance<T>(table, (InstanceCreationOptions)null);
        }

        [Obsolete("Use TableHelpers instead")]
        public static T CreateInstance<T>(this Table table, InstanceCreationOptions creationOptions)
        {
            var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));
            return TEHelpers.ThisTypeHasADefaultConstructor<T>()
                       ? TEHelpers.CreateTheInstanceWithTheDefaultConstructor<T>(Service.Instance, instanceTable, creationOptions)
                       : TEHelpers.CreateTheInstanceWithTheValuesFromTheTable<T>(Service.Instance, instanceTable, creationOptions);
        }

        [Obsolete("Use TableHelpers instead")]
        public static T CreateInstance<T>(this Table table, Func<T> methodToCreateTheInstance)
        {
            return CreateInstance(table, methodToCreateTheInstance, null);
        }

        [Obsolete("Use TableHelpers instead")]
        public static T CreateInstance<T>(this Table table, Func<T> methodToCreateTheInstance, InstanceCreationOptions creationOptions)
        {
            var instance = methodToCreateTheInstance();
            table.FillInstance(instance, creationOptions);
            return instance;
        }

        [Obsolete("Use TableHelpers instead")]
        public static void FillInstance(this Table table, object instance)
        {
            FillInstance(table, instance, null);
        }

        [Obsolete("Use TableHelpers instead")]
        public static void FillInstance(this Table table, object instance, InstanceCreationOptions creationOptions)
        {
            var instanceTable = TEHelpers.GetTheProperInstanceTable(table, instance.GetType());
            TEHelpers.LoadInstanceWithKeyValuePairs(Service.Instance, instanceTable, instance, creationOptions);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> CreateSet<T>(this Table table)
        {
            return CreateSet<T>(table, (InstanceCreationOptions)null);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> CreateSet<T>(this Table table, InstanceCreationOptions creationOptions)
        {
            int count = table.Rows.Count;

            var list = new List<T>(count);


            var pivotTable = new PivotTable(table);
            for (var index = 0; index < count; index++)
            {
                var instance = pivotTable.GetInstanceTable(index).CreateInstance<T>(creationOptions);
                list.Add(instance);
            }

            return list;
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> CreateSet<T>(this Table table, Func<T> methodToCreateEachInstance)
        {
            return CreateSet(table, methodToCreateEachInstance, null);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> CreateSet<T>(this Table table, Func<T> methodToCreateEachInstance, InstanceCreationOptions creationOptions)
        {
            int count = table.Rows.Count;
            var list = new List<T>(count);

            var pivotTable = new PivotTable(table);
            for (var index = 0; index < count; index++)
            {
                var instance = methodToCreateEachInstance();
                pivotTable.GetInstanceTable(index).FillInstance(instance, creationOptions);
                list.Add(instance);
            }

            return list;
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> CreateSet<T>(this Table table, Func<DataTableRow, T> methodToCreateEachInstance)
        {
            return CreateSet(table, methodToCreateEachInstance, null);
        }

        [Obsolete("Use TableHelpers instead")]
        public static IEnumerable<T> CreateSet<T>(this Table table, Func<DataTableRow, T> methodToCreateEachInstance, InstanceCreationOptions creationOptions)
        {
            int count = table.Rows.Count;
            var list = new List<T>(count);

            var pivotTable = new PivotTable(table);
            for (var index = 0; index < count; index++)
            {
                var row = table.Rows[index];
                var instance = methodToCreateEachInstance(row);
                pivotTable.GetInstanceTable(index).FillInstance(instance, creationOptions);
                list.Add(instance);
            }

            return list;
        }
    }
}
