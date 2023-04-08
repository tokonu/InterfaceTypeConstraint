using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace InterfaceConstraintAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking")]
    public class Analyzer : DiagnosticAnalyzer
    {
        private const string AttributeDisplayName = "InterfaceConstraintAnalyzer.OnlyAllowInterfaceCallsAttribute";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "ICG001",
            "Enforce interface generic type argument",
            "The generic type parameter must be an interface",
            "TypeConstraints",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor GenericMethodRule = new DiagnosticDescriptor(
            id: "ICG002",
            title: "OnlyAllowInterfaceCallsAttribute must be used with generic methods having an interface type constraint",
            messageFormat: "Method '{0}' must be generic and have an interface type constraint",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule, GenericMethodRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, invocationExpr).Symbol as IMethodSymbol;

            if (methodSymbol?.IsGenericMethod != true)
                return;

            if (HasRequiredAttribute(methodSymbol) == false)
                return;

            var enclosingSymbol = context.SemanticModel.GetEnclosingSymbol(invocationExpr.SpanStart) as IMethodSymbol;
            if (HasRequiredAttribute(enclosingSymbol))
                return;

            var typeArgument = methodSymbol.TypeArguments[0];
            if (typeArgument.TypeKind != TypeKind.Interface)
            {
                var diagnostic = Diagnostic.Create(Rule, invocationExpr.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (HasRequiredAttribute(methodSymbol))
            {
                if (!methodSymbol.IsGenericMethod || !methodSymbol.TypeParameters[0].ConstraintTypes
                        .Any(type => type.TypeKind == TypeKind.Interface))
                {
                    var diagnostic = Diagnostic.Create(GenericMethodRule, methodSymbol.Locations[0], methodSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool HasRequiredAttribute(ISymbol symbol)
        {
            return symbol != null &&
                   symbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == AttributeDisplayName);
        }
    }
}