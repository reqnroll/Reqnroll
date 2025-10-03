namespace Reqnroll.BindingSkeletons
{
    public interface ISkeletonTemplateProvider
    {
        string GetStepDefinitionTemplate(ProgrammingLanguage language, bool withExpression, bool asAsync);
        string GetStepDefinitionClassTemplate(ProgrammingLanguage language);
    }
}