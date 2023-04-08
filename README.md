# InterfaceTypeConstraint

## Why

In c# there isn't a way to have "only interface" generic type constraint.

## How

A custom Roslyn Analyzer to enforce interface constraint on methods that are marked with [OnlyAllowInterfaceCalls]

## Usage

    public interface IFoo {}

    public class Foo: IFoo {}

    public class Bar {
        [OnlyAllowInterfaceCalls]
        public void DoSomething<T>() where T : IFoo
        {

        }
    }

    bar.DoSomething<IFoo>(); // works
    bar.DoSomething<Foo>(); // compile error