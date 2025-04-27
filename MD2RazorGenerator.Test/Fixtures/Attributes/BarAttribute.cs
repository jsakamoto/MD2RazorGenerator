namespace MD2RazorGenerator.Test.Fixtures.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
sealed class BarAttribute : Attribute
{
    public string Text { get; private set; }

    public BarAttribute(string text) => this.Text = text;
}
