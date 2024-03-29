namespace Reqnroll
{
    using Bindings;

    public class StepInfo
    {
        public StepDefinitionType StepDefinitionType { get; private set; }

        public string Text { get; private set; }
        public Table Table { get; private set; }
        public string MultilineText { get; private set; }
        public BindingMatch BindingMatch { get; set; }
        public StepInstance StepInstance { get; set; }


        public StepInfo(StepDefinitionType stepDefinitionType, string text, Table table, string multilineText)
        {
            StepDefinitionType = stepDefinitionType;
            Text = text;
            Table = table;
            MultilineText = multilineText;
        }


    }
}