
# Extension Settings/Options

The extension settings can be configured as per below:

::: tabs
::: tab
VS2019

Navigate to **Tools \| Options \| Reqnroll \| General \|** to access the
extension settings. .. figure:: /\_static/images/vs2019settings.png
:alt: vs2019
:::

::: tab
VS2022

You must edit the
[reqnroll.json](https://reqnroll.net/wp-content/uploads/reqnrollconfigs/reqnroll-config.json)
config file to access the extension settings. If you don\'t have the
reqnroll.json file you can add it by right clicking on the Reqnroll
project -\> Add -\> New item\... -\> Add Reqnroll configuration file.

The configuration file has a JSON
[schema](https://reqnroll.net/wp-content/uploads/reqnrollconfigs/reqnroll-config.json)
, therefore you will see all available properties as you start typing.
.. important:: You must build your project for the changes in
reqnroll.json to take effect.

![](../_static/images/vs2022configfile.png)

alt

:   vs2022
:::
:::

