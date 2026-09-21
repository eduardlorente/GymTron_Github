# C# Guidance

This guide classifies repository evidence rather than turning every recurring style into a mandate. Apply confirmed conventions in new or changed code; preserve generated and platform-owned shapes unless the task requires changing them.

## Current and confirmed

### Project settings and language

- All five current project files enable nullable reference types and implicit global usings through `Nullable` and `ImplicitUsings`.
- Write identifiers, logs, and technical exception messages in English.
- Product code overwhelmingly uses file-scoped namespaces. Generated or platform startup files may retain block-scoped namespaces.
- No `GlobalUsings.cs` file currently exists. This observation is not a prohibition on introducing one through an approved change.

### Naming

- Prefix interfaces with `I`.
- Use PascalCase for types and members.
- Use camelCase for parameters and local variables.
- Name fields that hold injected dependencies `_camelCase`.

### Declarations and construction

- Explicit local types dominate core code. `var` is valid and common when the type is obvious or anonymous; do not impose either form universally.
- Target-typed `new` and collection expressions such as `[]` are established repository patterns where the target language version supports them.
- Primary constructors are common for dependency-injection-oriented handlers, repositories, DAL implementations, App services, and feature Razor `PageModel` classes. Use them in those established roles when they improve consistency; they are not a requirement for entities, view models, pages, or every class.

### Asynchronous code

- `Task`-returning asynchronous methods are established throughout the codebase.
- MediatR handlers receive `CancellationToken` and forward it through repository, DAL, and Dapper calls; the token parameter carries a `default` value to keep call sites without a token source concise.
- Non-event `async void` methods occur in MAUI loading and selection flows. Treat them as existing debt, not as an approved pattern.

## Formatting and static analysis

The repository uses `.editorconfig` at the root to enforce consistent code style across all projects. Key conventions:

- **Indentation**: 4 spaces for C# code, 2 spaces for JSON, XML, YAML, and XAML files
- **Line endings**: LF (Unix-style)
- **Encoding**: UTF-8
- **Namespaces**: File-scoped namespaces preferred
- **Braces**: Allman style (open brace on new line) for types and methods
- **Naming**: Enforced via `.editorconfig` rules (PascalCase for types/members, camelCase for parameters/locals, `_camelCase` for private fields)

### Analyzers

All projects reference two analyzer packages:

- **SonarAnalyzer.CSharp**: Detects bugs, vulnerabilities, and code smells
- **Meziantou.Analyzer**: Provides additional .NET-specific rules

Analyzer severity levels are configured in `.editorconfig`:
- **Errors**: Critical issues that must be fixed (e.g., S2259 - null dereference)
- **Warnings**: Important issues that should be addressed
- **Suggestions**: Code quality improvements for future consideration

### Build configuration

`Directory.Build.props` at the root applies common settings to all projects:
- `TreatWarningsAsErrors`: Enabled (warnings fail the build)
- `EnforceCodeStyleInBuild`: Enabled (style violations fail the build)
- Suppressed warnings: NuGet vulnerability warnings (NU1900, NU1301) and selected nullable reference warnings

### Observed but not governed

The repository is inconsistent in these areas, so this guide does not prescribe a universal form:

- member or "newspaper" ordering;
- expression-bodied versus block-bodied members;
- braces for single-statement control flow;
- `Async` method suffixes;
- comment density and comment formatting;
- blank-line density.

Avoid unrelated restyling. These areas may be standardized in future changes.
