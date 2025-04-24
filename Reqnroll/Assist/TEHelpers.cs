using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reqnroll.Assist.Attributes;
using Reqnroll.Tracing;

namespace Reqnroll.Assist
{
    internal static class TEHelpers
    {
        private static readonly Regex invalidPropertyNameRegex = new Regex(InvalidPropertyNamePattern, RegexOptions.Compiled);
        private const string InvalidPropertyNamePattern = @"[^\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Nd}_]";

        internal static T CreateInstanceAndInitializeWithValuesFromTheTable<T>(Table table, InstanceCreationOptions creationOptions)
        {
            var instance = (T)Activator.CreateInstance(typeof(T));
            LoadInstanceWithKeyValuePairs(table, instance, creationOptions);
            return instance;
        }

        internal static T ConstructInstanceWithValuesFromTheTable<T>(Table table, InstanceCreationOptions creationOptions)
        {
            creationOptions ??= new InstanceCreationOptions();
            creationOptions.AssumeInstanceIsAlreadyCreated = false;

            var (memberHandlers, constructorInfo) = GetMembersThatNeedToBeSet(table, typeof(T), creationOptions);

            VerifyAllColumn(table, creationOptions, memberHandlers.Select(m => m.MemberName));

            var parameters = constructorInfo.GetParameters();
            var parameterValues = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; ++i)
            {
                var parameter = parameters[i];
                var matchingHandler = memberHandlers.FirstOrDefault(memberHandler => memberHandler.MatchesParameter(parameter));
                parameterValues[i] = matchingHandler?.GetValue() ?? (parameter.HasDefaultValue ? parameter.DefaultValue : null);
                
                memberHandlers.Remove(matchingHandler);
            }

            var instance = (T)constructorInfo.Invoke(parameterValues);
            memberHandlers.ForEach(x => x.Setter(instance, x.GetValue()));
            
            return instance;
        }

        internal static bool ThisTypeHasADefaultConstructor<T>()
        {
            return typeof(T).GetConstructors().Any(c => c.GetParameters().Length == 0);
        }

        internal static bool IsMemberMatchingToColumnName(MemberInfo member, string columnName)
        {
            return member.Name.MatchesThisColumnName(columnName)
                || IsMatchingAlias(member, columnName);
        }

        private static bool MatchesThisColumnName(this string propertyName, string columnName)
        {
            var normalizedColumnName = NormalizePropertyNameToMatchAgainstAColumnName(RemoveAllCharactersThatAreNotValidInAPropertyName(columnName));
            var normalizedPropertyName = NormalizePropertyNameToMatchAgainstAColumnName(propertyName);

            return normalizedPropertyName.Equals(normalizedColumnName, StringComparison.OrdinalIgnoreCase);
        }

        private static string RemoveAllCharactersThatAreNotValidInAPropertyName(string name)
        {
            //Unicode groups allowed: Lu, Ll, Lt, Lm, Lo, Nl or Nd see https://msdn.microsoft.com/en-us/library/aa664670%28v=vs.71%29.aspx
            return invalidPropertyNameRegex.Replace(name, string.Empty);
        }

        private static string NormalizePropertyNameToMatchAgainstAColumnName(string name)
        {
            // we remove underscores, because they should be equivalent to spaces that were removed too from the column names
            // we also ignore accents
            return name.Replace("_", string.Empty).ToIdentifier();
        }

        internal static void LoadInstanceWithKeyValuePairs(Table table, object instance, InstanceCreationOptions creationOptions)
        {
            creationOptions ??= new InstanceCreationOptions();
            creationOptions.AssumeInstanceIsAlreadyCreated = true;

            var (memberHandlers, _) = GetMembersThatNeedToBeSet(table, instance.GetType(), creationOptions);
            var memberNames = memberHandlers.Select(h => h.MemberName);

            VerifyAllColumn(table, creationOptions, memberNames);

            memberHandlers.ForEach(x => x.Setter(instance, x.GetValue()));
        }

