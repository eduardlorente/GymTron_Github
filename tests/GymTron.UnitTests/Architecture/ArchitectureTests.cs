using System.Reflection;
using System.Xml.Linq;
using GymTron.Application.Trainings.Commands;
using GymTron.Domain.Aggregates;
using GymTron.Infrastructure.Persistence.Repositories;
using NetArchTest.Rules;

namespace GymTron.UnitTests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Training).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(StartTrainingCommand).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(RoutineRepository).Assembly;

    [Fact]
    public void DomainLayer_TypeDependencies_ShouldNotDependOnOtherLayersOrDatabaseDrivers()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "GymTron.Application",
                "GymTron.Infrastructure",
                "GymTron.Api",
                "GymTron.App",
                "GymTron.Web",
                "MySql.Data",
                "Dapper",
                "MediatR")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Domain layer types must not depend on outer layers, persistence packages, or MediatR: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainLayer_AssemblyReferences_ShouldNotReferenceOuterLayersOrPersistence()
    {
        List<string> referencedAssemblyNames = DomainAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("GymTron.Application", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Infrastructure", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Api", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.App", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Web", referencedAssemblyNames);
        Assert.DoesNotContain("MySql.Data", referencedAssemblyNames);
        Assert.DoesNotContain("Dapper", referencedAssemblyNames);
        Assert.DoesNotContain("MediatR", referencedAssemblyNames);
    }

    [Fact]
    public void ApplicationLayer_TypeDependencies_ShouldOnlyDependOnDomain()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "GymTron.Infrastructure",
                "GymTron.Api",
                "GymTron.App",
                "GymTron.Web",
                "MySql.Data",
                "Dapper")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Application layer types must not depend on infrastructure, presentation, or database drivers: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationLayer_AssemblyReferences_ShouldNotReferenceInfrastructureOrPresentation()
    {
        List<string> referencedAssemblyNames = ApplicationAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("GymTron.Infrastructure", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Api", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.App", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Web", referencedAssemblyNames);
        Assert.DoesNotContain("MySql.Data", referencedAssemblyNames);
        Assert.DoesNotContain("Dapper", referencedAssemblyNames);
    }

    [Fact]
    public void InfrastructureLayer_TypeDependencies_ShouldNotDependOnApplicationOrPresentation()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "GymTron.Application",
                "GymTron.Api",
                "GymTron.App",
                "GymTron.Web")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Infrastructure layer types must not depend on Application or presentation layers: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void InfrastructureLayer_AssemblyReferences_ShouldNotReferenceApplicationOrPresentation()
    {
        List<string> referencedAssemblyNames = InfrastructureAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("GymTron.Application", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Api", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.App", referencedAssemblyNames);
        Assert.DoesNotContain("GymTron.Web", referencedAssemblyNames);
    }

    [Fact]
    public void ProjectReferences_AcrossLayers_AdhereToPermittedBoundaryMatrix()
    {
        string repoRoot = FindRepositoryRoot();

        List<string> domainProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.Domain", "GymTron.Domain.csproj"));
        List<string> applicationProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.Application", "GymTron.Application.csproj"));
        List<string> infrastructureProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.Infrastructure", "GymTron.Infrastructure.csproj"));
        List<string> webProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.Web", "GymTron.Web.csproj"));
        List<string> apiProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.Api", "GymTron.Api.csproj"));
        List<string> appProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.App", "GymTron.App.csproj"));

        Assert.Empty(domainProjectReferences);

        Assert.Single(applicationProjectReferences);
        Assert.Contains(applicationProjectReferences, p => p.EndsWith("GymTron.Domain.csproj", StringComparison.OrdinalIgnoreCase));

        Assert.Single(infrastructureProjectReferences);
        Assert.Contains(infrastructureProjectReferences, p => p.EndsWith("GymTron.Domain.csproj", StringComparison.OrdinalIgnoreCase));

        Assert.Single(webProjectReferences);
        Assert.Contains(webProjectReferences, p => p.EndsWith("GymTron.Application.csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(webProjectReferences, p => p.EndsWith("GymTron.Infrastructure.csproj", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(2, apiProjectReferences.Count);
        Assert.Contains(apiProjectReferences, p => p.EndsWith("GymTron.Application.csproj", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(apiProjectReferences, p => p.EndsWith("GymTron.Infrastructure.csproj", StringComparison.OrdinalIgnoreCase));

        Assert.Empty(appProjectReferences);
    }

    [Fact]
    public void GymTronApp_ProjectReferences_MustNotDependOnInternalLayers()
    {
        string repoRoot = FindRepositoryRoot();
        List<string> appProjectReferences = GetProjectReferences(Path.Combine(repoRoot, "src", "GymTron.App", "GymTron.App.csproj"));

        Assert.DoesNotContain(appProjectReferences, p => p.Contains("GymTron.Domain", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(appProjectReferences, p => p.Contains("GymTron.Application", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(appProjectReferences, p => p.Contains("GymTron.Infrastructure", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(appProjectReferences);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null && !File.Exists(Path.Combine(current.FullName, "GymTron.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Could not locate repository root with GymTron.sln");
    }

    private static List<string> GetProjectReferences(string csprojPath)
    {
        XDocument doc = XDocument.Load(csprojPath);
        return doc.Descendants("ProjectReference")
            .Select(pr => pr.Attribute("Include")?.Value)
            .Where(val => !string.IsNullOrWhiteSpace(val))
            .Select(val => val!)
            .ToList();
    }
}
