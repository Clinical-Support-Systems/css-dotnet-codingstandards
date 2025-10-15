# CSS.DotNet.CodingStandards.Migrator

An automated migration assistant CLI tool designed to streamline the adoption of the CSS.DotNet.CodingStandards package in existing codebases.

## Overview

The Migrator tool transforms the often painful multi-week adoption process into a structured, automated workflow. It:

- **Analyzes** codebases to identify violations
- **Categorizes** issues by migration stage (formatting → naming → logic → manual)
- **Automatically fixes** issues in batches with separate commits
- **Generates reports** with before/after statistics
- **Suggests suppressions** for problematic areas
- **Provides rollback** capability per stage

## Installation

### As a .NET Tool (Recommended)

```bash
dotnet tool install --global CSS.DotNet.CodingStandards.Migrator
```

### From Source

```bash
cd tools/Migrator
dotnet build
dotnet run -- --help
```

## Commands

### Analyze

Analyzes a codebase and generates a detailed violation report.

```bash
css-migrator analyze <path-to-solution-or-project> [options]
```

**Options:**
- `--output <file>` - Save report to a file (optional)
- `--format <format>` - Report format: `text` (default) or `json`

**Example:**
```bash
css-migrator analyze MyProject.sln --output report.txt
```

### Fix

Applies automatic fixes for violations in specified stages.

```bash
css-migrator fix <path-to-solution-or-project> [options]
```

**Options:**
- `--stage <stage>` - Specific stage to fix: `formatting`, `naming`, `logic`, or `all` (default)
- `--dry-run` - Preview changes without applying them
- `--commit` - Automatically commit changes after each stage

**Examples:**
```bash
# Fix all auto-fixable stages
css-migrator fix MyProject.sln --commit

# Fix only formatting issues with dry-run
css-migrator fix MyProject.sln --stage formatting --dry-run

# Fix naming issues and commit
css-migrator fix MyProject.sln --stage naming --commit
```

### Suggest

Generates suppression suggestions for issues that cannot be automatically fixed.

```bash
css-migrator suggest <path-to-solution-or-project> [options]
```

**Options:**
- `--output <file>` - Save suggestions to a file (optional)

**Example:**
```bash
css-migrator suggest MyProject.sln --output suppressions.md
```

### Status

Shows current repository and migration status.

```bash
css-migrator status <path-to-repository>
```

**Example:**
```bash
css-migrator status .
```

### Rollback

Rolls back to a previous migration checkpoint.

```bash
css-migrator rollback <path-to-repository> <checkpoint-name>
```

**Example:**
```bash
css-migrator rollback . migration-formatting-20241015120000
```

## Migration Stages

The tool organizes fixes into four stages executed in order:

### 1. Formatting Stage
- Code indentation and spacing
- Line breaks and empty lines
- Braces and brackets placement
- File-scoped namespaces
- Unnecessary using directives

**Diagnostics:** IDE0001, IDE0004, IDE0005, IDE0007, IDE0009, IDE0011, IDE0055, IDE0161, IDE2000-IDE2006

### 2. Naming Stage
- Naming convention violations
- Consistent casing for types, methods, properties, etc.

**Diagnostics:** IDE1006

### 3. Logic Stage
- Auto-fixable code quality improvements
- Simplified expressions
- Modern C# pattern usage
- Unnecessary code removal

**Diagnostics:** IDE0010-IDE0305 (selected auto-fixable rules)

### 4. Manual Stage
- Issues requiring human review
- Complex refactoring
- Security-sensitive changes

## Typical Workflow

### 1. Initial Analysis
```bash
css-migrator analyze MyProject.sln --output initial-report.txt
```

Review the report to understand the scope of changes needed.

### 2. Apply Fixes in Stages

```bash
# Stage 1: Formatting (typically the largest number of issues)
css-migrator fix MyProject.sln --stage formatting --commit

# Stage 2: Naming
css-migrator fix MyProject.sln --stage naming --commit

# Stage 3: Logic
css-migrator fix MyProject.sln --stage logic --commit
```

Each stage creates a separate commit with a descriptive message.

### 3. Handle Manual Issues

```bash
css-migrator suggest MyProject.sln --output manual-items.md
```

Review the suggestions and decide whether to:
- Fix manually
- Suppress temporarily
- Adjust .editorconfig settings

### 4. Verify Changes

After each stage:
- Run your tests
- Review the committed changes
- Roll back if needed: `css-migrator rollback . <checkpoint-name>`

## Integration with CI/CD

### GitHub Actions Example

```yaml
name: Migration Report

on:
  workflow_dispatch:

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Install Migrator
        run: dotnet tool install --global CSS.DotNet.CodingStandards.Migrator
      
      - name: Analyze Codebase
        run: css-migrator analyze MySolution.sln --output report.txt
      
      - name: Upload Report
        uses: actions/upload-artifact@v3
        with:
          name: migration-report
          path: report.txt
```

## Best Practices

1. **Start with Analysis**: Always run `analyze` first to understand the scope
2. **Use Dry-Run**: Test fixes with `--dry-run` before committing
3. **One Stage at a Time**: Apply fixes incrementally to minimize risk
4. **Enable Commits**: Use `--commit` to create checkpoint commits
5. **Test Between Stages**: Run your test suite after each stage
6. **Create Backups**: Ensure you have a clean git state before starting
7. **Review Changes**: Always review the changes before pushing

## Troubleshooting

### Build Errors After Fixes

If you encounter build errors after applying fixes:

1. Review the last commit: `git log -1 --stat`
2. Check specific changes: `git diff HEAD~1`
3. Rollback if needed: `css-migrator rollback . <checkpoint>`
4. Report issues with specific examples

### Unable to Fix Certain Rules

Some rules cannot be automatically fixed. The tool will:
- Identify them in the report
- Provide suppression suggestions via `suggest` command
- Categorize them as "Manual" stage

### Performance with Large Codebases

For very large solutions:
- Analyze individual projects first
- Fix projects one at a time
- Consider using `--stage` to break down work
- Use `--dry-run` to estimate time

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

Copyright © 2024, CSS. This code is licensed under the Apache License, Version 2.0.

## Support

For issues, questions, or suggestions:
- [GitHub Issues](https://github.com/Clinical-Support-Systems/css-dotnet-codingstandards/issues)
- [Documentation](https://github.com/Clinical-Support-Systems/css-dotnet-codingstandards)
