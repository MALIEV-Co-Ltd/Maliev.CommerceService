namespace Maliev.CommerceService.Tests.Workflows;

using System;
using System.IO;
using Xunit;

public sealed class WorkflowContractTests
{
    private static readonly string Root = FindRoot();
    private static readonly string Workflows = Path.Combine(Root, ".github", "workflows");

    [Fact]
    public void PullRequests_UseReadOnlyReusableValidation()
    {
        var text = Read("pr-validation.yml");
        Assert.Contains("pull_request:", text);
        Assert.DoesNotContain("paths:", text);
        Assert.Contains("contents: read", text);
        Assert.DoesNotContain("packages: read", text);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", text);
        AssertSafe(text);
    }

    [Theory]
    [InlineData("ci-main.yml", "main")]
    [InlineData("ci-develop.yml", "develop")]
    [InlineData("ci-staging.yml", "release/v*")]
    public void BranchAndTagWorkflows_AreValidationOnly(string file, string trigger)
    {
        var text = Read(file);
        Assert.Contains(trigger, text);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", text);
        AssertSafe(text);
    }

    [Fact]
    public void ReusableValidation_IsCredentialFreeAndUsesPinnedActions()
    {
        var text = Read("_validate.yml");
        Assert.Contains("workflow_call:", text);
        Assert.Contains("name: validate", text);
        Assert.DoesNotContain("NUGET_PASSWORD", text);
        Assert.DoesNotContain("packages: read", text);
        Assert.Contains("persist-credentials: false", text);
        Assert.Contains("actions/checkout@9c091bb21b7c1c1d1991bb908d89e4e9dddfe3e0", text);
        Assert.Contains("actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68", text);
        AssertSafe(text);
    }

    [Fact]
    public void Validation_UsesImmutablePublicSharedSourcesWithoutPackageCredentials()
    {
        var workflow = Read("_validate.yml");
        var project = File.ReadAllText(Path.Combine(Root, "Maliev.CommerceService.Api", "Maliev.CommerceService.Api.csproj"));

        Assert.Contains("repository: MALIEV-Co-Ltd/Maliev.Aspire", workflow);
        Assert.Contains("ref: 01d506203763b914e237268a8746f1406423df86", workflow);
        Assert.Contains("repository: MALIEV-Co-Ltd/Maliev.MessagingContracts", workflow);
        Assert.Contains("ref: 559a00db0c7920a5247fdff60d4476ad23a9a501", workflow);
        Assert.Contains("SharedSourceRoot", workflow);
        Assert.Contains("--configfile nuget.validation.config", workflow);
        Assert.Contains("$(SharedSourceRoot)", project);
        Assert.DoesNotContain("$(GITHUB_ACTIONS)", project);
    }

    [Fact]
    public void AllWorkflows_ForbidLongLivedSecretsAndDeployment()
    {
        foreach (var file in Directory.GetFiles(Workflows, "*.yml"))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("pull_request_target", text, StringComparison.OrdinalIgnoreCase);
            AssertSafe(text);
        }
    }

    [Fact]
    public void Documentation_StatesTheValidationOnlyReleaseBoundary()
    {
        var readme = File.ReadAllText(Path.Combine(Root, "README.md"));
        Assert.Contains("No workflow in this repository publishes", readme);
        Assert.Contains("Aspire owner review", readme);
    }

    private static void AssertSafe(string text)
    {
        foreach (var value in new[] { "secrets.", "GITOPS_PAT", "NUGET_PASSWORD", "id-token: write", "credentials_json", "google-github-actions/auth", "gcloud auth", "docker push", "maliev-gitops", "kustomize edit", "gh pr create" })
        {
            Assert.DoesNotContain(value, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Read(string file)
    {
        var path = Path.Combine(Workflows, file);
        Assert.True(File.Exists(path), $"Required workflow is missing: {file}");
        return File.ReadAllText(path);
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.CommerceService.slnx"))) return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate CommerceService repository root.");
    }
}
