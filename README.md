# Analyzing source code using Roslyn

Roslyn is much more than just a compiler; it also has rich code analysis functionality. In this session, we’ll show how we used Roslyn to automatically analyze the C# solutions submitted by students on exercism.io. We’ll use Roslyn to parse source code, transform that source code to a normalized format, and check the code for patterns. Roslyn will also be used to compile source code to an in-memory assembly and run it against a predefined test suite.

After this session you’ll know what Roslyn syntax trees are, how to parse and transform source code using Roslyn. As a bonus, you’ll know how to convert a Roslyn source code to an assembly and see how to run xUnit tests in-memory.

