using Gherkin.Ast;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Reqnroll.Generator;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.Specs.Generator.ReqnrollPlugin
{
    internal class MultiFeatureGenerator : IFeatureGenerator
    {
        private readonly IFeatureGenerator _defaultFeatureGenerator;
        private readonly KeyValuePair<Combination, IFeatureGenerator>[] _featureGenerators;
        private readonly List<string> _unitTestProviderTags = new List<string> { "xunit", "mstest", "nunit3" };

        public MultiFeatureGenerator(IEnumerable<KeyValuePair<Combination, IFeatureGenerator>> featureGenerators, IFeatureGenerator defaultFeatureGenerator)
        {
            _defaultFeatureGenerator = defaultFeatureGenerator;
            _featureGenerators = featureGenerators.ToArray();

            foreach (var featureGenerator in _featureGenerators)
            {
                if (featureGenerator.Value is UnitTestFeatureGenerator unitTestFeatureGenerator)
                {
                    unitTestFeatureGenerator.TestClassNameFormat += $"_{featureGenerator.Key.UnitTestProvider}_{featureGenerator.Key.TargetFramework}_{featureGenerator.Key.ProjectFormat}_{featureGenerator.Key.ProgrammingLanguage}";
                }
            }
        }

        public CodeNamespace GenerateUnitTestFixture(ReqnrollDocument reqnrollDocument, string testClassName, string targetNamespace)
        {
            CodeNamespace result = null;
            bool onlyFullframework = false;

            var reqnrollFeature = reqnrollDocument.ReqnrollFeature;
            bool onlyDotNetCore = false;
            if (reqnrollFeature.HasTags())
            {
                if (reqnrollFeature.Tags.Where(t => t.Name == "@SingleTestConfiguration").Any())
                {
                    return _defaultFeatureGenerator.GenerateUnitTestFixture(reqnrollDocument, testClassName, targetNamespace);
                }

                onlyFullframework = HasFeatureTag(reqnrollFeature, "@fullframework");
                onlyDotNetCore = HasFeatureTag(reqnrollFeature, "@dotnetcore");
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                onlyFullframework = false;
                onlyDotNetCore = true;
            }

            var tagsOfFeature = reqnrollFeature.Tags.Select(t => t.Name);
            var unitTestProviders = tagsOfFeature.Where(t => _unitTestProviderTags.Where(utpt => string.Compare(t, "@" + utpt, StringComparison.CurrentCultureIgnoreCase) == 0).Any());

            foreach (var featureGenerator in GetFilteredFeatureGenerator(unitTestProviders, onlyFullframework, onlyDotNetCore))
            {
                var clonedDocument = CloneDocumentAndAddTags(reqnrollDocument, featureGenerator.Key);


                var featureGeneratorResult = featureGenerator.Value.GenerateUnitTestFixture(clonedDocument, testClassName, targetNamespace);

                if (result == null)
                {
                    result = featureGeneratorResult;
                }
                else
                {
                    foreach (CodeTypeDeclaration type in featureGeneratorResult.Types)
                    {
                        result.Types.Add(type);
                    }
                }
            }

            if (result == null)
            {
                result = new CodeNamespace(targetNamespace);
            }

            return result;
        }

        private ReqnrollDocument CloneDocumentAndAddTags(ReqnrollDocument reqnrollDocument, Combination combination)
        {
            var tags = new List<Tag>();
            var reqnrollFeature = reqnrollDocument.ReqnrollFeature;
            tags.AddRange(reqnrollFeature.Tags);
            if (!HasFeatureTag(reqnrollDocument.ReqnrollFeature, "@" + combination.UnitTestProvider))
                tags.Add(new Tag(null, "@" + combination.UnitTestProvider));
            foreach (string otherUnitTestProvider in _unitTestProviderTags.Where(utp => !utp.Equals(combination.UnitTestProvider, StringComparison.InvariantCultureIgnoreCase)))
            {
                tags.RemoveAll(t => ("@" + otherUnitTestProvider).Equals(t.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            if (!HasFeatureTag(reqnrollDocument.ReqnrollFeature, "@" + combination.TargetFramework))
                tags.Add(new Tag(null, "@" + combination.TargetFramework));
            var feature = new ReqnrollFeature(tags.ToArray(),
                                              reqnrollFeature.Location,
                                              reqnrollFeature.Language,
                                              reqnrollFeature.Keyword,
                                              reqnrollFeature.Name,
                                              reqnrollFeature.Description,
                                              reqnrollFeature.Children.ToArray());

            return new ReqnrollDocument(feature, reqnrollDocument.Comments.ToArray(), reqnrollDocument.DocumentLocation);
        }

        private IEnumerable<KeyValuePair<Combination, IFeatureGenerator>> GetFilteredFeatureGenerator(IEnumerable<string> unitTestProviders, bool onlyFullframework, bool onlyDotNetCore)
        {
            if (!unitTestProviders.Any())
            {
                foreach (var featureGenerator in _featureGenerators)
                {
                    if (onlyFullframework)
                    {
                        if (featureGenerator.Key.TargetFramework == TestRunCombinations.TfmEnumValuenet462)
                        {
                            yield return featureGenerator;
                        }
                    }
                    else
                    {
                        if (onlyDotNetCore)
                        {
                            if (ShouldCompileForNetCore21(featureGenerator.Key)
                                || ShouldCompileForNetCore31(featureGenerator.Key) || ShouldCompileForNet50(featureGenerator.Key) || ShouldCompileForNet60(featureGenerator.Key))
                            {
                                yield return featureGenerator;
                            }
                        }
                        else
                        {
                            yield return featureGenerator;
                        }
                    }
                }
            }

            foreach (string unitTestProvider in unitTestProviders)
            {
                foreach (var featureGenerator in _featureGenerators)
                {
                    if (IsForUnitTestProvider(featureGenerator, unitTestProvider))
                    {
                        if (onlyFullframework)
                        {
                            if (featureGenerator.Key.TargetFramework == TestRunCombinations.TfmEnumValuenet462)
                            {
                                yield return featureGenerator;
                            }
                        }
                        else
                        {
                            if (onlyDotNetCore)
                            {
                                if (ShouldCompileForNetCore21(featureGenerator.Key)
                                    || ShouldCompileForNetCore31(featureGenerator.Key) || ShouldCompileForNet50(featureGenerator.Key) || ShouldCompileForNet60(featureGenerator.Key))
                                { 
                                    yield return featureGenerator;
                                }
                            }
                            else
                            {
                                yield return featureGenerator;
                            }
                        }
                    }
                }
            }
        }
        public bool ShouldCompileForNetCore21(Combination combination)
        {
            return combination.TargetFramework == TestRunCombinations.TfmEnumValueNetCore21
                   && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public bool ShouldCompileForNetCore31(Combination combination)
        {
            return combination.TargetFramework == TestRunCombinations.TfmEnumValueNetCore31;
        }

        public bool ShouldCompileForNet50(Combination combination)
        {
            return combination.TargetFramework == TestRunCombinations.TfmEnumValueNet50;
        }

        public bool ShouldCompileForNet60(Combination combination)
        {
            return combination.TargetFramework == TestRunCombinations.TfmEnumValueNet60;
        }

        private bool IsForUnitTestProvider(KeyValuePair<Combination, IFeatureGenerator> featureGenerator, string unitTestProvider)
        {
            return string.Compare("@" + featureGenerator.Key.UnitTestProvider, unitTestProvider, StringComparison.CurrentCultureIgnoreCase) == 0;
        }

        private bool HasFeatureTag(ReqnrollFeature reqnrollFeature, string tag)
        {
            return reqnrollFeature.Tags.Any(t => string.Compare(t.Name, tag, StringComparison.CurrentCultureIgnoreCase) == 0);
        }
    }
}
