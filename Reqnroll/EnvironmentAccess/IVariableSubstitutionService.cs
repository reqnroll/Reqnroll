namespace Reqnroll.EnvironmentAccess;

public interface IVariableSubstitutionService
{
    string ResolveTemplatePlaceholders(string template);
}
