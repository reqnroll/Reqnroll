# Available Plugins

Below is a list of plugins for Reqnroll. Use the tag buttons to filter the list, or click "show all" to display every plugin again.

```{important}
Plugins marked with the <span class="reqnroll-docs-tag">official</span> tag are maintained and verified by the [Reqnroll team](https://github.com/reqnroll), and use the same [open‑source license](https://github.com/reqnroll/Reqnroll/blob/main/LICENSE) as Reqnroll.

Plugins marked with the <span class="reqnroll-docs-tag">3rd‑party</span> tag are maintained by third parties. **The Reqnroll team is not responsible for these plugins**. Please review each plugin's behavior and licensing terms before use, and submit issues to the plugin's own repository.
```

```{raw} html
<div id="tag-filter-buttons">
  <button onclick="filterPlugins('all')" class="reqnroll-docs-tag">show all</button>
</div>
```

{#plugins-table}
| Name | Description | Tags | Download |
|---|---|---|---|
| [Reqnroll.Autofac](https://github.com/reqnroll/Reqnroll) | Reqnroll plugin for using Autofac as a dependency injection framework for step definitions. [Read more...](autofac.md) | official di-container | <a href="https://www.nuget.org/packages/Reqnroll.Autofac/">![](https://img.shields.io/nuget/v/Reqnroll.Autofac.svg)</a> |
| [Reqnroll.Microsoft.Extensions. DependencyInjection](https://github.com/reqnroll/Reqnroll) | Reqnroll plugin for using Microsoft.Extensions.DependencyInjection as a dependency injection framework for step definitions. [Read more...](dependency-injection.md) | official di-container |<a href="https://www.nuget.org/packages/Reqnroll.Microsoft.Extensions.DependencyInjection/">![](https://img.shields.io/nuget/v/Reqnroll.Microsoft.Extensions.DependencyInjection.svg)</a> |
| [Reqnroll.Windsor](https://github.com/reqnroll/Reqnroll) | Reqnroll plugin for using Castle Windsor as a dependency injection framework for step definitions. [Read more...](windsor.md) | official di-container |<a href="https://www.nuget.org/packages/Reqnroll.Windsor/">![](https://img.shields.io/nuget/v/Reqnroll.Windsor.svg)</a> |
| [Reqnroll.ExternalData](https://www.nuget.org/packages/Reqnroll.ExternalData/) | Package to use external data in Gherkin scenarios. [Read more...](https://go.reqnroll.net/doc-externaldata) | official |<a href="https://www.nuget.org/packages/Reqnroll.ExternalData/">![](https://img.shields.io/nuget/vpre/Reqnroll.ExternalData.svg)</a> |
| [Reqnroll.Verify](https://github.com/reqnroll/Reqnroll/tree/main/Plugins/Reqnroll.Verify) | Reqnroll plugin for using Verify in scenarios. [Read more...](verify.md) | official xunit |<a href="https://www.nuget.org/packages/Reqnroll.Verify/">![](https://img.shields.io/nuget/v/Reqnroll.Verify.svg)</a> |
| [Reqnroll.Assist.Dynamic](https://github.com/reqnroll/Reqnroll/tree/main/Plugins/Reqnroll.Assist.Dynamic) | Reqnroll library for using dynamic types in bindings. | official | <a href="https://www.nuget.org/packages/Assist.Dynamic/">![](https://img.shields.io/nuget/v/Reqnroll.Assist.Dynamic.svg)</a> |
| [DSL.Reqnroll](https://github.com/nowakpi/DSL.Reqnroll) | Reqnroll plugin that enables use of dynamic test data. [Read more...](https://github.com/nowakpi/DSL.Reqnroll/blob/master/README.md) | 3rd-party | <a href="https://www.nuget.org/packages/DSL.Reqnroll/">![](https://img.shields.io/nuget/v/DSL.Reqnroll.svg)</a> |

```{raw} html
<script>
function filterPlugins(tag) {
  var list = document.getElementById('plugins-table').querySelectorAll("tbody > tr");
  for (var i = 0; i < list.length; i++) {
    var tagsCell = list[i].querySelectorAll("td")[2]
    var tags = []
    for (var tagIndex = 0; tagIndex < tagsCell.children.length; tagIndex++) {
      tags.push(tagsCell.children[tagIndex].innerText)
    }
    if (tag === 'all' || tags.includes(tag)) {
      list[i].style.display = '';
    } else {
      list[i].style.display = 'none';
    }
  }
}

function markTags(){
  var tagsWithButton = []
  var buttonListDiv = document.getElementById('tag-filter-buttons');
  var list = document.getElementById('plugins-table').querySelectorAll("tbody > tr");
  for (var i = 0; i < list.length; i++) {
    var tagsCell = list[i].querySelectorAll("td")[2]
    var tags = tagsCell.innerText.split(/\s+/)
    var tagsHtml = ""
    tagsCell.innerHTML = ''
    tags.forEach(function(tag) {
      var span = document.createElement("span")
      span.innerText = tag
      span.className = "reqnroll-docs-tag"
      span.setAttribute('onclick',"filterPlugins('" + tag + "')")
      tagsCell.appendChild(span)

      if (!tagsWithButton.includes(tag))
      {
        tagsWithButton.push(tag)
        var button = document.createElement("button")
        button.innerText = tag
        button.className = "reqnroll-docs-tag"
        button.setAttribute('onclick',"filterPlugins('" + tag + "')")
        buttonListDiv.appendChild(button)
      }
    });
  }
}

markTags();
</script>
```

```{note}
If you would like your Reqnroll plugin to be listed here, please submit a [pull request](https://github.com/reqnroll/Reqnroll/blob/main/CONTRIBUTING.md#pull-requests) to update the documentation source page: [`main/docs/integrations/available-plugins.md`](https://github.com/reqnroll/Reqnroll/edit/main/docs/integrations/available-plugins.md).
```