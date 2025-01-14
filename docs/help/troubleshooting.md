# Troubleshooting

## Enabling Tracing

You can enable traces for Reqnroll. Once tracing is enabled, a new `Reqnroll` pane is added to the output window showing diagnostic messages. 

To enable tracing, select **Tools | Options | Reqnroll** from the menu in Visual Studio and set **Enable Tracing** to 'True'. 

## Additional MSBuild logs

You can enable additional Reqnroll-specific MSBuild logs. Enable a higher [level of detail in MSBuild](https://learn.microsoft.com/en-us/visualstudio/msbuild/obtaining-build-logs-with-msbuild#set-the-level-of-detail).

The most important logs are already displayed with `detailed' output.

All Reqnroll log entries have a `[Reqnroll]` prefix.
