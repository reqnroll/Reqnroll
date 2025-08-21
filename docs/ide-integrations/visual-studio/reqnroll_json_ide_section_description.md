# Reqnroll Configuration: `ide` Section

The `ide` section configures all extension settings related to the **Integrated Development Environment (IDE)** for Reqnroll projects. This section is extensible and allows fine-tuning of your development experience with Reqnroll.

## Schema Structure

```json
"ide": {
  "reqnroll": { "$ref": "#/definitions/Reqnroll" },
  "traceability": { "$ref": "#/definitions/Traceability" },
  "editor": { "$ref": "#/definitions/Editor" },
  "bindingDiscovery": { "$ref": "#/definitions/BindingDiscovery" }
}
```


***

## Elements in `ide`

### 1. `reqnroll`

- **Purpose**: Handles project-level settings related to Reqnroll itself.
- **Type**: object
- **Properties**:
    - `isReqnrollProject` (boolean): Enables the project as a Reqnroll project. Default: *(auto-detect)*.
    - `configFilePath` (string): Path to `App.config` or `reqnroll.json`. Default: *(auto-detect)*.
    - `version` (string): Specifies the Reqnroll version (e.g., `"2.3.1"`). Default: *(auto-detect)*.
    - `traits` (array): List of traits (e.g., `"XUnitAdapter"`, `"MsBuildGeneration"`, `"DesignTimeFeatureFileGeneration"`). Default: *(detected from NuGet packages)*.


### 2. `traceability`

- **Purpose**: Enables traceability settings for scenarios, such as linking scenario tags to external issue trackers.
- **Type**: object
- **Properties**:
    - `tagLinks` (array): Defines patterns for tags and the corresponding external URLs.
        - Each entry:
            - `tagPattern` (string): Regex to match tag names (e.g., `"issue\\:(?<id>\\d+)"`).
            - `urlTemplate` (string): URL template using captured regex groups (e.g., `"https://github.com/org/repo/issues/{id}"`).


#### Example

```json
"traceability": {
  "tagLinks": [
    {
      "tagPattern": "issue\\:(?<id>\\d+)",
      "urlTemplate": "https://github.com/org/repo/issues/{id}"
    }
  ]
}
```


### 3. `editor`

- **Purpose**: Controls editor behaviors such as feature file formatting and code completion.
- **Type**: object
- **Properties**:
    - `showStepCompletionAfterStepKeywords` (boolean): Enables/disables step completions after keywords (`Given`, `When`, etc.). Default: `true`.
    - `gherkinFormat` (object): Controls the formatting of Gherkin feature files.
        - `indentFeatureChildren` (boolean): Indent children of `Feature` (`Background`, `Rule`, etc.). Default: `false`.
        - `indentRuleChildren` (boolean): Indent children of `Rule` elements. Default: `false`.
        - `indentSteps` (boolean): Indent steps in scenarios. Default: `true`.
        - `indentAndSteps` (boolean): Extra indent for "And"/"But" steps. Default: `false`.
        - `indentDataTable` (boolean): Indent `DataTable` arguments. Default: `true`.
        - `indentDocString` (boolean): Indent `DocString` arguments. Default: `true`.
        - `indentExamples` (boolean): Indent `Examples` blocks. Default: `false`.
        - `indentExamplesTable` (boolean): Indent `Examples` tables. Default: `true`.
        - `tableCellPaddingSize` (integer): Padding for table cells (spaces, default: `1`).
        - `tableCellRightAlignNumericContent` (boolean): Specifies whether Table cells that contain digits should be right-aligned. Default: `true`.


#### Example

```json
"editor": {
  "showStepCompletionAfterStepKeywords": true,
  "gherkinFormat": {
    "indentFeatureChildren": false,
    "indentSteps": true,
    "indentAndSteps": false,
    "tableCellPaddingSize": 1
  }
}
```


### 4. `bindingDiscovery`

- **Purpose**: Manages settings for discovering step bindings within the IDE.
- **Type**: object
- **Properties**:
    - `connectorPath` (string): File path to custom binding connector. Can reference environment variables (e.g., `%ENV_VAR%`). Relative paths use the default connector folder as base.


#### Example

```json
"bindingDiscovery": {
  "connectorPath": "%USERPROFILE%/custom-connector"
}
```


***

## Example `ide` Configuration

```json
"ide": {
  "reqnroll": {
    "isReqnrollProject": true,
    "configFilePath": "reqnroll.json",
    "version": "2.3.1",
    "traits": ["XUnitAdapter"]
  },
  "traceability": {
    "tagLinks": [
      {
        "tagPattern": "issue\\:(?<id>;\\d+)",
        "urlTemplate": "https://github.com/org/repo/issues/{id}"
      }
    ]
  },
  "editor": {
    "showStepCompletionAfterStepKeywords": true,
    "gherkinFormat": {
      "indentFeatureChildren": false,
      "indentSteps": true
    }
  },
  "bindingDiscovery": {
    "connectorPath": "connectors/customConnector"
  }
}
```


***

## See Also

- [Reqnroll Configuration Reference](https://go.reqnroll.net/doc-config)
- [Traceability Settings](http://speclink.me/deveroomtraceability)


