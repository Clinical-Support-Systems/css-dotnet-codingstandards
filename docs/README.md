# CSS .NET Coding Standards - Documentation

Welcome to the documentation for CSS .NET Coding Standards.

## 🔧 Interactive Rule Configuration Tool

Use our [Interactive Rule Configuration Tool](./config-tool/) to:

- Browse all 500+ rules with descriptions and help links
- Filter rules by category (Style, Quality, Security, Performance)
- Search rules by ID, title, or description
- Adjust severity levels interactively with live preview
- Apply rule templates for different project types:
  - **API Project** - Optimized for Web API projects
  - **Library** - Optimized for class libraries
  - **Blazor** - Optimized for Blazor applications
  - **Test Project** - Relaxed rules for test projects
- Export custom `.editorconfig` snippet to download

### Getting Started

1. Visit the [Configuration Tool](./config-tool/)
2. Browse or search for rules you want to customize
3. Adjust severity levels using the dropdown selectors
4. Click "Export Configuration" to download your custom `.editorconfig`
5. Add the downloaded configuration to your project's `.editorconfig` file

### Rule Categories

- **Style**: Code formatting and naming conventions
- **Quality**: Code quality and maintainability rules
- **Security**: Security-related analysis rules
- **Performance**: Performance optimization rules
- **Other**: Miscellaneous rules

### Severity Levels

- **none**: The rule is disabled
- **silent**: The rule is enabled but doesn't produce any diagnostics
- **suggestion**: Produces an IDE suggestion (green squiggle)
- **warning**: Produces a warning (yellow squiggle)
- **error**: Produces an error (red squiggle)

## Rules Metadata

The complete rules metadata is available in JSON format: [rules-metadata.json](./rules-metadata.json)

This file contains:
- Rule ID
- Title
- Description
- Help URL
- Category
- Default severity
- Package source

## Learn More

- [Main Repository](https://github.com/Clinical-Support-Systems/css-dotnet-codingstandards)
- [NuGet Package](https://www.nuget.org/packages/CSS.DotNet.CodingStandards/)
- [Microsoft .NET Code Analysis Documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
