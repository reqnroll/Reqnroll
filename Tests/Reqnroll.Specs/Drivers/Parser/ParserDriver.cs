using FluentAssertions;
using Gherkin;
using Gherkin.Ast;
using Reqnroll.Parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Reqnroll.Specs.Drivers.Parser
{
    public class ParserDriver
    {
        readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver(),
        };

        /// <summary>
        /// We don't want to decorate Gherkin objects with System.Text.Json Attributes, that's why poloymophy is handled manually in this class
        /// </summary>
        sealed class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
        {
            readonly Dictionary<Type, Type[]> _Inheritance = new()
            {
                 { typeof(IHasLocation), [typeof(Tag), typeof(Comment), typeof(ReqnrollFeature), typeof(Background), typeof(Scenario), typeof(ScenarioOutline), typeof(Examples), typeof(ReqnrollStep), typeof(TableCell)] },
                 { typeof(StepsContainer), [typeof(Background), typeof(Scenario), typeof(ScenarioOutline)] },
                 { typeof(Step), [typeof(ReqnrollStep)] },
                 { typeof(Feature), [typeof(ReqnrollFeature)] },
                 { typeof(StepArgument), [typeof(Gherkin.Ast.DataTable), typeof(DocString)] },
            };
            public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

                if (!_Inheritance.TryGetValue(type, out var derivedTypes))
                    return jsonTypeInfo;

                var polymorphismOptions = new JsonPolymorphismOptions
                {
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                foreach (var derivedType in derivedTypes)
                    polymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));

                jsonTypeInfo.PolymorphismOptions = polymorphismOptions;

                return jsonTypeInfo;
            }
        }

        private readonly ReqnrollGherkinParser _parser = new ReqnrollGherkinParser(new CultureInfo("en-US"));
        public string FileContent { get; set; }
        public ReqnrollDocument ParsedDocument { get; private set; }
        public ParserException[] ParsingErrors { get; private set; }

        public void ParseFile()
        {
            var contentReader = new StringReader(FileContent);
            ParsedDocument = null;
            ParsingErrors = new ParserException[0];

            try
            {
                ParsedDocument = _parser.Parse(contentReader, new ReqnrollDocumentLocation("sample.feature"));
                ParsedDocument.Should().NotBeNull();                
            }
            catch (ParserException ex)
            {
                ParsingErrors = ex.GetParserExceptions();
                Console.WriteLine("-> parsing errors");
                foreach (var error in ParsingErrors)
                {
                    Console.WriteLine("-> {0}:{1} {2}", error.Location?.Line ?? 0, error.Location?.Column ?? 0, error.Message);
                }
            }
        }

        public void AssertParsedFeatureEqualTo(string expected)
        {
            static string Normalize(string value)
                => value.Replace("\r", "").Replace(@"\r", "");

            string got = SerializeDocument(ParsedDocument);
            got = Normalize(got);

            var expectedNormalized = Normalize(expected);

            got.Should().Be(expectedNormalized);
        }

        public void AssertErrors(List<ExpectedError> expectedErrors)
        {
            expectedErrors.Should().NotBeEmpty("please specify expected errors");
            
            ParsingErrors.Should().NotBeEmpty("The parsing was successful");
            

            foreach (var expectedError in expectedErrors)
            {
                string message = expectedError.Error.ToLower();

                var errorDetail =
                    ParsingErrors.FirstOrDefault(ed => ed.Location != null && ed.Location?.Line == expectedError.Line &&
                        ed.Message.ToLower().Contains(message));

                errorDetail.Should().NotBeNull("no such error: {0}", message);
            }
        }

        public void SaveSerializedFeatureTo(string fileName)
        {
            ParsedDocument.Should().NotBeNull("The parsing was not successful");
            SerializeDocument(ParsedDocument, fileName);
        }

        private void SerializeDocument(ReqnrollDocument feature, string fileName)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                writer.Write(SerializeDocument(feature));
            }
        }

        private string SerializeDocument(ReqnrollDocument feature)
        {
            return JsonSerializer.Serialize(feature, _serializerOptions);
        }
    }

    public class ExpectedError
    {
        public int? Line { get; set; }
        public string Error { get; set; }
    }
}
