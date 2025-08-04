namespace Reqnroll;

public enum ExpressionType
{
    /// <summary>
    /// Detect Automatically if the expression is a Regular expression (Regex) or a Cucumber expression
    /// </summary>
    Automatic,
    /// <summary>
    /// The expression is a Cucumber Expression
    /// </summary>
    CucumberExpression,
    /// <summary>
    /// The expression is a Regular expression (Regex)
    /// </summary>
    RegularExpression
}
