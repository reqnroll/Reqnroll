using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Utils;
using Reqnroll.Generator.CodeDom;

namespace Reqnroll.Generator.UnitTestProvider
{
    public class MsTestV2GeneratorProvider : MsTestGeneratorProvider
    {
        protected internal const string DONOTPARALLELIZE_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute";
        protected internal const string DONOTPARALLELIZE_TAG = "MsTest:donotparallelize";
        protected internal const string CATEGORY_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute";
        protected internal const string OWNER_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.OwnerAttribute";
        protected internal const string PRIORITY_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.PriorityAttribute";
        protected internal const string WORKITEM_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute";
        protected internal const string DEPLOYMENTITEM_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute";
        protected internal const string ROW_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute";
        protected internal const string OWNER_TAG = "owner:";
        protected internal const string PRIORITY_TAG = "priority:";
        protected internal const string WORKITEM_TAG = "workitem:";
        protected internal const string DEPLOYMENTITEM_TAG = "MsTest:deploymentitem:";

        public MsTestV2GeneratorProvider(CodeDomHelper codeDomHelper) : base(codeDomHelper)
        {
        }

        public override UnitTestGeneratorTraits GetTraits()
        {
            return UnitTestGeneratorTraits.RowTests | UnitTestGeneratorTraits.ParallelExecution;
        }

        public override void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            SetTestMethod(generationContext, testMethod, scenarioTitle);
        }

        public override void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            // MsTest doesn't support to ignore a specific test case / DataRow
            if (isIgnored)
            {
                return;
            }

