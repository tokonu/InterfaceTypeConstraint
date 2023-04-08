using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace InterfaceConstraintAnalyzer
{
    [Generator]
    public class AttributeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Source code for the attribute class.
            var attributeSourceCode = @"
using System;

namespace InterfaceConstraintAnalyzer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class OnlyAllowInterfaceCallsAttribute : Attribute
    {
    }
}
";
            context.AddSource("OnlyAllowInterfaceCallsAttribute", SourceText.From(attributeSourceCode, Encoding.UTF8));
        }
    }
}