# Formatters Subsystem - Contributor Documentation

This document provides an in-depth overview of the Reqnroll Formatters subsystem, which is responsible for generating Cucumber Messages output (NDJSON files and HTML reports) during test execution.

## Table of Contents

1. [Overview](#overview)
2. [Structural Overview](#structural-overview)
3. [Startup Process](#startup-process)
4. [Execution Events](#execution-events)
5. [Formatter Class Hierarchy](#formatter-class-hierarchy)
6. [Configuration](#configuration)

---

## Overview

The Formatters subsystem implements the [Cucumber Messages Protocol](https://github.com/cucumber/messages), providing a standardized way to output test execution data. This enables integration with various reporting tools that understand the Cucumber Messages format.

Key capabilities:
- **NDJSON Output**: Raw Cucumber Messages in newline-delimited JSON format
- **HTML Reports**: Interactive HTML reports using the Cucumber HTML formatter
- **Extensibility**: Base classes for creating custom formatters

---

## Structural Overview

The Formatters subsystem is organized into several key namespaces:

```
Reqnroll/Formatters/
+-- Configuration/           # Configuration resolution
+-- ExecutionTracking/       # Test execution state tracking
+-- Html/                    # HTML formatter implementation
+-- Message/                 # NDJSON message formatter
+-- PayloadProcessing/       # Cucumber message creation
|   +-- Cucumber/            # Message factory and utilities
+-- PubSub/                  # Publisher/Subscriber infrastructure
+-- RuntimeSupport/          # Runtime support utilities
```

### Core Class Structure

```mermaid
classDiagram
    direction TB
    
    %% Interfaces
    class ICucumberMessageFormatter {
        <<interface>>
        +Name: string
        +LaunchFormatter(broker: ICucumberMessageBroker)
        +PublishAsync(message: Envelope): Task
    }
    
    class IMessagePublisher {
        <<interface>>
        +PublishAsync(message: Envelope): Task
    }
    
    class ICucumberMessageBroker {
        <<interface>>
        +IsEnabled: bool
        +Initialize()
        +FormatterInitialized(formatter, enabled)
    }
    
    class ICucumberMessagePublisher {
        <<interface>>
        +Initialize(runtimePluginEvents)
    }
    
    %% Core Classes
    class CucumberMessageBroker {
        -_registeredFormatters: List~ICucumberMessageFormatter~
        -_activeFormatters: ConcurrentDictionary
        +Initialize()
        +PublishAsync(message): Task
    }
    
    class CucumberMessagePublisher {
        -_broker: ICucumberMessageBroker
        +StartedFeatures: ConcurrentDictionary
        +OnEventAsync(executionEvent): Task
    }
    
    %% Relationships
    IMessagePublisher <|.. ICucumberMessageBroker
    ICucumberMessageBroker <|.. CucumberMessageBroker
    ICucumberMessagePublisher <|.. CucumberMessagePublisher
    CucumberMessagePublisher --> CucumberMessageBroker : publishes to
    CucumberMessageBroker --> ICucumberMessageFormatter : routes to
```

### PubSub Architecture

The Formatters use a publish/subscribe pattern:

```mermaid
flowchart LR
    subgraph TestExecution["Test Execution"]
        Events[Execution Events]
    end
    
    subgraph PubSub["Pub/Sub Layer"]
        Publisher[CucumberMessagePublisher]
        Broker[CucumberMessageBroker]
    end
    
    subgraph Formatters["Formatters"]
        HTML[HtmlFormatter]
        Message[MessageFormatter]
        Custom[Custom Formatters]
    end
    
    Events --> Publisher
    Publisher --> Broker
    Broker --> HTML
    Broker --> Message
    Broker --> Custom
```

---

## Startup Process

The Formatters subsystem is initialized during `ContainerBuilder.CreateGlobalContainer()`. Here's the detailed startup sequence:

### Initialization Sequence Diagram

```mermaid
sequenceDiagram
    participant CB as ContainerBuilder
    participant DDP as DefaultDependencyProvider
    participant FCP as FormattersConfigurationProvider
    participant CMP as CucumberMessagePublisher
    participant CMB as CucumberMessageBroker
    participant BMG as BindingMessagesGenerator
    participant F as Formatters (HTML/Message)
    
    Note over CB: CreateGlobalContainer() called
    
    CB->>DDP: RegisterGlobalContainerDefaults()
    activate DDP
    DDP->>DDP: Register IFormattersConfigurationProvider
    DDP->>DDP: Register ICucumberMessageBroker
    DDP->>DDP: Register ICucumberMessagePublisher
    DDP->>DDP: Register MessageFormatter as "message"
    DDP->>DDP: Register HtmlFormatter as "html"
    DDP->>DDP: Register supporting services
    deactivate DDP
    
    CB->>FCP: Resolve configuration
    FCP-->>CB: Enabled = true/false
    
    alt Formatters Enabled
        CB->>CMP: Initialize(runtimePluginEvents)
        activate CMP
        
        CMP->>CMB: Initialize()
        activate CMB
        
        loop For each registered formatter
            CMB->>F: LaunchFormatter(broker)
            activate F
            F->>F: Check configuration
            F->>F: Open output file stream
            F->>F: Start background task
            F->>CMB: FormatterInitialized(enabled)
            deactivate F
        end
        
        CMB->>CMB: Check all formatters initialized
        deactivate CMB
        
        CMP->>CMP: Subscribe to test thread events
        deactivate CMP
    end
    
    Note over CB: Container ready for test execution
```

### Startup Code Flow

1. **Dependency Registration** (`DefaultDependencyProvider.RegisterGlobalContainerDefaults`):
   ```csharp
   // Formatter configuration
   container.RegisterTypeAs<FormattersConfigurationProvider, IFormattersConfigurationProvider>();
   
   // Formatters (named registrations)
   container.RegisterTypeAs<MessageFormatter, ICucumberMessageFormatter>("message");
   container.RegisterTypeAs<HtmlFormatter, ICucumberMessageFormatter>("html");
   
   // Pub/Sub infrastructure
   container.RegisterTypeAs<CucumberMessageBroker, ICucumberMessageBroker>();
   container.RegisterTypeAs<CucumberMessagePublisher, ICucumberMessagePublisher>();
   ```

2. **Publisher Initialization** (`ContainerBuilder.CreateGlobalContainer`):
   ```csharp
   var cucumberMessageConfiguration = container.Resolve<IFormattersConfigurationProvider>();
   if (cucumberMessageConfiguration.Enabled)
       container.Resolve<ICucumberMessagePublisher>().Initialize(runtimePluginEvents);
   ```

3. **Broker Initialization** (`CucumberMessageBroker.Initialize`):
   - Iterates through all registered formatters
   - Calls `LaunchFormatter()` on each
   - Formatters report back via `FormatterInitialized()`

4. **Formatter Launch** (`FormatterBase.LaunchInner`):
   - Validates configuration
   - Opens output file stream (for file-based formatters)
   - Starts background message consumption task

---

## Execution Events

The Formatters respond to execution events published by the test execution engine. Each event triggers message generation and publishing.

### Event Tracking Architecture

```mermaid
classDiagram
    direction TB
    
    class IFeatureExecutionTracker {
        <<interface>>
        +Enabled: bool
        +FeatureName: string
        +ProcessEvent(various)
        +FinalizeTracking()
    }
    
    class IPickleExecutionTracker {
        <<interface>>
        +PickleId: string
        +TestCaseTracker: TestCaseTracker
        +ProcessEvent(various)
    }
    
    class TestCaseExecutionTracker {
        +AttemptId: int
        +TestCaseStartedId: string
        +ProcessEvent(various)
    }
    
    class TestCaseTracker {
        +TestCaseId: string
        +PickleId: string
        +Steps: List~StepTrackerBase~
    }
    
    class StepTrackerBase {
        <<abstract>>
        +TestStepId: string
    }
    
    class TestStepTracker {
        +PickleStepId: string
        +IsBound: bool
        +StepDefinitionIds: List
    }
    
    class HookStepTracker {
        +HookId: string
    }
    
    class StepExecutionTrackerBase {
        <<abstract>>
        +StepTracker: StepTrackerBase
        +Status: ScenarioExecutionStatus
        +Duration: TimeSpan?
    }
    
    class TestStepExecutionTracker {
        +ProcessEvent(StepStartedEvent)
        +ProcessEvent(StepFinishedEvent)
    }
    
    class HookStepExecutionTracker {
        +ProcessEvent(HookBindingStartedEvent)
        +ProcessEvent(HookBindingFinishedEvent)
    }
    
    IFeatureExecutionTracker <|.. FeatureExecutionTracker
    IPickleExecutionTracker <|.. PickleExecutionTracker
    
    FeatureExecutionTracker "1" --> "*" IPickleExecutionTracker
    IPickleExecutionTracker "1" --> "1" TestCaseTracker
    IPickleExecutionTracker "1" --> "*" TestCaseExecutionTracker
    
    TestCaseTracker "1" --> "*" StepTrackerBase
    StepTrackerBase <|-- TestStepTracker
    StepTrackerBase <|-- HookStepTracker
    
    StepExecutionTrackerBase <|-- TestStepExecutionTracker
    StepExecutionTrackerBase <|-- HookStepExecutionTracker
    
    TestCaseExecutionTracker "1" --> "*" StepExecutionTrackerBase
```

### TestRunStarted Event

Triggered when the test run begins.

```mermaid
sequenceDiagram
    participant TE as TestExecutionEngine
    participant CMP as CucumberMessagePublisher
    participant BMG as BindingMessagesGenerator
    participant CMB as CucumberMessageBroker
    participant F as Formatters
    
    TE->>CMP: OnEventAsync(TestRunStartedEvent)
    activate CMP
    
    CMP->>CMP: Check StartupCompleted flag
    CMP->>CMB: Check IsEnabled
    CMP->>BMG: Check Ready
    
    alt First startup and enabled
        CMP->>CMB: PublishAsync(TestRunStarted)
        CMB->>F: PublishAsync(TestRunStarted)
        
        CMP->>CMB: PublishAsync(Meta)
        CMB->>F: PublishAsync(Meta)
        
        loop For each binding message
            CMP->>CMB: PublishAsync(StepDefinition/Hook/ParameterType)
            CMB->>F: PublishAsync(...)
        end
        
        CMP->>CMP: Set StartupCompleted = true
    end
    
    deactivate CMP
```

**Messages Published:**
- `TestRunStarted` - Marks the beginning of the test run
- `Meta` - Contains environment metadata
- `ParameterType` - For each step argument transformation
- `StepDefinition` - For each step definition binding
- `Hook` - For each hook binding

### FeatureStarted Event

Triggered when a feature begins execution.

```mermaid
sequenceDiagram
    participant TE as TestExecutionEngine
    participant CMP as CucumberMessagePublisher
    participant FET as FeatureExecutionTracker
    participant CMB as CucumberMessageBroker
    participant F as Formatters
    
    TE->>CMP: OnEventAsync(FeatureStartedEvent)
    activate CMP
    
    CMP->>CMP: GetOrAdd FeatureExecutionTracker
    
    alt New feature
        CMP->>FET: Create new tracker
        activate FET
        
        FET->>CMB: PublishAsync(Source)
        CMB->>F: PublishAsync(Source)
        
        FET->>CMB: PublishAsync(GherkinDocument)
        CMB->>F: PublishAsync(GherkinDocument)
        
        loop For each Pickle in feature
            FET->>CMB: PublishAsync(Pickle)
            CMB->>F: PublishAsync(Pickle)
        end
        
        deactivate FET
    end
    
    deactivate CMP
```

**Messages Published:**
- `Source` - The feature file content
- `GherkinDocument` - Parsed feature AST
- `Pickle` - One for each scenario/example combination

### ScenarioStarted Event

Triggered when a scenario begins execution.

```mermaid
sequenceDiagram
    participant TE as TestExecutionEngine
    participant CMP as CucumberMessagePublisher
    participant FET as FeatureExecutionTracker
    participant PET as PickleExecutionTracker
    participant TCET as TestCaseExecutionTracker
    participant CMB as CucumberMessageBroker
    
    TE->>CMP: OnEventAsync(ScenarioStartedEvent)
    activate CMP
    
    CMP->>FET: ProcessEvent(ScenarioStartedEvent)
    activate FET
    
    FET->>FET: Find PickleId from ScenarioInfo
    FET->>PET: GetOrAdd PickleExecutionTracker
    
    FET->>PET: ProcessEvent(ScenarioStartedEvent)
    activate PET
    
    PET->>PET: Increment AttemptCount
    
    alt Retry (AttemptCount > 0)
        PET->>CMB: PublishAsync(TestCaseFinished with willBeRetried=true)
    end
    
    PET->>TCET: Create TestCaseExecutionTracker
    
    TCET->>CMB: PublishAsync(TestCaseStarted)
    
    deactivate PET
    deactivate FET
    deactivate CMP
```

**Messages Published:**
- `TestCaseStarted` - Marks scenario execution start
- (On retry) `TestCaseFinished` with `willBeRetried=true`

### StepStarted Event

Triggered when a step begins execution.

```mermaid
sequenceDiagram
    participant TE as TestExecutionEngine
    participant CMP as CucumberMessagePublisher
    participant FET as FeatureExecutionTracker
    participant PET as PickleExecutionTracker
    participant TCET as TestCaseExecutionTracker
    participant TSET as TestStepExecutionTracker
    participant TCT as TestCaseTracker
    participant CMB as CucumberMessageBroker
    
    TE->>CMP: OnEventAsync(StepStartedEvent)
    activate CMP
    
    CMP->>FET: ProcessEvent(StepStartedEvent)
    FET->>PET: ProcessEvent(StepStartedEvent)
    PET->>TCET: ProcessEvent(StepStartedEvent)
    
    activate TCET
    TCET->>TSET: Create TestStepExecutionTracker
    
    alt First attempt
        TCET->>TCT: ProcessEvent(StepStartedEvent)
        TCT->>TCT: Create TestStepTracker
    end
    
    TSET->>CMB: PublishAsync(TestStepStarted)
    
    deactivate TCET
    deactivate CMP
```

**Messages Published:**
- `TestStepStarted` - Marks step execution start

### HookStarted Event

Triggered when a hook begins execution.

```mermaid
sequenceDiagram
    participant TE as TestExecutionEngine
    participant CMP as CucumberMessagePublisher
    participant FET as FeatureExecutionTracker
    participant PET as PickleExecutionTracker
    participant TCET as TestCaseExecutionTracker
    participant HSET as HookStepExecutionTracker
    participant TCT as TestCaseTracker
    participant CMB as CucumberMessageBroker
    
    TE->>CMP: OnEventAsync(HookBindingStartedEvent)
    activate CMP
    
    alt BeforeTestRun/AfterTestRun/BeforeFeature/AfterFeature
        CMP->>CMP: Create TestRunHookExecutionTracker
        CMP->>CMB: PublishAsync(TestRunHookStarted)
    else Scenario-level hooks
        CMP->>FET: ProcessEvent(HookBindingStartedEvent)
        FET->>PET: ProcessEvent(HookBindingStartedEvent)
        PET->>TCET: ProcessEvent(HookBindingStartedEvent)
        
        activate TCET
        TCET->>HSET: Create HookStepExecutionTracker
        
        alt First attempt
            TCET->>TCT: ProcessEvent(HookBindingStartedEvent)
            TCT->>TCT: Create HookStepTracker
        end
        
        HSET->>CMB: PublishAsync(TestStepStarted)
        deactivate TCET
    end
    
    deactivate CMP
```

**Messages Published:**
- `TestStepStarted` - For hook execution (hooks are modeled as test steps)

---

## Formatter Class Hierarchy

The Formatters follow a well-defined class hierarchy that provides increasing levels of functionality:

```mermaid
classDiagram
    direction TB
    
    class ICucumberMessageFormatter {
        <<interface>>
        +Name: string
        +LaunchFormatter(broker)
        +PublishAsync(message): Task
    }
    
    class FormatterBase {
        <<abstract>>
        #PostedMessages: Channel~Envelope~
        #Closed: bool
        +Logger: IFormatterLog
        +Name: string
        +LaunchFormatter(broker)
        +PublishAsync(message): Task
        +Dispose()
        #LaunchInner(config, callback)*
        #ConsumeAndFormatMessagesBackgroundTask(ct)*
    }
    
    class FileWritingFormatterBase {
        <<abstract>>
        #TargetFileStream: Stream
        +LaunchInner(config, callback)
        #ConsumeAndFormatMessagesBackgroundTask(ct)
        #FinalizeInitialization(path, config, callback)
        #CreateTargetFileStream(path): Stream
        #WriteToFile(envelope, ct)*
        #OnTargetFileStreamInitialized(stream)*
        #OnTargetFileStreamDisposing()*
        #FlushTargetFileStream(ct)
    }
    
    class MessageFormatter {
        #WriteToFile(envelope, ct)
        #OnTargetFileStreamInitialized(stream)
        #OnTargetFileStreamDisposing()
    }
    
    class HtmlFormatter {
        -_htmlWriter: MessagesToHtmlWriter
        #WriteToFile(envelope, ct)
        #OnTargetFileStreamInitialized(stream)
        #OnTargetFileStreamDisposing()
        #CreateMessagesToHtmlWriter(stream, serializer)
        #GetHtmlReportSettings(): HtmlReportSettings
    }
    
    ICucumberMessageFormatter <|.. FormatterBase
    FormatterBase <|-- FileWritingFormatterBase
    FileWritingFormatterBase <|-- MessageFormatter
    FileWritingFormatterBase <|-- HtmlFormatter
```

### FormatterBase

The abstract base class for all formatters. Provides:

- **Message Channel**: Unbounded channel for receiving messages asynchronously
- **Lifecycle Management**: Launch, close, and dispose patterns
- **Configuration Integration**: Access to formatter configuration
- **Background Processing**: Abstract method for message consumption

Key methods:
| Method | Purpose |
|--------|---------|
| `LaunchFormatter()` | Entry point called by broker during initialization |
| `LaunchInner()` | Abstract - formatter-specific initialization |
| `PublishAsync()` | Receives messages from broker, adds to channel |
| `ConsumeAndFormatMessagesBackgroundTask()` | Abstract - processes messages from channel |
| `CloseAsync()` | Signals channel completion, waits for processing |
| `Dispose()` | Cleanup with timeout handling |

### FileWritingFormatterBase

Extends `FormatterBase` for formatters that write to files. Provides:

- **File Path Resolution**: Handles configured paths with variable substitution
- **Stream Management**: Opens, flushes, and closes file streams
- **Error Handling**: Graceful handling of file system errors

Key methods:
| Method | Purpose |
|--------|---------|
| `LaunchInner()` | Resolves output path, creates directory, opens stream |
| `FinalizeInitialization()` | Creates file stream, calls initialization hook |
| `WriteToFile()` | Abstract - write single message to file |
| `OnTargetFileStreamInitialized()` | Abstract - hook for stream setup |
| `OnTargetFileStreamDisposing()` | Abstract - hook for stream cleanup |
| `FlushTargetFileStream()` | Ensures all data written to disk |

### MessageFormatter

Produces Cucumber Messages NDJSON output files (`.ndjson`).

- **Output**: One JSON object per line (newline-delimited)
- **Default File**: `reqnroll_report.ndjson`
- **Serialization**: Uses `NdjsonSerializer`

### HtmlFormatter

Produces interactive HTML reports using the Cucumber HTML formatter library.

- **Output**: Self-contained HTML file with embedded viewer
- **Default File**: `reqnroll_report.html`
- **Dependencies**: `Cucumber.HtmlFormatter` library

---

## Configuration

Formatters are configured through the `reqnroll.json` file or environment variables.

### Configuration Resolution Chain

```mermaid
flowchart TB
    subgraph Sources["Configuration Sources"]
        File[reqnroll.json file]
        JsonEnv[REQNROLL_FORMATTERS JSON env var]
        KVEnv[REQNROLL_FORMATTERS_* key-value env vars]
    end
    
    subgraph Resolution["Configuration Resolution"]
        FBR[FileBasedConfigurationResolver]
        JER[JsonEnvironmentConfigurationResolver]
        KVER[KeyValueEnvironmentConfigurationResolver]
        FCP[FormattersConfigurationProvider]
    end
    
    subgraph Override["Disable Override"]
        DOP[FormattersDisabledOverrideProvider]
    end
    
    File --> FBR
    JsonEnv --> JER
    KVEnv --> KVER
    
    FBR --> FCP
    JER --> FCP
    KVER --> FCP
    DOP --> FCP
    
    FCP --> Config[Final Configuration]
```

> **Note:** Configuration sources are evaluated in order: file-based first, then JSON environment variable, then key-value environment variables. Later sources can override earlier ones.

### Configuration Classes

```mermaid
classDiagram
    class IFormattersConfigurationProvider {
        <<interface>>
        +Enabled: bool
        +GetFormatterConfigurationByName(name): IDictionary
        +ResolveTemplatePlaceholders(template): string
    }
    
    class FormattersConfigurationProvider {
        -_resolvers: IList~IFormattersConfigurationResolverBase~
        -_resolvedConfiguration: Lazy~FormattersConfiguration~
        +Enabled: bool
        +GetFormatterConfigurationByName(name): IDictionary
    }
    
    class FormattersConfiguration {
        +Enabled: bool
        +Formatters: IDictionary~string, IDictionary~
    }
    
    class IFormattersConfigurationResolverBase {
        <<interface>>
        +Resolve(): IDictionary~string, IDictionary~
    }
    
    class FileBasedConfigurationResolver
    class JsonEnvironmentConfigurationResolver
    class KeyValueEnvironmentConfigurationResolver
    
    IFormattersConfigurationProvider <|.. FormattersConfigurationProvider
    FormattersConfigurationProvider --> FormattersConfiguration
    FormattersConfigurationProvider --> IFormattersConfigurationResolverBase
    IFormattersConfigurationResolverBase <|.. FileBasedConfigurationResolver
    IFormattersConfigurationResolverBase <|.. JsonEnvironmentConfigurationResolver
    IFormattersConfigurationResolverBase <|.. KeyValueEnvironmentConfigurationResolver
```

### Example Configuration

```json
{
  "formatters": {
    "message": {
      "outputFilePath": "./TestResults/{timestamp}_reqnroll_report.ndjson"
    },
    "html": {
      "outputFilePath": "./TestResults/{timestamp}_reqnroll_report.html"
    }
  }
}
```

---

## Creating Custom Formatters

To create a custom formatter:

1. **Inherit from appropriate base class**:
   - `FormatterBase` for non-file-based formatters
   - `FileWritingFormatterBase` for file-based formatters

2. **Register in dependency container**:
   ```csharp
   container.RegisterTypeAs<MyCustomFormatter, ICucumberMessageFormatter>("myformatter");
   ```

3. **Implement required abstract methods**:
   - `LaunchInner()` or use base implementation
   - `ConsumeAndFormatMessagesBackgroundTask()` or `WriteToFile()`

4. **Configure in reqnroll.json**:
   ```json
   {
     "formatters": {
       "myformatter": {
         "outputFilePath": "./output.custom"
       }
     }
   }
   ```

---

## Summary

The Formatters subsystem provides a robust, extensible architecture for generating Cucumber Messages output. Key design principles:

- **Pub/Sub Pattern**: Decouples message generation from consumption
- **Async Processing**: Background tasks prevent blocking test execution
- **Lazy Initialization**: Configuration resolved on first access
- **Graceful Shutdown**: Timeouts and cancellation support
- **Thread Safety**: Concurrent collections for parallel test execution

For questions or contributions, please refer to the main [CONTRIBUTING.md](../../CONTRIBUTING.md) guide.
