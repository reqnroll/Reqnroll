using System;
using System.Collections.Generic;
using Reqnroll;
using Reqnroll.Assist;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow.Assist;

public static class TableHelperExtensionMethods
{
    public static T CreateInstance<T>(this Table table)
    {
        return Reqnroll.TableExtensionMethods.CreateInstance<T>(table);
    }

    public static T CreateInstance<T>(this Table table, InstanceCreationOptions creationOptions)
    {
        return Reqnroll.TableExtensionMethods.CreateInstance<T>(table, creationOptions);
    }

    public static T CreateInstance<T>(this Table table, Func<T> methodToCreateTheInstance)
    {
        return Reqnroll.TableExtensionMethods.CreateInstance(table, methodToCreateTheInstance);
    }

    public static T CreateInstance<T>(this Table table, Func<T> methodToCreateTheInstance, InstanceCreationOptions creationOptions)
    {
        return Reqnroll.TableExtensionMethods.CreateInstance(table, methodToCreateTheInstance, creationOptions);
    }

    public static void FillInstance(this Table table, object instance)
    {
        Reqnroll.TableExtensionMethods.FillInstance(table, instance);
    }

    public static void FillInstance(this Table table, object instance, InstanceCreationOptions creationOptions)
    {
        Reqnroll.TableExtensionMethods.FillInstance(table, instance, creationOptions);
    }

    public static IEnumerable<T> CreateSet<T>(this Table table)
    {
        return Reqnroll.TableExtensionMethods.CreateSet<T>(table);
    }

    public static IEnumerable<T> CreateSet<T>(this Table table, InstanceCreationOptions creationOptions)
    {
        return Reqnroll.TableExtensionMethods.CreateSet<T>(table, creationOptions);
    }

    public static IEnumerable<T> CreateSet<T>(this Table table, Func<T> methodToCreateEachInstance)
    {
        return Reqnroll.TableExtensionMethods.CreateSet<T>(table, methodToCreateEachInstance);
    }

    public static IEnumerable<T> CreateSet<T>(this Table table, Func<T> methodToCreateEachInstance, InstanceCreationOptions creationOptions)
    {
        return Reqnroll.TableExtensionMethods.CreateSet<T>(table, methodToCreateEachInstance, creationOptions);
    }

    public static IEnumerable<T> CreateSet<T>(this Table table, Func<DataTableRow, T> methodToCreateEachInstance)
    {
        return Reqnroll.TableExtensionMethods.CreateSet<T>(table, methodToCreateEachInstance);
    }

    public static IEnumerable<T> CreateSet<T>(this Table table, Func<DataTableRow, T> methodToCreateEachInstance, InstanceCreationOptions creationOptions)
    {
        return Reqnroll.TableExtensionMethods.CreateSet<T>(table, methodToCreateEachInstance, creationOptions);
    }
}