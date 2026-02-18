# Configuration file for the Sphinx documentation builder.
#
# This file only contains a selection of the most common options. For a full
# list see the documentation:
# https://www.sphinx-doc.org/en/master/usage/configuration.html

# -- Path setup --------------------------------------------------------------

# If extensions (or modules to document with autodoc) are in another directory,
# add these directories to sys.path here. If the directory is relative to the
# documentation root, use os.path.abspath to make it absolute, like shown here.
# sys.path.insert(0, os.path.abspath('.'))
#
import os
import sys
from datetime import datetime


# -- Project information -----------------------------------------------------


project = 'Reqnroll'
copyright = f"2024-{datetime.now().year}, Reqnroll"
author = 'Reqnroll'


# -- General configuration ---------------------------------------------------

# Add any Sphinx extension module names here, as strings. They can be
# extensions coming with Sphinx (named 'sphinx.ext.*') or your custom
# ones.
extensions = [
  'myst_parser',  # see https://myst-parser.readthedocs.io/en/v0.16.0/sphinx/intro.html, https://jdsalaro.com/cheatsheet/sphinx-myst-markdown/
  'sphinx_copybutton',
  'sphinxcontrib.googleanalytics'  # see https://github.com/sphinx-contrib/googleanalytics
]

# Add any paths that contain templates here, relative to this directory.
templates_path = ['_templates']

# List of patterns, relative to source directory, that match files and
# directories to ignore when looking for source files.
# This pattern also affects html_static_path and html_extra_path.
exclude_patterns = ['_build', 'Thumbs.db', '.DS_Store', 'Lib']

master_doc = 'index'

# -- sphinxcontrib.googleanalytics -------------------------------------------------

googleanalytics_id = 'G-Y2KPXR5RYB'
googleanalytics_enabled = (os.getenv("READTHEDOCS", "False") == "True") # enable it only on read-the-docs, see https://docs.readthedocs.io/en/stable/reference/environment-variables.html

# -- Options MyST -------------------------------------------------

# See supported Callouts & Admonitions at https://mystmd.org/guide/admonitions

myst_enable_extensions = [
    "attrs_block",
    "colon_fence",
    "attrs_inline"
]

myst_heading_anchors = 3

# -- Options for HTML output -------------------------------------------------

# The theme to use for HTML and HTML Help pages.  See the documentation for
# a list of builtin themes.
#
html_theme = "furo"  # see https://pradyunsg.me/furo/

# Add any paths that contain custom static files (such as style sheets) here,
# relative to this directory. They are copied after the builtin static files,
# so a file named "default.css" will overwrite the builtin "default.css".
html_static_path = ['_static']

html_logo = "_static/logo.png"
html_favicon = 'favicon.ico'

html_title = '%s Documentation' % project

html_theme_options = {
    "source_repository": "https://github.com/reqnroll/Reqnroll",
    "source_branch": "main",
    "source_directory": "docs/",
    # "announcement": "<em>Important</em> announcement!",
    # "sidebar_hide_name": True,
    "light_css_variables": {  # see https://github.com/pradyunsg/furo/blob/main/src/furo/assets/styles/variables/_colors.scss
        "color-brand-primary": "#3B4000",
        "color-brand-content": "#3B4000",
        # "color-background-primary": "#FFFFFF",                  # for content
        "color-background-secondary": "#fffff0",                # for navigation + ToC
        "color-background-hover": "#c4d600ff",                  # for navigation-item hover
        "color-background-hover--transparent": "#c4d60000",
        "color-background-border": "#f6f9d9",                   # for UI borders
        "color-background-item": "#ccc",                        # for "background" items (eg: copybutton)"
        "color-announcement-background": "#3b4000",
        "color-announcement-text": "#f6f9d9"
    },
    "dark_css_variables": {
        "color-brand-primary": "#c4d600",
        "color-brand-content": "#c4d600",
        "color-background-primary": "#11111f",                  # for content
        "color-background-secondary": "#262a00",                # for navigation + ToC
        "color-background-hover": "#3b4000ff",                  # for navigation-item hover
        "color-background-hover--transparent": "#3b400000",
        "color-background-border": "#3b4000",                   # for UI borders
        # "color-background-item": "#ccc",                        # for "background" items (eg: copybutton)"
        # "color-announcement-background": "#3b4000",
        # "color-announcement-text": "#f6f9d9"
    }
}

html_css_files = [
    'css/custom.css',
] 

html_js_files = [
    'tag-filtered-table.js',  # Enable tag-filtered-table functionality
]

# see https://pygments.org/styles/
pygments_style = "solarized-light"
pygments_dark_style = "lightbulb"

# html_baseurl = 'https://docs.reqnroll.net/projects/reqnroll/'
# html_extra_path = ['robots.txt']

sys.path.append(os.path.abspath('exts'))

# sitemap_filename = 'sitemap_generated.xml'
# sitemap_url_scheme = "{lang}latest/{link}"
