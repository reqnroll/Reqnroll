@NUnit3
#TODO: port to System Tests
@ignore
Feature: GH1052

Wrong scenario context injected when running tests in parallel using NUnit


Scenario: GH1052

    Given the following binding class
        """
        [assembly: NUnit.Framework.Parallelizable(NUnit.Framework.ParallelScope.Fixtures)]
        [assembly: NUnit.Framework.LevelOfParallelism(2)]

        """
    And there is a feature file in the project as
         """
         Feature: Parallel Test
             In order to know my illustrate a bug
             As a user of reqnroll
             I need to execute these scenarios

         Scenario Outline: The first scenario example
             Given I'm illustrating the issue

         Examples:
            | Title | 
            | A     | 
            | B     | 
            | C     | 
            | D     | 
            | E     | 
            | F     | 
            | G     | 
            | H     | 
            | I     | 
            | J     | 
            | K     | 
            | L     | 
            | M     | 
            | N     | 
            | O     | 
            | P     | 
            | Q     | 
            | R     | 
            | S     | 


         Scenario Outline: The second scenario example
             Given I'm illustrating the issue

         Examples:
            | Title | 
            | A     | 
            | B     | 
            | C     | 
            | D     | 
            | E     | 
            | F     | 
            | G     | 
            | H     | 
            | I     | 
            | J     | 
            | K     | 
            | L     | 
            | M     | 
            | N     | 
            | O     | 
            | P     | 
            | Q     | 
            | R     | 
            | S     | 
         
         
         """

    And the following binding class
        """
        using System;
        using System.Threading;
        using Reqnroll;
        using Reqnroll.Infrastructure;

        [Binding]
        public class LosingTheWillToLiveSteps
        {
            private readonly ScenarioContext _scenarioContext;
            private readonly IReqnrollOutputHelper _reqnrollOutputHelper;

            public LosingTheWillToLiveSteps(ScenarioContext scenarioContext, IReqnrollOutputHelper reqnrollOutputHelper)
            {
                _scenarioContext = scenarioContext;
                _reqnrollOutputHelper = reqnrollOutputHelper;
            }

            [BeforeScenario]
            public void BeforeScenario()
            {
                Console.Out.WriteLine("BeforeScenario");
                try
                {
                    var id = Guid.NewGuid().ToString();
                    _scenarioContext.Add("ID", id);
                    WriteId();
                }
                catch (Exception e)
                {
                    _reqnrollOutputHelper.WriteLine($"Error adding id {e.Message}");
                }
            }

            [AfterScenario]
            public void AfterScenario()
            {
                try
                {
                    WriteId();
                }
                catch (Exception e)
                {
                    _reqnrollOutputHelper.WriteLine($"Error clearing up the Scenario {e.Message}");
                }
            }

            [Given(@"I'm illustrating the issue")]
            public void GivenImIllustratingTheIssue()
            {
                WriteId();
            }

            private void WriteId()
            {
                try
                {
                    _reqnrollOutputHelper.WriteLine($"Context ID {_scenarioContext["ID"].ToString()}");
                    _reqnrollOutputHelper.WriteLine($"ManagedThreadId {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    _reqnrollOutputHelper.WriteLine($"Error- {e.Message}");
                }
            }

        }
         
        """
    
    When I execute the tests
    Then every scenario has it's individual context id
    And the execution summary should contain
         | Total | Succeeded | Failed |
         | 38    | 38        | 0      |