        private static void VerifyAllColumn(Table table, InstanceCreationOptions creationOptions, IEnumerable<string> memberNames)
        {
            if (creationOptions?.VerifyAllColumnsBound == true)
            {
                var comparer = creationOptions.VerifyCaseInsensitive ? StringComparer.OrdinalIgnoreCase : null;
                var memberNameKeys = new HashSet<string>(memberNames, comparer);
                var allIds = table.Rows.Select(r => r.Id()).ToList();
                var missing = allIds.Where(m => !memberNameKeys.Contains(m)).ToList();
                if (missing.Any())
                {
                    throw new ColumnCouldNotBeBoundException(missing);
                }
            }
        }

        private static (List<MemberHandler>, ConstructorInfo) GetMembersThatNeedToBeSet(Table table, Type type, InstanceCreationOptions creationOptions)
        {
            if (creationOptions is null)
            {
                throw new ArgumentNullException(nameof(creationOptions));
            }

            var properties = (from property in type.GetProperties()
                              from row in table.Rows
                              where TheseTypesMatch(type, property.PropertyType, row)
                                    && IsMemberMatchingToColumnName(property, row.Id())
                              select new MemberHandler { Type = type, Row = row, MemberName = property.Name, PropertyType = property.PropertyType, Setter = (i, v) => property.SetValue(i, v, null) }).ToList();

            var fieldInfos = type.GetFields();
            var fields = (from field in fieldInfos
                          from row in table.Rows
                          where TheseTypesMatch(type, field.FieldType, row)
                                && IsMemberMatchingToColumnName(field, row.Id())
                          select new MemberHandler { Type = type, Row = row, MemberName = field.Name, PropertyType = field.FieldType, Setter = (i, v) => field.SetValue(i, v) }).ToList();

            var memberHandlers = new List<MemberHandler>(properties.Capacity + fields.Count);

            memberHandlers.AddRange(properties);
            memberHandlers.AddRange(fields);

            // tuple special case
            if (IsValueTupleType(type))
            {
                if (fieldInfos.Length > 7)
                {
                    throw new Exception("You should just map to tuple with small objects, types with more than 7 properties are not currently supported");
                }

                if (fieldInfos.Length == table.RowCount)
                {
                    for (var index = 0; index < table.Rows.Count; index++)
                    {
                        var field = fieldInfos[index];
                        var row = table.Rows[index];

                        if (TheseTypesMatch(type, field.FieldType, row))
                        {
                            memberHandlers.Add(new MemberHandler
                            {
                                Type = type,
                                Row = row,
                                MemberName = field.Name,
                                PropertyType = field.FieldType,
                                Setter = (i, v) => field.SetValue(i, v)
                            });
                        }
                    }
                }
            }

            if (creationOptions.AssumeInstanceIsAlreadyCreated)
            {
                return (memberHandlers, null);
            }

            var constructors = type.GetConstructors()
                                   .Select(
                                       c => new
                                       {
                                           Constructor = c,
                                           Parameters = c.GetParameters()
                                       })
                                   .Where(i => i.Parameters.Length > 0);

            if (!creationOptions.RequireTableToProvideAllConstructorParameters)
            {
                // Prefer constructor with the least parameters that takes all members
                var candidateConstructors = constructors.OrderBy(c => c.Parameters.Length);
                foreach (var candidate in candidateConstructors)
                {
                    var resolvedMembers = memberHandlers.Where(m => m.AnyParameterMatchesThisMemberHandler(candidate.Parameters)).ToList();
                    if (resolvedMembers.Count == memberHandlers.Count)
                    {
                        return (memberHandlers, candidate.Constructor);
                    }
                }
            }
            else
            {
                // Prefer constructor with the most parameters
                var candidateConstructors = constructors
                                            .OrderByDescending(i => i.Parameters.Length)
                                            .ToList();

                foreach (var candidate in candidateConstructors)
                {
                    var unresolvedParameters = candidate.Parameters.Where(p => p.NoMemberHandlerMatchesThisParameter(memberHandlers)).ToList();
                    if (unresolvedParameters.Count == 0)
                    {
                        return (memberHandlers, candidate.Constructor);
                    }

                    var matchingHandlers = (from parameter in unresolvedParameters
                                            from row in table.Rows
                                            where parameter.Name.MatchesThisColumnName(row.Id()) && TheseTypesMatch(type, parameter.ParameterType, row)
                                            select new MemberHandler
                                            {
                                                Type = type,
                                                Row = row,
                                                MemberName = parameter.Name,
                                                PropertyType = parameter.ParameterType,
                                                Setter = (_, _) => throw new InvalidOperationException($"This {nameof(MemberHandler)} is used for a constructor parameter only")
                                            }).ToList();

                    if (matchingHandlers.Count == unresolvedParameters.Count)
                    {
                        // We found the correct constructor candidate
                        memberHandlers.AddRange(matchingHandlers);
                        return (memberHandlers, candidate.Constructor);
                    }
                }
            }

            throw new MissingMethodException($"Unable to find a suitable constructor to create instance of {type}");
        }

