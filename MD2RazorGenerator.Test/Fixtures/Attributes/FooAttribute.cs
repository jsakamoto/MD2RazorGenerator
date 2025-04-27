namespace MD2RazorGenerator.Test.Fixtures.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
sealed class FooAttribute : Attribute
{
    public string Text { get; private set; }

    public FooAttribute(string text) => this.Text = text;
}
