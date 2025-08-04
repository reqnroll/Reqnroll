namespace Reqnroll;

public enum ExpressionType
{
    /// <summary>
    /// Unspecified, the expression is a Regular expression (Regex) or a Cucumber expression. The expression type will be determined at runtime based on the expression string.
    /// </summary>
    Unspecified,
    /// <summary>
    /// The expression is a Cucumber Expression
    /// </summary>
    CucumberExpression,
    /// <summary>
    /// The expression is a Regular expression (Regex)
    /// </summary>
    RegularExpression
}