        private static bool MatchesParameter(this MemberHandler memberHandler, ParameterInfo parameter)
        {
            return memberHandler.MemberName.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)
                   && memberHandler.PropertyType == parameter.ParameterType;
        }

        private static bool AnyParameterMatchesThisMemberHandler(this MemberHandler memberHandler, ParameterInfo[] parameters)
        {
            return parameters.Any(memberHandler.MatchesParameter);
        }

        private static bool NoMemberHandlerMatchesThisParameter(this ParameterInfo parameter, List<MemberHandler> memberHandlers)
        {
            return !memberHandlers.Any(m => m.MatchesParameter(parameter));
        }

        private static bool IsMatchingAlias(MemberInfo field, string id)
        {
            var aliases = field.GetCustomAttributes().OfType<TableAliasesAttribute>();
            return aliases.Any(a => a.Aliases.Any(al => Regex.Match(id, al).Success));
        }

        private static bool TheseTypesMatch(Type targetType, Type memberType, DataTableRow row)
        {
            return Service.Instance.GetValueRetrieverFor(row, targetType, memberType) != null;
        }

        internal class MemberHandler
        {
            public DataTableRow Row { get; set; }
            public string MemberName { get; set; }
            public Action<object, object> Setter { get; set; }
            public Type Type { get; set; }
            public Type PropertyType { get; set; }

            public object GetValue()
            {
                var valueRetriever = Service.Instance.GetValueRetrieverFor(Row, Type, PropertyType);
                if (valueRetriever is null)
                {
                    throw new InvalidOperationException($"Unable to resolve value retriever for member {MemberName}");
                }
                
                return valueRetriever.Retrieve(new KeyValuePair<string, string>(Row[0], Row[1]), Type, PropertyType);
            }
        }

        internal static Table GetTheProperInstanceTable(Table table, Type type)
        {
            return ThisIsAVerticalTable(table, type)
                ? table
                : FlipThisHorizontalTableToAVerticalTable(table);
        }

        private static Table FlipThisHorizontalTableToAVerticalTable(Table table)
        {
            return new PivotTable(table).GetInstanceTable(0);
        }

        private static bool ThisIsAVerticalTable(Table table, Type type)
        {
            if (TheHeaderIsTheOldFieldValuePair(table))
                return true;
            return (table.Rows.Count() != 1) || (table.Header.Count == 2 && TheFirstRowValueIsTheNameOfAProperty(table, type));
        }

        private static bool TheHeaderIsTheOldFieldValuePair(Table table)
        {
            return table.Header.Count == 2 && table.Header.First() == "Field" && table.Header.Last() == "Value";
        }

        private static bool TheFirstRowValueIsTheNameOfAProperty(Table table, Type type)
        {
            var firstRowValue = table.Rows[0][table.Header.First()];
            return type.GetProperties()
                       .Any(property => IsMemberMatchingToColumnName(property, firstRowValue));
        }

        private static bool IsValueTupleType(Type type, bool checkBaseTypes = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(Tuple))
                return true;

            while (type != null)
            {
                if (type.IsGenericType)
                {
                    var genType = type.GetGenericTypeDefinition();
                    if
                    (
                        genType == typeof(ValueTuple)
                        || genType == typeof(ValueTuple<>)
                        || genType == typeof(ValueTuple<,>)
                        || genType == typeof(ValueTuple<,,>)
                        || genType == typeof(ValueTuple<,,,>)
                        || genType == typeof(ValueTuple<,,,,>)
                        || genType == typeof(ValueTuple<,,,,,>)
                        || genType == typeof(ValueTuple<,,,,,,>)
                        || genType == typeof(ValueTuple<,,,,,,,>)
                    )
                        return true;
                }

                if (!checkBaseTypes)
                    break;

                type = type.BaseType;
            }

            return false;
        }
    }
}
