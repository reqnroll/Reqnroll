using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Reqnroll.Bindings.Discovery;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.Discovery
{
    public class ReqnrollAttributesFilterTests
    {
        public static IEnumerable<object[]> GetAllReqnrollAttributes()
        {
            var reqnrollAssembly = typeof(BindingAttribute).Assembly;
            var parameterlessAttributes = GetAllParameterlessReqnrollAttributes(reqnrollAssembly);
            var attributesWithTags = GetAllReqnrollAttributesWithRequiredTags(reqnrollAssembly);
            var attributes = parameterlessAttributes.Concat(attributesWithTags);

            return from attribute in attributes
                   select new object[] { attribute };
        }

        private static IEnumerable<Attribute> GetAllParameterlessReqnrollAttributes(Assembly reqnrollAssembly)
        {
            var attribute = from type in reqnrollAssembly.GetTypes()
                            where type.IsSubclassOf(typeof(Attribute))
                            where !type.IsAbstract
                            where type.IsPublic
                            where type.IsClass
                            where type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any(c => c.GetParameters().Length == 0)
                            select (Attribute)Activator.CreateInstance(type);
            return attribute;
        }

        private static IEnumerable<Attribute> GetAllReqnrollAttributesWithRequiredTags(Assembly reqnrollAssembly)
        {
            var attribute = from type in reqnrollAssembly.GetTypes()
                            where type.IsSubclassOf(typeof(Attribute))
                            where !type.IsAbstract
                            where type.IsPublic
                            where type.IsClass
                            where type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any(c => c.GetParameters().Length > 0 && c.GetParameters()[0].ParameterType == typeof(string[]))
                            select (Attribute)Activator.CreateInstance(type, new object[] { new string[0] });
            return attribute;
        }

        [Theory]
        [MemberData(nameof(GetAllReqnrollAttributes))]
        public void FilterForReqnrollAttributes_ReqnrollAttributes_ShouldReturnAllReqnrollAttributes(Attribute attribute)
        {
            // ARRANGE
            var reqnrollAttributesFilter = new ReqnrollAttributesFilter();
            var attributesToFilter = new[] { attribute };

            // ACT
            var filteredAttributes = reqnrollAttributesFilter.FilterForReqnrollAttributes(attributesToFilter);

            // ASSERT
            filteredAttributes.Should().BeEquivalentTo(attributesToFilter.AsEnumerable());
        }
    }
}
