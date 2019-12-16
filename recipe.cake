#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&version=2.0.0-unstable0157&prerelease

Environment.SetVariableNames(githubUserNameVariable: "GITTOOLS_GITHUB_USERNAME",
							githubPasswordVariable: "GITTOOLS_GITHUB_PASSWORD");

BuildParameters.SetParameters(context: Context,
							buildSystem: BuildSystem,
							sourceDirectoryPath: "./Source",
							title: "GitReleaseManager",
							repositoryOwner: "GitTools",
							repositoryName: "GitReleaseManager",
							appVeyorAccountName: "GitTools",
							shouldRunGitVersion: true,
							shouldRunDotNetCorePack: true,
                            shouldRunIntegrationTests: true,
                            integrationTestScriptPath: "./tests/integration/tests.cake");

BuildParameters.PackageSources.Add(new PackageSourceData(Context, "GPR", "https://nuget.pkg.github.com/GitTools/index.json", FeedType.NuGet, false));

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
							dupFinderExcludePattern: new string[] {
								BuildParameters.RootDirectoryPath + "/Source/GitReleaseManager.Tests/*.cs",
                                BuildParameters.RootDirectoryPath + "/Source/GitReleaseManager.IntegrationTests/*.cs",
								BuildParameters.RootDirectoryPath + "/Source/GitReleaseManager/AutoMapperConfiguration.cs" },
							testCoverageFilter: "+[GitReleaseManager*]* -[GitReleaseManager.Tests*]*",
							testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
							testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");

BuildParameters.Tasks.DotNetCoreBuildTask.Does((context) =>
{
	var buildDir = BuildParameters.Paths.Directories.PublishedApplications;

	var grmExecutable = context.GetFiles(buildDir + "/**/*.exe").First();

	context.Information("Registering Built GRM executable...");
	context.Tools.RegisterFile(grmExecutable);
});

BuildParameters.Tasks.CreateReleaseNotesTask
	.IsDependentOn(BuildParameters.Tasks.DotNetCoreBuildTask); // We need to be sure that the executable exist, and have been registered before using it

((CakeTask)BuildParameters.Tasks.ExportReleaseNotesTask.Task).ErrorHandler = null;
((CakeTask)BuildParameters.Tasks.PublishGitHubReleaseTask.Task).ErrorHandler = null;
BuildParameters.Tasks.PublishPreReleasePackagesTask.IsDependentOn(BuildParameters.Tasks.PublishGitHubReleaseTask);
BuildParameters.Tasks.PublishReleasePackagesTask.IsDependentOn(BuildParameters.Tasks.PublishGitHubReleaseTask);

Build.RunDotNetCore();
