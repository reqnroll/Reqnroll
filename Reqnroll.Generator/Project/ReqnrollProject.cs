using System;
using System.Collections.Generic;
using System.IO;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Utils;

namespace Reqnroll.Generator.Project
{
    public class ReqnrollProject
    {
        public ProjectSettings ProjectSettings { get; set; }
        public ReqnrollProjectConfiguration Configuration { get; set; }

        public ReqnrollProject()
        {
            ProjectSettings = new ProjectSettings();
            Configuration = new ReqnrollProjectConfiguration();
        }
    }
}