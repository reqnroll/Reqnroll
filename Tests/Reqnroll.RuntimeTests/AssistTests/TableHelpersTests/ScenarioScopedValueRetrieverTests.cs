using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll.Assist;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;
using Xunit;

namespace Reqnroll.RuntimeTests.AssistTests.TableHelpersTests;

public class ScenarioScopedValueRetrieverTests : IDisposable
{
    public class CustomId
    {
        public string Value { get; set; }

        public CustomId(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
    }

    public class EntityWithCustomId
    {
        public CustomId Id { get; set; }
        public string Name { get; set; }
        public string Extra { get; set; }
    }

    public class CustomIdValueRetriever : IValueRetriever
    {
        private readonly string _prefix;

        public CustomIdValueRetriever(string prefix = "")
        {
            _prefix = prefix;
        }

        public bool CanRetrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return propertyType == typeof(CustomId);
        }

        public object Retrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return new CustomId(_prefix + keyValuePair.Value);
        }
    }

    public class CustomIdValueComparer : IValueComparer
    {
        public bool CanCompare(object actualValue)
        {
            return actualValue is CustomId;
        }

        public bool Compare(string expectedValue, object actualValue)
        {
            return actualValue is CustomId cid && cid.Value == expectedValue;
        }
    }

    public class SecondaryCustomType
    {
        public int Code { get; set; }
    }

    public class EntityWithMultipleCustomTypes
    {
        public CustomId Id { get; set; }
        public SecondaryCustomType Secondary { get; set; }
    }

    public class SecondaryValueRetriever : IValueRetriever
    {
        public bool CanRetrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return propertyType == typeof(SecondaryCustomType);
        }

        public object Retrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return new SecondaryCustomType { Code = int.Parse(keyValuePair.Value) * 10 };
        }
    }

    public void Dispose()
    {
        Service.Instance.RestoreDefaults();
    }

    private static IObjectContainer CreateScenarioContainer()
    {
        var container = new ObjectContainer();
        new DefaultDependencyProvider().RegisterScenarioContainerDefaults(container);
        return container;
    }

    [Fact]
    public void TableHelpers_and_Service_are_resolvable_from_scenario_container()
    {
        var container = CreateScenarioContainer();

        var tableHelpers = container.Resolve<TableHelpers>();
        var service = container.Resolve<Service>();

        tableHelpers.Should().NotBeNull();
        service.Should().NotBeNull();
    }

    [Fact]
    public void Scenario_registered_value_retriever_is_used_by_TableHelpers_CreateInstance()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("prefix-"));

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "123");
        table.AddRow("Name", "Test");

        var entity = tableHelpers.CreateInstance<EntityWithCustomId>(table);

        entity.Should().NotBeNull();
        entity.Id.Value.Should().Be("prefix-123");
        entity.Name.Should().Be("Test");
    }

    [Fact]
    public void Scenario_registered_value_retriever_is_used_by_TableHelpers_CreateSet()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("set-"));

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Id", "Name");
        table.AddRow("1", "First");
        table.AddRow("2", "Second");

        var set = tableHelpers.CreateSet<EntityWithCustomId>(table).ToList();

        set.Should().HaveCount(2);
        set[0].Id.Value.Should().Be("set-1");
        set[0].Name.Should().Be("First");
        set[1].Id.Value.Should().Be("set-2");
        set[1].Name.Should().Be("Second");
    }

    [Fact]
    public void Scenario_registered_value_retriever_is_used_by_TableHelpers_FillInstance()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("fill-"));

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "999");
        table.AddRow("Name", "Existing");

        var entity = new EntityWithCustomId();
        tableHelpers.FillInstance(table, entity);

        entity.Id.Value.Should().Be("fill-999");
        entity.Name.Should().Be("Existing");
    }

    [Fact]
    public void Scenario_registered_value_comparer_is_used_by_TableHelpers_CompareToInstance()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueComparer>(new CustomIdValueComparer());

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "expected-id");
        table.AddRow("Name", "Test");

        var entity = new EntityWithCustomId
        {
            Id = new CustomId("expected-id"),
            Name = "Test"
        };

        var act = () => tableHelpers.CompareToInstance(table, entity);
        act.Should().NotThrow();

        var nonMatchingEntity = new EntityWithCustomId
        {
            Id = new CustomId("different-id"),
            Name = "Test"
        };
        var actMismatch = () => tableHelpers.CompareToInstance(table, nonMatchingEntity);
        actMismatch.Should().Throw<ComparisonException>();
    }

    [Fact]
    public void Scenario_registered_value_comparer_is_used_by_TableHelpers_CompareToSet()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueComparer>(new CustomIdValueComparer());

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Id", "Name");
        table.AddRow("id-1", "First");
        table.AddRow("id-2", "Second");

        var set = new List<EntityWithCustomId>
        {
            new() { Id = new CustomId("id-1"), Name = "First" },
            new() { Id = new CustomId("id-2"), Name = "Second" }
        };

        var act = () => tableHelpers.CompareToSet(table, set);
        act.Should().NotThrow();
    }

    [Fact]
    public void Multiple_scenario_value_retrievers_can_be_registered_with_names()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("multi-"), "customId");
        container.RegisterInstanceAs<IValueRetriever>(new SecondaryValueRetriever(), "secondary");

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "42");
        table.AddRow("Secondary", "5");

        var entity = tableHelpers.CreateInstance<EntityWithMultipleCustomTypes>(table);

        entity.Id.Value.Should().Be("multi-42");
        entity.Secondary.Code.Should().Be(50);
    }

    [Fact]
    public void Scenarios_are_isolated_and_do_not_contaminate_each_other_or_global_state()
    {
        var containerA = CreateScenarioContainer();
        containerA.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("A-"));

        var containerB = CreateScenarioContainer();
        containerB.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("B-"));

        var containerC = CreateScenarioContainer(); // No custom retriever registered

        var tableHelpersA = containerA.Resolve<TableHelpers>();
        var tableHelpersB = containerB.Resolve<TableHelpers>();
        var tableHelpersC = containerC.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "100");

        var entityA = tableHelpersA.CreateInstance<EntityWithCustomId>(table);
        var entityB = tableHelpersB.CreateInstance<EntityWithCustomId>(table);

        entityA.Id.Value.Should().Be("A-100");
        entityB.Id.Value.Should().Be("B-100");

        // Container C has no retriever for CustomId, so it should not be able to retrieve it (Id will be null)
        var entityC = tableHelpersC.CreateInstance<EntityWithCustomId>(table);
        entityC.Id.Should().BeNull();

        // Global Service.Instance is untouched
        var defaultEntity = table.CreateInstance<EntityWithCustomId>();
        defaultEntity.Id.Should().BeNull();
    }

    [Fact]
    public void Scenario_inherits_global_registrations_from_Service_Instance()
    {
        Service.Instance.ValueRetrievers.Register(new CustomIdValueRetriever("global-"));

        var container = CreateScenarioContainer();
        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "500");

        var entity = tableHelpers.CreateInstance<EntityWithCustomId>(table);
        entity.Id.Value.Should().Be("global-500");
    }

    [Fact]
    public void Scenario_retriever_overrides_global_registration_for_that_scenario_only()
    {
        Service.Instance.ValueRetrievers.Register(new CustomIdValueRetriever("global-"));

        var containerOverride = CreateScenarioContainer();
        containerOverride.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("override-"));

        var containerInherit = CreateScenarioContainer();

        var tableHelpersOverride = containerOverride.Resolve<TableHelpers>();
        var tableHelpersInherit = containerInherit.Resolve<TableHelpers>();

        var table = new Table("Field", "Value");
        table.AddRow("Id", "500");

        var entityOverride = tableHelpersOverride.CreateInstance<EntityWithCustomId>(table);
        var entityInherit = tableHelpersInherit.CreateInstance<EntityWithCustomId>(table);

        entityOverride.Id.Value.Should().Be("override-500");
        entityInherit.Id.Value.Should().Be("global-500");
    }

    [Fact]
    public void TableHelpers_methods_work_with_custom_retrievers_and_delegates()
    {
        var container = CreateScenarioContainer();
        container.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever("delegate-"));
        container.RegisterInstanceAs<IValueComparer>(new CustomIdValueComparer());

        var tableHelpers = container.Resolve<TableHelpers>();

        var table = new Table("Id", "Name");
        table.AddRow("1", "Alice");
        table.AddRow("2", "Bob");

        // CreateSet with Func<T>
        var set1 = tableHelpers.CreateSet(table, () => new EntityWithCustomId()).ToList();
        set1.Should().HaveCount(2);
        set1[0].Id.Value.Should().Be("delegate-1");
        set1[0].Name.Should().Be("Alice");

        // CreateSet with Func<DataTableRow, T>
        var set2 = tableHelpers.CreateSet(table, row => new EntityWithCustomId { Extra = row["Name"].ToUpperInvariant() }).ToList();
        set2.Should().HaveCount(2);
        set2[0].Id.Value.Should().Be("delegate-1");
        set2[0].Name.Should().Be("Alice");
        set2[0].Extra.Should().Be("ALICE");

        // FindInSet
        var searchTable = new Table("Id", "Name");
        searchTable.AddRow("delegate-1", "Alice");
        var found = tableHelpers.FindInSet(searchTable, set1);
        found.Should().NotBeNull();
        found.Name.Should().Be("Alice");

        // FindAllInSet
        var allFound = tableHelpers.FindAllInSet(searchTable, set1).ToList();
        allFound.Should().HaveCount(1);

        // CompareToSet
        var compareTable = new Table("Id", "Name");
        compareTable.AddRow("delegate-1", "Alice");
        compareTable.AddRow("delegate-2", "Bob");
        var actCompare = () => tableHelpers.CompareToSet(compareTable, set1);
        actCompare.Should().NotThrow();
    }

    [Fact]
    public async Task Concurrent_scenarios_remain_isolated_in_parallel()
    {
        var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() =>
        {
            var container = CreateScenarioContainer();
            var prefix = $"thread-{i}:";
            container.RegisterInstanceAs<IValueRetriever>(new CustomIdValueRetriever(prefix));

            var tableHelpers = container.Resolve<TableHelpers>();

            var table = new Table("Field", "Value");
            table.AddRow("Id", $"item-{i}");
            table.AddRow("Name", $"Name-{i}");

            var entity = tableHelpers.CreateInstance<EntityWithCustomId>(table);

            entity.Id.Value.Should().Be($"{prefix}item-{i}");
            entity.Name.Should().Be($"Name-{i}");
        }));

        await Task.WhenAll(tasks);
    }
}
