using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspNetDeploy.Model;
using AspNetDeploy.Projects.Contracts;
using Guids;

namespace AspNetDeploy.Projects
{
    public class SourceFilesParser : IProjectParser
    {
        private readonly string sourcesFolder;
        private IList<SourceFilesProject> parsedProjects;

        public SourceFilesParser(string sourcesFolder)
        {
            this.sourcesFolder = sourcesFolder;
        }

        public void LoadProjects()
        {
            if (!Directory.Exists(this.sourcesFolder))
            {
                this.parsedProjects = new List<SourceFilesProject>();
                return;
            }

            string normalizedSourcesFolder = this.NormalizeSourcesFolder();

            DirectoryInfo rootDirectory = new DirectoryInfo(this.sourcesFolder);
            string rootFullName = rootDirectory.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            List<SourceFilesProject> projects = Directory
                .GetDirectories(this.sourcesFolder, "*", SearchOption.TopDirectoryOnly)
                .Select(path => new DirectoryInfo(path))
                .Where(directoryInfo => !directoryInfo.Name.StartsWith(".", StringComparison.Ordinal))
                .Select(directoryInfo =>
                {
                    string fullName = directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string relativePath = fullName.StartsWith(normalizedSourcesFolder, StringComparison.OrdinalIgnoreCase)
                        ? fullName.Substring(normalizedSourcesFolder.Length)
                        : directoryInfo.Name;

                    return new SourceFilesProject
                    {
                        DirectoryPath = fullName,
                        RelativePath = relativePath,
                        Name = directoryInfo.Name,
                        Guid = GuidUtility.Create(GuidUtility.UrlNamespace, relativePath.Replace(Path.DirectorySeparatorChar, '/'))
                    };
                })
                .ToList();

            projects.Insert(0, new SourceFilesProject
            {
                DirectoryPath = rootFullName,
                RelativePath = string.Empty,
                Name = string.IsNullOrEmpty(rootDirectory.Name) ? rootFullName : rootDirectory.Name,
                Guid = GuidUtility.Create(GuidUtility.UrlNamespace, rootFullName.Replace(Path.DirectorySeparatorChar, '/'))
            });

            this.parsedProjects = projects;
        }

        public IList<Guid> ListProjectGuids()
        {
            return this.parsedProjects.Select(project => project.Guid).ToList();
        }

        public bool IsExists(Guid guid)
        {
            return this.parsedProjects.Any(project => project.Guid == guid);
        }

        public void UpdateProject(Project project, Guid guid)
        {
            SourceFilesProject parsedProject = this.parsedProjects.FirstOrDefault(p => p.Guid == guid);

            if (parsedProject == null)
            {
                return;
            }

            project.Name = parsedProject.Name;
        }

        public void UpdateProjectVersion(ProjectVersion projectVersion, Guid guid)
        {
            SourceFilesProject parsedProject = this.parsedProjects.FirstOrDefault(p => p.Guid == guid);

            if (parsedProject == null)
            {
                return;
            }

            projectVersion.Name = parsedProject.Name;
            projectVersion.ProjectFile = parsedProject.RelativePath;
            projectVersion.ProjectType = ProjectType.SourceFiles;
            projectVersion.SolutionFile = string.Empty;
        }

        private string NormalizeSourcesFolder()
        {
            return this.sourcesFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                   Path.DirectorySeparatorChar;
        }

        private class SourceFilesProject
        {
            public string DirectoryPath { get; set; }

            public string RelativePath { get; set; }

            public string Name { get; set; }

            public Guid Guid { get; set; }
        }
    }
}
