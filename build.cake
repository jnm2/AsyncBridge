using System.Diagnostics;

var configuration = Argument("configuration", "Release");

var packDir = Directory("pub");

MSBuildSettings CreateMSBuildSettings(string target) => new MSBuildSettings()
    .UseToolVersion(MSBuildToolVersion.VS2017)
    .SetConfiguration(configuration)
    .WithTarget(target);

Task("Clean")
    .Does(() => MSBuild(".", CreateMSBuildSettings("Clean")));

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => MSBuild(".", CreateMSBuildSettings("Restore")));

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => MSBuild(".", CreateMSBuildSettings("Build")));

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        VSTest(
            from (string name, string framework) project in new[]
            {
                ("AsyncBridge.Net35.Tests", "net35"),
                ("AsyncBridge.Tests", "net40"),
                ("AsyncTargetingPack.Tests", "net40"),
                ("ReferenceAsync.Net45", "net45")
            }
            select File($"tests/{project.name}/bin/{configuration}/{project.framework}/{project.name}.dll").Path,
            FixToolPath(new VSTestSettings
            {
                Parallel = true
            }));

        VSTestSettings FixToolPath(VSTestSettings settings)
        {
            #tool vswhere
            settings.ToolPath =
                VSWhereLatest(new VSWhereLatestSettings { Requires = "Microsoft.VisualStudio.PackageGroup.TestTools.Core" })
                .CombineWithFilePath(File(@"Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"));
            return settings;
        }
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
    {
        foreach (var (project, settingsCustomizer) in new (string, Action<NuGetPackSettings>)[]
        {
            ("AsyncBridge", s =>
            {
                s.Tags = s.Tags.Concat(new[] { ".NET40" }).ToList();
            }),
            ("AsyncBridge.Net35", s =>
            {
                s.Tags = s.Tags.Concat(new[] { ".NET35" }).ToList();
                s.Dependencies = new[] { new NuSpecDependency { Id = "TaskParallelLibrary", Version = "1.0.2856" } };
            }),
            ("AsyncBridge.Portable", s =>
            {
                s.Tags = s.Tags.Concat(new[] { "portable", "Silverlight", ".NET40" }).ToList();
            }),
        })
        {
            var binDir = Directory($"src/{project}/bin/{configuration}");
            var versionInfo = FileVersionInfo.GetVersionInfo(binDir + File($"{project}.dll"));

            EnsureDirectoryExists(packDir);
            var settings = new NuGetPackSettings
            {
                Id = project,
                Version = versionInfo.ProductVersion,
                Title = versionInfo.ProductName,
                Authors = new[] { versionInfo.CompanyName },
                Description = versionInfo.Comments,
                Copyright = versionInfo.LegalCopyright,
                Tags = new[] { "async", "bridge", "C#", "C#5" },
                Files = new[]
                {
                    new NuSpecContent { Source = File($"{project}.dll"), Target = "lib" },
                    new NuSpecContent { Source = File($"{project}.xml"), Target = "lib" }
                },
                ProjectUrl = new Uri("https://omermor.github.com/AsyncBridge/"),
                LicenseUrl = new Uri("https://github.com/OmerMor/AsyncBridge/blob/master/LICENSE.md"),
                BasePath = binDir,
                OutputDirectory = packDir
            };
            settingsCustomizer?.Invoke(settings);
            NuGetPack(settings);
        }
    });

RunTarget(Argument("target", "Test"));
