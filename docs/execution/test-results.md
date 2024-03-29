# Test Results

When Reqnroll tests are executed, the execution engine processes the test steps, executing the necessary test logic and either finishing successfully or failing for various reasons.  

## Test Passes

While executing the tests, the engine outputs information about the execution to the test output. In some cases it makes sense to investigate the test output even if the test passes.  

By default, the test output includes the executed test steps, the invoked test logic methods ([bindings](../automation/bindings)) and the execution time for longer operations. You can [configure](../installation/configuration) the information displayed in the test output.

## Test Fails due to an Error

A test can fail because it causes an error. The test output contains more detailed information, e.g. a stack trace.

## Test Fails due to step binding problems

A test can fail if the test logic (bindings) have not yet been implemented (or are configured improperly). By default, this is reported as an "inconclusive" result, although you can [configure](../installation/configuration) how Reqnroll behaves in this case.

```{note}
Some unit test frameworks do not support inconclusive result. In this case the problem is reported as an error instead.
```

The test output can be very useful if you are missing bindings, as it contain a step binding method skeleton you can copy to your project and extend with the test logic.

{#ignored-tests}
## Ignored Tests

Just like with normal unit tests, you can also ignore Reqnroll tests. To do so, tag the feature or scenario with the `@ignore` tag. 

```{danger}
Don't forget that ignoring a test will not solve any problems with your implementation...
```

