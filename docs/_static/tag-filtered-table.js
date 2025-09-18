// JavaScript for tag-filtered tables
// Adds buttons to filter table rows based on tags in the "Tags" column
// Tables must have the class "tag-filtered-table" to be processed

document.addEventListener('DOMContentLoaded', function () {
  'use strict';

  try {
    function filterList(table, tag, buttonListDiv) {
      const tableRows = table.querySelectorAll("tbody > tr");
      for (let i = 0; i < tableRows.length; i++) {
        const row = tableRows[i];
        const tags = Array.from(row.querySelectorAll('span.reqnroll-docs-tag')).map(span => span.textContent.trim())
        const shouldShow = tag === 'all' || tags.includes(tag);
        if (shouldShow) {
          tableRows[i].style.display = '';
          tableRows[i].setAttribute('aria-hidden', 'false');
        } else {
          tableRows[i].style.display = 'none';
          tableRows[i].setAttribute('aria-hidden', 'true');
        }
      }

      const filterButtons = buttonListDiv.querySelectorAll('.reqnroll-docs-tag');
      filterButtons.forEach(function (button) {
        if (button.innerText === tag) {
          button.classList.add('selected');
        } else {
          button.classList.remove('selected');
        }
      });
    }

    function getTagsCellIndex(table) {
      const headings = table.querySelectorAll("thead > tr > th");
      let tagCellIndex = -1;
      headings.forEach(function (heading, index) {
        if (heading.innerText.trim() === "Tags") {
          tagCellIndex = index;
        }
      });
      return tagCellIndex;
    }

    function getTableWrapper(table) {
      return table.parentElement &&
        table.parentElement.tagName === 'DIV' &&
        table.parentElement.classList.contains('tag-filtered-table') ?
        table.parentElement : table
    }

    function createButtonListDiv(table) {
      const buttonListDiv = document.createElement("div")
      buttonListDiv.className = "reqnroll-docs-tag-button-list"
      buttonListDiv.setAttribute('role', 'group')
      buttonListDiv.setAttribute('aria-label', 'Filter list by tag')
      const allButton = document.createElement("button")
      allButton.innerText = "show all"
      allButton.className = "reqnroll-docs-tag"
      addFiltering(allButton, 'all', table, buttonListDiv, 'Show all items')
      buttonListDiv.appendChild(allButton)
      return buttonListDiv
    }

    function addFiltering(element, tag, table, buttonListDiv, label) {

      label = label || `Filter by '${tag}' tag`
      element.setAttribute('role', 'button')
      element.setAttribute('tabindex', '0')
      element.setAttribute('aria-label', label)
      element.setAttribute('title', label);

      // Add mouse/touch support
      element.addEventListener('click', function (event) {
        event.preventDefault();
        filterList(table, tag, buttonListDiv);
      });
      
      // Add keyboard support
      element.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          filterList(table, tag, buttonListDiv);
        }
      });
    }

    function initializeTable(table) {
      const wrapper = getTableWrapper(table);
      const tagCellIndex = getTagsCellIndex(table);
      if (tagCellIndex === -1) {
        console.warn("No 'Tags' column found in table:", table);
        return;
      }

      const tagsWithButton = []
      const buttonListDiv = createButtonListDiv(table);
      wrapper.parentNode.insertBefore(buttonListDiv, wrapper);
      const list = table.querySelectorAll("tbody > tr");
      for (let i = 0; i < list.length; i++) {
        const tagsCell = list[i].querySelectorAll("td")[tagCellIndex]
        const tagText = tagsCell.textContent || tagsCell.innerText || '';
        const tags = tagText.split(/\s+/).filter(function(tag) { return tag.trim().length > 0; });
        tagsCell.replaceChildren()
        tags.forEach(function (tag) {
          const span = document.createElement("span")
          span.innerText = tag
          span.className = "reqnroll-docs-tag"
          addFiltering(span, tag, table, buttonListDiv)
          tagsCell.appendChild(span)

          if (!tagsWithButton.includes(tag)) {
            tagsWithButton.push(tag)
            const button = document.createElement("button")
            button.innerText = tag
            button.className = "reqnroll-docs-tag"
            addFiltering(button, tag, table, buttonListDiv)
            buttonListDiv.appendChild(button)
          }
        });
      }
    }

    const tagFilteredTables = document.querySelectorAll('table.tag-filtered-table')
    for (let tableIndex = 0; tableIndex < tagFilteredTables.length; tableIndex++) {
      const table = tagFilteredTables[tableIndex]
      initializeTable(table)
    }

  } catch (error) {
    console.error('Error initializing tag-filtered tables:', error);
  }
});