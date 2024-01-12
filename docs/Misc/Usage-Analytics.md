# Usage Analytics

In order to improve the quality of Reqnroll and understand how it is used, we collect anonymous usage data via Azure Application Insights, an analytics platform provided by Microsoft. We do not collect any personally identifiable information, but information such as the Visual Studio version, operating system, MSBuild version, target frameworks etc.

For more details on the information collected by Application Insights, see our [privacy policy](https://reqnroll.net/privacy-policy/).  

You can disable these analytics as follows:

* Select **Tools | Options** from the menu in Visual Studio, and navigate to **Reqnroll* in the list on the left. Set **Opt-Out of Data Collection** to "True".
  This disables analytics collected by the Visual Studio extension (see "Reqnroll Visual Studio Extension" in the [privacy policy](https://reqnroll.net/privacy-policy/) for details).
* Define an environment variable called SPECFLOW_TELEMETRY_ENABLED and set its value to 0.
  This disables all analytics, i.e. those collected by both the extension and Reqnroll itself (see "Reqnroll" and "Reqnroll Visual Studio Extension" in the [privacy policy](https://reqnroll.net/privacy-policy/) for details)

