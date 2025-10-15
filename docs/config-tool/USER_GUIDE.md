# Interactive Rule Configuration Tool - User Guide

## Overview

The Interactive Rule Configuration Tool is a web-based application that helps developers customize CSS .NET Coding Standards for their projects without manually editing `.editorconfig` files.

## Features

### 🎯 Browse All Rules
- View all 2046+ analyzer rules from multiple packages
- Each rule includes:
  - Rule ID (e.g., CA1000, IDE0055)
  - Title and description
  - Category (Style, Quality, Security, Performance, Other)
  - Default severity level
  - Link to official documentation
  - Source package information

### 🔍 Advanced Filtering
- **Search**: Find rules by ID, title, or description
- **Category Filter**: Show only rules from specific categories
- **Severity Filter**: Filter by current severity level
- **Pagination**: Navigate through rules efficiently (50 per page)

### 📋 Project Templates
Apply pre-configured rule sets optimized for different project types:

- **Default**: All rules as configured in the base package
- **API Project**: Enhanced security rules for Web API projects
- **Library**: Stricter quality rules for class libraries
- **Blazor**: Optimized for Blazor applications
- **Test Project**: Relaxed rules for test projects

### ⚙️ Interactive Configuration
- Adjust severity for any rule with a dropdown selector:
  - **none**: Rule is disabled
  - **silent**: Enabled but no diagnostics
  - **suggestion**: IDE suggestion (green squiggle)
  - **warning**: Warning (yellow squiggle)
  - **error**: Error (red squiggle)
- Color-coded severity indicators for easy identification
- Track how many rules you've modified

### 💾 Export Configuration
- Generate a custom `.editorconfig` file with your changes
- Download includes:
  - Header with template information
  - Generation timestamp
  - Only modified rules (unchanged rules use defaults)
  - Comments with rule titles

## How to Use

### Step 1: Access the Tool
Visit: https://clinical-support-systems.github.io/css-dotnet-codingstandards/config-tool/

### Step 2: Choose a Template (Optional)
If you're starting from scratch, select a project template that matches your project type. This will apply sensible defaults.

### Step 3: Browse and Filter Rules
Use the search box and filters to find rules you want to customize:
- Search for specific rule IDs (e.g., "CA1000")
- Filter by category (Style, Quality, Security, Performance)
- Filter by severity level

### Step 4: Adjust Severities
Click on the severity dropdown for any rule and select your preferred level. The tool will track your changes.

### Step 5: Export Configuration
Click the "Export Configuration" button at the bottom right to download your custom `.editorconfig` file.

### Step 6: Apply to Your Project
1. Open the downloaded `.editorconfig` file
2. Copy the contents
3. Paste into your project's `.editorconfig` file (or create one if it doesn't exist)
4. Commit the changes

## Statistics Dashboard

The tool displays real-time statistics:
- **Filtered Rules**: Number of rules matching your current filters
- **Modified Rules**: Number of rules you've customized
- **Category Counts**: Distribution of rules by category

## Examples

### Example 1: Relax Styling Rules for Test Projects
1. Select "Test Project" template
2. Search for "style" category
3. Note that many style rules are set to "silent"
4. Export and apply to your test project

### Example 2: Enhance Security for API Projects
1. Select "API Project" template
2. Filter by "Security" category
3. Note that security rules are elevated to "error"
4. Export and apply to your API project

### Example 3: Custom Configuration
1. Start with "Default" template
2. Search for "CA1031" (Do not catch general exception types)
3. Change severity from "none" to "warning"
4. Search for "IDE0055" (Fix formatting)
5. Change severity from "error" to "warning"
6. Export your custom configuration

## Tips

- **Start with a template**: Templates provide sensible defaults for common scenarios
- **Use search effectively**: Search by rule ID when you know what you're looking for
- **Read descriptions**: Click documentation links to understand what each rule does
- **Test incrementally**: Apply changes gradually and test your build
- **Document your choices**: Add comments in your `.editorconfig` explaining why rules were modified

## Technical Details

### Data Source
Rule metadata is automatically generated from:
- Microsoft.CodeAnalysis.NetAnalyzers
- Microsoft.CodeAnalysis.CSharp.CodeStyle
- StyleCop.Analyzers
- Meziantou.Analyzer
- Microsoft.CodeAnalysis.BannedApiAnalyzers

### Updates
The rules metadata is regenerated with each package release, ensuring you always have the latest rule information.

### Browser Compatibility
The tool uses modern web technologies and works best with:
- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)

## Troubleshooting

### No rules showing
- Check your internet connection
- Try refreshing the page
- Clear browser cache if needed

### Export not working
- Check that JavaScript is enabled
- Try a different browser
- Check for browser extensions that might block downloads

### Can't find a specific rule
- Use the search feature with the rule ID
- Check if you have any filters applied
- Some rules may be deprecated or renamed

## Support

For issues or questions:
- Check the [main repository](https://github.com/Clinical-Support-Systems/css-dotnet-codingstandards)
- Review the [package documentation](https://www.nuget.org/packages/CSS.DotNet.CodingStandards/)
- Search existing issues or create a new one
