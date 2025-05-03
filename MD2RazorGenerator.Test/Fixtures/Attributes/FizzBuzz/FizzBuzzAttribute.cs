namespace MD2RazorGenerator.Test.Fixtures.Attributes.FizzBuzz;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
sealed class FizzBuzzAttribute : Attribute
{
    public string Text { get; private set; }

    public FizzBuzzAttribute(string text) => this.Text = text;
}
