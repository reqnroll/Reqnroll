namespace Reqnroll.Generator.Interfaces;

public class ProjectSettings
{
    /// <summary>
    /// The name of the project. Mandatory.
    /// </summary>
    public string ProjectName { get; set; }
    /// <summary>
    /// The assembly name of the compiled project (without .dll extension). Mandatory.
    /// </summary>
    public string AssemblyName { get; set; }
    /// <summary>
    /// The root folder of the project. Mandatory.
    /// </summary>
    public string ProjectFolder { get; set; }
    /// <summary>
    /// The default namespace of the project. Optional.
    /// </summary>
    public string DefaultNamespace { get; set; }
    /// <summary>
    /// The platform settings of the project. Mandatory.
    /// </summary>
    public ProjectPlatformSettings ProjectPlatformSettings { get; set; } = new();

    /// <summary>
    /// The reference of the unparsed Reqnroll configuration of the project. Mandatory.
    /// </summary>
    public ReqnrollConfigurationHolder ConfigurationHolder { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether feature files will be embedded as resources in the output assembly. Optional.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, feature files will be included in the compiled assembly
    /// as embedded resources by the build system. This flag informs the code-behind generator to expect that, but it is 
    /// not the responsibility of the generator to perform the embedding.</remarks>
    public bool FeatureFilesEmbedded { get; set; }
}