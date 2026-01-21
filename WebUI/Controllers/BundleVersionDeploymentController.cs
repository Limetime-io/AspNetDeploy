using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AspNetDeploy.Contracts;
using AspNetDeploy.Contracts.Exceptions;
using AspNetDeploy.Model;
using AspNetDeploy.WebUI.Models;
using AspNetDeploy.WebUI.Models.DeploymentSteps;
using MvcSiteMapProvider.Linq;
using Newtonsoft.Json;

namespace AspNetDeploy.WebUI.Controllers
{
    public class BundleVersionDeploymentController : AuthorizedAccessController
    {
        public BundleVersionDeploymentController(ILoggingService loggingService) : base(loggingService)
        {
        }

        public ActionResult MoveUp(int bundleVersionId, int deploymentStepId)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            BundleVersion bundleVersion = this.Entities.BundleVersion
                .Include("DeploymentSteps")
                .First(bv => bv.Id == bundleVersionId);

            List<DeploymentStep> deploymentSteps = bundleVersion.DeploymentSteps.OrderBy( ds => ds.OrderIndex).ToList();
            DeploymentStep deploymentStep = deploymentSteps.First( ds => ds.Id == deploymentStepId);

            int index = deploymentSteps.IndexOf(deploymentStep);

            if (index > 0)
            {
                deploymentSteps[index - 1].OrderIndex ++;
                deploymentSteps[index].OrderIndex --;
            }

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = bundleVersionId});
        }
        public ActionResult MoveDown(int bundleVersionId, int deploymentStepId)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            BundleVersion bundleVersion = this.Entities.BundleVersion
                .Include("DeploymentSteps")
                .First(bv => bv.Id == bundleVersionId);

            List<DeploymentStep> deploymentSteps = bundleVersion.DeploymentSteps.OrderBy(ds => ds.OrderIndex).ToList();
            DeploymentStep deploymentStep = deploymentSteps.First(ds => ds.Id == deploymentStepId);

            int index = deploymentSteps.IndexOf(deploymentStep);

            if (index < deploymentSteps.Count - 1)
            {
                deploymentSteps[index + 1].OrderIndex--;
                deploymentSteps[index].OrderIndex++;
            }

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = bundleVersionId});
        }

        public ActionResult AddStep(int id, DeploymentStepType deploymentStepType = DeploymentStepType.Undefined)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            BundleVersion bundleVersion = this.Entities.BundleVersion
                .Include("DeploymentSteps")
                .First(bv => bv.Id == id);

            List<MachineRole> machineRoles = this.Entities.MachineRole.ToList();

            this.ViewBag.BundleVersion = bundleVersion;
            this.ViewBag.MachineRoles = machineRoles;

            if (deploymentStepType == DeploymentStepType.Undefined)
            {
                return this.View("AddStep");
            }

            this.ViewBag.DeploymentStep = new DeploymentStep();

            if (deploymentStepType == DeploymentStepType.DeployWebSite)
            {
                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.Web) && !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                WebSiteDeploymentStepModel model = new WebSiteDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditWebsiteStep", model);
            }

            if (deploymentStepType == DeploymentStepType.DeployDacpac)
            {
                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.Database) && !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                DacpacDeploymentStepModel model = new DacpacDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditDacpacStep", model);
            }

            if (deploymentStepType == DeploymentStepType.CopyFiles)
            {
                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.ZipArchive) || pv.ProjectType.HasFlag(ProjectType.GulpFile))
                    .Where(pv => !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                ZipArchiveDeploymentStepModel model = new ZipArchiveDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditZipArchiveStep", model);
            }

            if (deploymentStepType == DeploymentStepType.DeploySourceFiles)
            {
                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.SourceFiles))
                    .Where(pv => !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                SourceFilesDeploymentStepModel model = new SourceFilesDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditSourceFilesStep", model);
            }

            if (deploymentStepType == DeploymentStepType.UpdateHostsFile)
            {
                HostsDeploymentStepModel model = new HostsDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditHostsStep", model);
            }

            if (deploymentStepType == DeploymentStepType.RunSQLScript)
            {
                SqlScriptDeploymentStepModel model = new SqlScriptDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditSqlScriptStep", model);
            }

            if (deploymentStepType == DeploymentStepType.Configuration)
            {
                ConfigDeploymentStepModel model = new ConfigDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditConfigStep", model);
            }

            if (deploymentStepType == DeploymentStepType.RunVsTests)
            {
                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                        .SelectMany(scv => scv.ProjectVersions)
                        .Where(pv => pv.ProjectType.HasFlag(ProjectType.Test) && !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                        .Select(pv => new SelectListItem
                        {
                            Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                            Value = pv.Id.ToString()
                        })
                        .OrderBy(sli => sli.Text)
                        .ToList();

                RunVsTestStepModel model = new RunVsTestStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                return this.View("EditRunTestStep", model);
            }

            if (deploymentStepType == DeploymentStepType.DeployContainer)
            {
                this.PopulateContainerProjectsSelect(false);

                ContainerDeploymentStepModel model = new ContainerDeploymentStepModel
                {
                    BundleVersionId = bundleVersion.Id
                };

                this.PopulateContainerSelectLists(model);

                return this.View("EditContainerStep", model);
            }
            
            throw new AspNetDeployException("Invalid deployment step type");
        }

        public ActionResult EditStep(int id, int deploymentStepId)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            DeploymentStep deploymentStep = this.Entities.DeploymentStep
                .Include("Properties")
                .Include("MachineRoles")
                .Include("BundleVersion.Bundle")
                .First( ds => ds.Id == deploymentStepId && ds.BundleVersion.Id == id);

            List<MachineRole> machineRoles = this.Entities.MachineRole.ToList();

            this.ViewBag.DeploymentStep = deploymentStep;
            this.ViewBag.BundleVersion = deploymentStep.BundleVersion;
            this.ViewBag.MachineRoles = machineRoles;

            if (deploymentStep.Type == DeploymentStepType.DeployWebSite)
            {
                WebSiteDeploymentStepModel model = new WebSiteDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    DeploymentStepId = deploymentStepId,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    SiteName = deploymentStep.GetStringProperty("IIS.SiteName"),
                    ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                    Destination = deploymentStep.GetStringProperty("IIS.DestinationPath"),
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select( mr => mr.Name)),
                    BindingsJson = deploymentStep.GetStringProperty("IIS.Bindings")
                };

                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.Web) && !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                return this.View("EditWebsiteStep", model);
            }
            if (deploymentStep.Type == DeploymentStepType.CopyFiles)
            {
                ZipArchiveDeploymentStepModel model = new ZipArchiveDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    DeploymentStepId = deploymentStepId,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                    Destination = deploymentStep.GetStringProperty("DestinationPath"),
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select( mr => mr.Name)),
                    CustomConfigurationJson = deploymentStep.GetStringProperty("CustomConfiguration")
                };

                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.ZipArchive) || pv.ProjectType.HasFlag(ProjectType.GulpFile))
                    .Where(pv => !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                return this.View("EditZipArchiveStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.DeploySourceFiles)
            {
                SourceFilesDeploymentStepModel model = new SourceFilesDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    DeploymentStepId = deploymentStepId,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                    Destination = deploymentStep.GetStringProperty("DestinationPath"),
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                    CustomConfigurationJson = deploymentStep.GetStringProperty("CustomConfiguration")
                };

                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.SourceFiles))
                    .Where(pv => !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                return this.View("EditSourceFilesStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.Configuration)
            {
                ConfigDeploymentStepModel model = new ConfigDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    DeploymentStepId = deploymentStepId,
                    ConfigJson = deploymentStep.GetStringProperty("SetValues"),
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    File = deploymentStep.GetStringProperty("File"),
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name))
                };

                return this.View("EditConfigStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.RunSQLScript)
            {
                SqlScriptDeploymentStepModel model = new SqlScriptDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    DeploymentStepId = deploymentStepId,
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                    ConnectionString = deploymentStep.GetStringProperty("ConnectionString"),
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    Command = deploymentStep.GetStringProperty("Command")
                };

                return this.View("EditSqlScriptStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.UpdateHostsFile)
            {
                HostsDeploymentStepModel model = new HostsDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    DeploymentStepId = deploymentStepId,
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    ConfigJson = deploymentStep.GetStringProperty("ConfigurationJson")
                };

                return this.View("EditHostsStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.DeployDacpac)
            {
                DacpacDeploymentStepModel model = new DacpacDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    DeploymentStepId = deploymentStepId,
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                    ConnectionString = deploymentStep.GetStringProperty("ConnectionString"),
                    TargetDatabase = deploymentStep.GetStringProperty("TargetDatabase"),
                    CustomConfiguration = deploymentStep.GetStringProperty("CustomConfiguration")
                };

                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.Database) && !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                return this.View("EditDacpacStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.RunVsTests)
            {
                RunVsTestStepModel model = new RunVsTestStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    DeploymentStepId = deploymentStepId,
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                    FiltersJson = deploymentStep.GetStringProperty("FiltersJson"),
                    StopOnFailure = deploymentStep.GetBoolProperty("StopOnFailure")
                };

                this.ViewBag.ProjectsSelect = this.Entities.SourceControlVersion
                    .SelectMany(scv => scv.ProjectVersions)
                    .Where(pv => pv.ProjectType.HasFlag(ProjectType.Test) && !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"))
                    .Select(pv => new SelectListItem
                    {
                        Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                        Value = pv.Id.ToString()
                    })
                    .OrderBy(sli => sli.Text)
                    .ToList();

                return this.View("EditRunTestStep", model);
            }

            if (deploymentStep.Type == DeploymentStepType.DeployContainer)
            {
                ContainerDeploymentStepModel model = new ContainerDeploymentStepModel
                {
                    OrderIndex = deploymentStep.OrderIndex,
                    BundleVersionId = deploymentStep.BundleVersionId,
                    DeploymentStepId = deploymentStepId,
                    Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                    StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                    ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                    ContainerName = deploymentStep.GetStringProperty("Container.Name"),
                    Ports = deploymentStep.GetStringProperty("Container.Ports"),
                    EnvironmentVariables = deploymentStep.GetStringProperty("Container.EnvironmentVariables"),
                    Labels = deploymentStep.GetStringProperty("Container.Labels"),
                    Volumes = deploymentStep.GetStringProperty("Container.Volumes"),
                    RestartPolicy = deploymentStep.GetStringProperty("Container.RestartPolicy"),
                    Networks = deploymentStep.GetStringProperty("Container.Networks"),
                    Platform = (NetCorePlatform)deploymentStep.GetIntProperty("Container.Platform"),
                    Architecture = (NetCoreArchitecture)deploymentStep.GetIntProperty("Container.Architecture")
                };

                this.PopulateContainerProjectsSelect(true);

                this.PopulateContainerSelectLists(model);

                return this.View("EditContainerStep", model);
            }

            return this.Content("Unsupported step type");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveConfigStep(ConfigDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditConfigStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.Configuration);

            deploymentStep.SetStringProperty("SetValues", model.ConfigJson);
            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("File", model.File);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveHostsStep(HostsDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditHostsStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.UpdateHostsFile);

            deploymentStep.SetStringProperty("ConfigurationJson", model.ConfigJson);
            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSQLStep(SqlScriptDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditSqlScriptStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.RunSQLScript);

            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("Command", model.Command);
            deploymentStep.SetStringProperty("ConnectionString", model.ConnectionString);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWebSiteStep(WebSiteDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditWebsiteStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.DeployWebSite);

            deploymentStep.SetStringProperty("IIS.SiteName", model.SiteName);
            deploymentStep.SetStringProperty("IIS.DestinationPath", model.Destination);
            deploymentStep.SetStringProperty("IIS.Bindings", model.BindingsJson);
            deploymentStep.SetStringProperty("ProjectId", model.ProjectId.ToString());

            this.UpdateProjectReference(model);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDacpacStep(DacpacDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditDacpacStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.DeployDacpac);

            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("ConnectionString", model.ConnectionString);
            deploymentStep.SetStringProperty("TargetDatabase", model.TargetDatabase);
            deploymentStep.SetStringProperty("CustomConfiguration", model.CustomConfiguration);
            deploymentStep.SetStringProperty("ProjectId", model.ProjectId.ToString());

            this.UpdateProjectReference(model);
            this.SaveRoles(model, deploymentStep);
            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveZipArchiveStep(ZipArchiveDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditZipArchiveStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.CopyFiles);

            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("DestinationPath", model.Destination);
            deploymentStep.SetStringProperty("CustomConfiguration", model.CustomConfigurationJson);
            deploymentStep.SetStringProperty("ProjectId", model.ProjectId.ToString(CultureInfo.InvariantCulture));

            this.UpdateProjectReference(model);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSourceFilesStep(SourceFilesDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditSourceFilesStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.DeploySourceFiles);

            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("DestinationPath", model.Destination);
            deploymentStep.SetStringProperty("CustomConfiguration", model.CustomConfigurationJson);
            deploymentStep.SetStringProperty("ProjectId", model.ProjectId.ToString(CultureInfo.InvariantCulture));

            this.UpdateProjectReference(model);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new { id = deploymentStep.BundleVersionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRunVsTestsStep(RunVsTestStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                return this.View("EditRunTestStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.RunVsTests);

            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("FiltersJson", model.FiltersJson);
            deploymentStep.SetStringProperty("StopOnFailure", model.StopOnFailure.ToString());
            deploymentStep.SetStringProperty("ProjectId", model.ProjectId.ToString(CultureInfo.InvariantCulture));

            this.UpdateProjectReference(model);

            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveContainerStep(ContainerDeploymentStepModel model)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            if (!this.ModelState.IsValid)
            {
                BundleVersion bundleVersion = this.Entities.BundleVersion
                    .Include("Bundle")
                    .First(bv => bv.Id == model.BundleVersionId);
                this.ViewBag.BundleVersion = bundleVersion;
                this.ViewBag.MachineRoles = this.Entities.MachineRole.ToList();
                this.PopulateContainerProjectsSelect(model.DeploymentStepId > 0);
                this.PopulateContainerSelectLists(model);
                return this.View("EditContainerStep", model);
            }

            DeploymentStep deploymentStep = this.GetDeploymentStep(model, DeploymentStepType.DeployContainer);

            deploymentStep.SetStringProperty("Step.Title", model.StepTitle);
            deploymentStep.SetStringProperty("Container.Name", model.ContainerName);
            deploymentStep.SetStringProperty("Container.Ports", model.Ports);
            deploymentStep.SetStringProperty("Container.EnvironmentVariables", model.EnvironmentVariables);
            deploymentStep.SetStringProperty("Container.Labels", model.Labels);
            deploymentStep.SetStringProperty("Container.Volumes", model.Volumes);
            deploymentStep.SetStringProperty("Container.RestartPolicy", model.RestartPolicy);
            deploymentStep.SetStringProperty("Container.Networks", model.Networks);
            deploymentStep.SetStringProperty("Container.Platform", ((int)model.Platform).ToString(CultureInfo.InvariantCulture));
            deploymentStep.SetStringProperty("Container.Architecture", ((int)model.Architecture).ToString(CultureInfo.InvariantCulture));
            deploymentStep.SetStringProperty("ProjectId", model.ProjectId.ToString(CultureInfo.InvariantCulture));

            this.UpdateProjectReference(model);
            this.SaveRoles(model, deploymentStep);

            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new { id = deploymentStep.BundleVersionId });
        }

        public ActionResult DeleteStep(int id, int deploymentStepId)
        {
            this.CheckPermission(UserRoleAction.DeploymentChangeSteps);

            DeploymentStep deploymentStep = this.Entities.DeploymentStep
                .Include("Properties")
                .Include("MachineRoles")
                .Include("BundleVersion")
                .First(ds => ds.BundleVersionId == id && ds.Id == deploymentStepId);

            switch (deploymentStep.Type)
            {
                    case DeploymentStepType.DeployWebSite:
                    case DeploymentStepType.DeployDacpac:
                    case DeploymentStepType.CopyFiles:
                    case DeploymentStepType.DeploySourceFiles:
                    case DeploymentStepType.RunVsTests:
                    case DeploymentStepType.DeployContainer:
                        this.UpdateProjectReference(new ProjectRelatedDeploymentStepModel
                        {
                            BundleVersionId = deploymentStep.BundleVersionId,
                            DeploymentStepId = deploymentStep.Id,
                            ProjectId = 0
                        });
                        break;
            }

            deploymentStep.MachineRoles.Clear();

            this.Entities.DeploymentStep.Remove(deploymentStep);
            this.Entities.SaveChanges();

            return this.RedirectToAction("VersionDeployment", "Bundles", new {id = deploymentStep.BundleVersionId});
        }

        private void SaveRoles(DeploymentStepModel model, DeploymentStep deploymentStep)
        {
            List<MachineRole> machineRoles = this.Entities.MachineRole.ToList();

            deploymentStep.MachineRoles.Clear();

            if (string.IsNullOrWhiteSpace(model.Roles))
            {
                return;
            }

            foreach (string role in model.Roles.ToLowerInvariant().Split(',').Select(r => r.Trim()))
            {
                MachineRole machineRole = machineRoles.FirstOrDefault(mr => mr.Name.ToLowerInvariant() == role);

                if (machineRole != null)
                {
                    deploymentStep.MachineRoles.Add(machineRole);
                }
            }
        }

        private DeploymentStep GetDeploymentStep(DeploymentStepModel model, DeploymentStepType deploymentStepType)
        {
            DeploymentStep deploymentStep;

            if (model.DeploymentStepId == 0)
            {
                deploymentStep = new DeploymentStep();
                deploymentStep.Type = deploymentStepType;
                deploymentStep.BundleVersionId = model.BundleVersionId;
                deploymentStep.OrderIndex = this.Entities.DeploymentStep.Count(ds => ds.BundleVersionId == model.BundleVersionId) + 1;
                this.Entities.DeploymentStep.Add(deploymentStep);
            }
            else
            {
                deploymentStep = this.Entities.DeploymentStep
                    .Include("Properties")
                    .First(ds => ds.Id == model.DeploymentStepId);
            }

            return deploymentStep;
        }

        private void UpdateProjectReference(ProjectRelatedDeploymentStepModel model)
        {
            BundleVersion bundleVersion = this.Entities.BundleVersion
                .Include("ProjectVersionTobundleVersion.ProjectVersion")
                .Include("DeploymentSteps.Properties")
                .First(bv => bv.Id == model.BundleVersionId);

            foreach (ProjectVersionToBundleVersion link in bundleVersion.ProjectVersionToBundleVersion.ToList())
            {
                bool isContainerLink = link.PackagerId.HasValue && link.PackagerId.Value == (int)PackagerType.Docker;

                if (isContainerLink)
                {
                    // For container links, check if there's a matching step with same ProjectId AND ConfigurationJson
                    bool hasMatchingContainerStep = bundleVersion.DeploymentSteps.Any(ds =>
                    {
                        if (ds.Type != DeploymentStepType.DeployContainer)
                            return false;

                        if (ds.GetIntProperty("ProjectId") != link.ProjectVersionId)
                            return false;

                        // Get Platform and Architecture from deployment step properties
                        int platform = ds.GetIntProperty("Container.Platform");
                        int architecture = ds.GetIntProperty("Container.Architecture");

                        // Create config from step properties
                        NetCoreProjectBundleConfig stepConfig = new NetCoreProjectBundleConfig
                        {
                            Version = 1,
                            Type = ProjectBundleConfigType.NetCore,
                            Platform = (NetCorePlatform)platform,
                            Architecture = (NetCoreArchitecture)architecture,
                            OutputType = NetCoreOutputType.DockerContainer
                        };

                        string stepConfigJson = JsonConvert.SerializeObject(stepConfig);

                        return stepConfigJson == link.ConfigurationJson;
                    });

                    if (!hasMatchingContainerStep)
                    {
                        this.Entities.ProjectVersionToBundleVersion.Remove(link);
                    }
                }
                else if (bundleVersion.DeploymentSteps.All(ds => ds.GetIntProperty("ProjectId") != link.ProjectVersionId))
                {
                    this.Entities.ProjectVersionToBundleVersion.Remove(link);
                }
            }

            if (model.ProjectId > 0) // add project
            {
                int? packagerId = model is ContainerDeploymentStepModel ? (int)PackagerType.Docker : (int?)null;
                string configurationJson = null;

                // For container models, create configuration JSON first
                if (model is ContainerDeploymentStepModel containerModel)
                {
                    NetCoreProjectBundleConfig config = new NetCoreProjectBundleConfig
                    {
                        Version = 1,
                        Type = ProjectBundleConfigType.NetCore,
                        Platform = containerModel.Platform,
                        Architecture = containerModel.Architecture,
                        OutputType = NetCoreOutputType.DockerContainer
                    };

                    configurationJson = JsonConvert.SerializeObject(config);
                }

                // Find existing link by ProjectId, PackagerId, and ConfigurationJson
                // This ensures different targets create separate entries
                ProjectVersionToBundleVersion existingLink = bundleVersion.ProjectVersionToBundleVersion
                    .FirstOrDefault(x => x.ProjectVersionId == model.ProjectId &&
                                       x.PackagerId == packagerId &&
                                       (configurationJson == null || x.ConfigurationJson == configurationJson));

                if (existingLink == null)
                {
                    existingLink = new ProjectVersionToBundleVersion
                    {
                        BundleVersion = bundleVersion,
                        ProjectVersionId = model.ProjectId,
                        PackagerId = packagerId,
                        ConfigurationJson = configurationJson
                    };

                    this.Entities.ProjectVersionToBundleVersion.Add(existingLink);
                }
            }
        }

        private void PopulateContainerSelectLists(ContainerDeploymentStepModel model)
        {
            this.ViewBag.NetCorePlatforms = Enum.GetValues(typeof(NetCorePlatform))
                .Cast<NetCorePlatform>()
                .Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = ((int)value).ToString(CultureInfo.InvariantCulture),
                    Selected = value == model.Platform
                })
                .ToList();

            this.ViewBag.NetCoreArchitectures = Enum.GetValues(typeof(NetCoreArchitecture))
                .Cast<NetCoreArchitecture>()
                .Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = ((int)value).ToString(CultureInfo.InvariantCulture),
                    Selected = value == model.Architecture
                })
                .ToList();
        }

        private void PopulateContainerProjectsSelect(bool includeTargetFrameworkFilter)
        {
            IQueryable<ProjectVersion> projectQuery = this.Entities.SourceControlVersion
                .SelectMany(scv => scv.ProjectVersions)
                .Where(pv => pv.ProjectType.HasFlag(ProjectType.Web) && pv.ProjectType.HasFlag(ProjectType.NetCore))
                .Where(pv => !pv.Project.Properties.Any(p => p.Key == "NotForDeployment" && p.Value == "true"));

            if (includeTargetFrameworkFilter)
            {
               // projectQuery = projectQuery.Where(pv => pv.Properties.Any(p => p.Key == "TargetFrameworkVersion" &&
               //     (p.Value.Contains("net8.0") || p.Value.Contains("net9.0") || p.Value.Contains("net10.0"))));
            }

            this.ViewBag.ProjectsSelect = projectQuery
                .Select(pv => new SelectListItem
                {
                    Text = pv.SourceControlVersion.SourceControl.Name + " / " + pv.SourceControlVersion.Name + " / " + pv.Name,
                    Value = pv.Id.ToString()
                })
                .OrderBy(sli => sli.Text)
                .ToList();
        }

    }
}
