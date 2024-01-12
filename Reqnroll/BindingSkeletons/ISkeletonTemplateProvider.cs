namespace Reqnroll.BindingSkeletons
{
    public interface ISkeletonTemplateProvider
    {
        string GetStepDefinitionTemplate(ProgrammingLanguage language, bool withExpression);
        string GetStepDefinitionClassTemplate(ProgrammingLanguage language);
    }
}