            // MsTest doesn't support categories for a specific test case / DataRow
            var args = arguments.Select(arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

            var tagExpressions = tags.Select(t => (CodeExpression)new CodePrimitiveExpression(t)).ToArray();

            // MsTest v2.* cannot handle string[] as the second argument of the [DataRow] attribute.
            // To compensate this, we include a dummy parameter and argument in this case.
            if (args.Count <= 1)
            {
                args.Add(new CodeAttributeArgument(new CodePrimitiveExpression("")));
                var dummyParameterName = "notUsed6248";
                if (testMethod.Parameters.OfType<CodeParameterDeclarationExpression>().All(p => p.Name != dummyParameterName))
                    testMethod.Parameters.Insert(testMethod.Parameters.Count - 1, new CodeParameterDeclarationExpression(typeof(string), dummyParameterName));
            }

            args.Add(new CodeAttributeArgument(tagExpressions.Any()
               ? new CodeArrayCreateExpression(typeof(string[]), tagExpressions)
               : new CodePrimitiveExpression(null)));

            // GH867: If config DisableFriendlyNames is true, bypass setting the DisplayName property of the RowTest attribute
            if (!generationContext.DisableFriendlyTestNames)
            {
                // GH193 - Adding a human readable display name for Data Rows
                // This pulls the display name from the TestMethod attribute on the test method
                // adds the list of argument values
                // [TestMethod("friendly name")]
                // [DataRow(argvalue1, argvalue2, DisplayName="friendly name(argvalue1,argvalue2)"]

                // Find the "TestMethod" attribute and retrieve its "DisplayName" property value
                var testMethodAttr = testMethod.CustomAttributes
                    .OfType<CodeAttributeDeclaration>()
                    .FirstOrDefault(attr => attr.AttributeType.BaseType == TEST_ATTR);

                string testMethodDisplayName = null;
                if (testMethodAttr != null && testMethodAttr.Arguments.Count >= 1)
                {
                    var displayNameArg = testMethodAttr.Arguments
                        .OfType<CodeAttributeArgument>()
                        .FirstOrDefault();
                    if (displayNameArg != null && displayNameArg.Value is CodePrimitiveExpression expr && expr.Value is string str)
                    {
                        testMethodDisplayName = str;
                    }
                }
                if (string.IsNullOrEmpty(testMethodDisplayName))
                {
                    testMethodDisplayName = testMethod.Name;
                }
                var displayName = $"{testMethodDisplayName}({string.Join(",", arguments)})";
                var displayNameProp = new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(displayName));
                args.Add(displayNameProp);
            }
            CodeDomHelper.AddAttribute(testMethod, ROW_ATTR, args.ToArray());
        }

        public override void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            if (generationContext.Feature.Tags.Any(t => t.Name.Substring(1).StartsWith(DEPLOYMENTITEM_TAG, StringComparison.InvariantCultureIgnoreCase)))
            {
                CodeDomHelper.AddAttribute(generationContext.TestClass, DEPLOYMENTITEM_ATTR, "Reqnroll.MSTest.ReqnrollPlugin.dll");
            }

            base.SetTestClass(generationContext, featureTitle, featureDescription);
        }

        public override void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            var doNotParallelizeTags = featureCategories.Where(f => f.StartsWith(DONOTPARALLELIZE_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (doNotParallelizeTags.Any())
            {
               generationContext.CustomData[DONOTPARALLELIZE_TAG] = string.Empty;
            }

            generationContext.CustomData["featureCategories"] = GetNonMSTestSpecificTags(featureCategories).ToArray();

            var ownerTags = featureCategories.Where(t => t.StartsWith(OWNER_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (ownerTags.Any())
            {
                generationContext.CustomData[OWNER_TAG] = ownerTags.Select(t => t.Substring(OWNER_TAG.Length).Trim('\"')).FirstOrDefault();
            }

            var priorityTags = featureCategories.Where(t => t.StartsWith(PRIORITY_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (priorityTags.Any())
            {
                var priorityTextValue = priorityTags.Select(t => t.Substring(PRIORITY_TAG.Length).Trim('\"')).FirstOrDefault();
                if (int.TryParse(priorityTextValue, out var priority))
                {
                    generationContext.CustomData[PRIORITY_TAG] =  priority;
                }
            }

            var workItemTags = featureCategories.Where(t => t.StartsWith(WORKITEM_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (workItemTags.Any())
            {
                int temp;
                var workItemsAsStrings = workItemTags.Select(t => t.Substring(WORKITEM_TAG.Length).Trim('\"'));
                if (workItemsAsStrings.Any())
                {
                    generationContext.CustomData[WORKITEM_TAG] = workItemsAsStrings.Where(t => int.TryParse(t, out temp)).Select(t => int.Parse(t));
                }
            }

            var deploymentItemTags = featureCategories.Where(t => t.StartsWith(DEPLOYMENTITEM_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (deploymentItemTags.Any())
            {
                var deploymentItemsAsStrings = deploymentItemTags.Select(t => t.Substring(DEPLOYMENTITEM_TAG.Length));
                if (deploymentItemsAsStrings.Any())
                {
                    generationContext.CustomData[DEPLOYMENTITEM_TAG] = deploymentItemsAsStrings;
                }
            }
        }

        public override void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
        {
            if (generationContext.CustomData.ContainsKey(DONOTPARALLELIZE_TAG))
            {
                CodeDomHelper.AddAttribute(testMethod, DONOTPARALLELIZE_ATTR);
            }

            base.SetTestMethod(generationContext, testMethod, friendlyTestName);
            if (generationContext.CustomData.ContainsKey("featureCategories"))
            {
                var featureCategories = (string[])generationContext.CustomData["featureCategories"];
                CodeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, featureCategories);
            }

            if (generationContext.CustomData.ContainsKey(OWNER_TAG))
            {
                string ownerName = generationContext.CustomData[OWNER_TAG] as string;
                if (!string.IsNullOrEmpty(ownerName))
                {
                    CodeDomHelper.AddAttribute(testMethod, OWNER_ATTR, ownerName);
                }
            }

            if (generationContext.CustomData.ContainsKey(PRIORITY_TAG))
            {
                var priority = (int)generationContext.CustomData[PRIORITY_TAG];
                CodeDomHelper.AddAttribute(testMethod, PRIORITY_ATTR, priority);
            }

            if (generationContext.CustomData.ContainsKey(WORKITEM_TAG))
            {
                var workItems = generationContext.CustomData[WORKITEM_TAG] as IEnumerable<int>;
                foreach (int workItem in workItems)
                {
                    CodeDomHelper.AddAttribute(testMethod, WORKITEM_ATTR, workItem);
                }
            }

            if (generationContext.CustomData.ContainsKey(DEPLOYMENTITEM_TAG))
            {
                var deploymentItems = generationContext.CustomData[DEPLOYMENTITEM_TAG] as IEnumerable<string>;
                foreach (string deploymentItem in deploymentItems)
                {
                    var deploymentItemParts = deploymentItem.Split(':');
                    var itemPath = FileSystemHelper.NormalizeDirectorySeparators(deploymentItemParts[0]);
                    var outputDir = deploymentItemParts.Length > 1 ? deploymentItemParts[1] : null;
                    if (outputDir != null)
                    {
                        CodeDomHelper.AddAttribute(testMethod, DEPLOYMENTITEM_ATTR, itemPath, outputDir);
                    }
                    else
                    {
                        CodeDomHelper.AddAttribute(testMethod, DEPLOYMENTITEM_ATTR, itemPath);
                    }
                }
            }
        }

        public override void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            var scenarioCategoriesArray = scenarioCategories.ToArray();
            var doNotParallelizeTags = scenarioCategoriesArray.Where(s => s.StartsWith(DONOTPARALLELIZE_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);

            if (doNotParallelizeTags.Any() && !generationContext.CustomData.ContainsKey(DONOTPARALLELIZE_TAG))
            {
                CodeDomHelper.AddAttribute(testMethod, DONOTPARALLELIZE_ATTR);
            }

            if (doNotParallelizeTags.Any() && generationContext.CustomData.ContainsKey(DONOTPARALLELIZE_TAG))
            {
                scenarioCategoriesArray = scenarioCategoriesArray.Where(s => !s.StartsWith(DONOTPARALLELIZE_TAG)).ToArray();
            }

            var ownerTags = scenarioCategoriesArray.Where(t => t.StartsWith(OWNER_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (ownerTags.Any())
            {
                string ownerName = ownerTags.Select(t => t.Substring(OWNER_TAG.Length).Trim('\"')).FirstOrDefault();
                if (!string.IsNullOrEmpty(ownerName))
                {
                    CodeDomHelper.AddAttribute(testMethod, OWNER_ATTR, ownerName);
                }
            }

            var priorityTags = scenarioCategoriesArray.Where(t => t.StartsWith(PRIORITY_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (priorityTags.Any())
            {
                var priorityTextValue = priorityTags.Select(t => t.Substring(PRIORITY_TAG.Length).Trim('\"')).FirstOrDefault();
                if (int.TryParse(priorityTextValue, out var priority))
                {
                    CodeDomHelper.AddAttribute(testMethod, PRIORITY_ATTR, priority);
                }
            }

            var workItemTags = scenarioCategoriesArray.Where(t => t.StartsWith(WORKITEM_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (workItemTags.Any())
            {
                var workItemsAsStrings = workItemTags.Select(t => t.Substring(WORKITEM_TAG.Length).Trim('\"'));
                var workItems = workItemsAsStrings.Where(t => int.TryParse(t, out _)).Select(int.Parse);
                foreach (int workItem in workItems)
                {
                    CodeDomHelper.AddAttribute(testMethod, WORKITEM_ATTR, workItem);
                }
            }

            CodeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, GetNonMSTestSpecificTags(scenarioCategoriesArray));
        }

        public override void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, DONOTPARALLELIZE_ATTR);
        }

        public override void SetTestMethodNonParallelizable(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            CodeDomHelper.AddAttribute(testMethod, DONOTPARALLELIZE_ATTR);
        }

        private IEnumerable<string> GetNonMSTestSpecificTags(IEnumerable<string> tags)
        {
            return tags == null ? new string[0] : tags.Where(t => !t.StartsWith(OWNER_TAG, StringComparison.InvariantCultureIgnoreCase))
                                                      .Where(t => !t.StartsWith(PRIORITY_TAG, StringComparison.InvariantCultureIgnoreCase))
                                                      .Where(t => !t.StartsWith(WORKITEM_TAG, StringComparison.InvariantCultureIgnoreCase))
                                                      .Where(t => !t.StartsWith(DEPLOYMENTITEM_TAG, StringComparison.InvariantCultureIgnoreCase))
                                                      .Select(t => t);
        }
    }
}
