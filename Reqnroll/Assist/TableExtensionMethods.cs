using System;
using System.Collections.Generic;
using Reqnroll.Assist;

// ReSharper disable once CheckNamespace
namespace Reqnroll
{
    public static class TableExtensionMethods
    {
        private static TableHelpers DefaultTableHelpers => new TableHelpers(Service.Instance);

        public static T CreateInstance<T>(this Table table)
        {
            return DefaultTableHelpers.CreateInstance<T>(table);
        }

        public static T CreateInstance<T>(this Table table, InstanceCreationOptions creationOptions)
        {
            return DefaultTableHelpers.CreateInstance<T>(table, creationOptions);
        }

        public static T CreateInstance<T>(this Table table, Func<T> methodToCreateTheInstance)
        {
            return DefaultTableHelpers.CreateInstance(table, methodToCreateTheInstance);
        }

        public static T CreateInstance<T>(this Table table, Func<T> methodToCreateTheInstance, InstanceCreationOptions creationOptions)
        {
            return DefaultTableHelpers.CreateInstance(table, methodToCreateTheInstance, creationOptions);
        }

        public static void FillInstance(this Table table, object instance)
        {
            DefaultTableHelpers.FillInstance(table, instance);
        }

        public static void FillInstance(this Table table, object instance, InstanceCreationOptions creationOptions)
        {
            DefaultTableHelpers.FillInstance(table, instance, creationOptions);
        }

        public static IEnumerable<T> CreateSet<T>(this Table table)
        {
            return DefaultTableHelpers.CreateSet<T>(table);
        }

        public static IEnumerable<T> CreateSet<T>(this Table table, InstanceCreationOptions creationOptions)
        {
            return DefaultTableHelpers.CreateSet<T>(table, creationOptions);
        }

        public static IEnumerable<T> CreateSet<T>(this Table table, Func<T> methodToCreateEachInstance)
        {
            return DefaultTableHelpers.CreateSet(table, methodToCreateEachInstance);
        }

        public static IEnumerable<T> CreateSet<T>(this Table table, Func<T> methodToCreateEachInstance, InstanceCreationOptions creationOptions)
        {
            return DefaultTableHelpers.CreateSet(table, methodToCreateEachInstance, creationOptions);
        }

        public static IEnumerable<T> CreateSet<T>(this Table table, Func<DataTableRow, T> methodToCreateEachInstance)
        {
            return DefaultTableHelpers.CreateSet(table, methodToCreateEachInstance);
        }

        public static IEnumerable<T> CreateSet<T>(this Table table, Func<DataTableRow, T> methodToCreateEachInstance, InstanceCreationOptions creationOptions)
        {
            return DefaultTableHelpers.CreateSet(table, methodToCreateEachInstance, creationOptions);
        }
    }
}
