DCG (Dynamic Code Generator)

Release 3.3.100
Last revised Dec 29, 2008

This software is Wei Yuan (Cavingdeep) Free Software.

TABLE OF CONTENTS

    1. Introduction
    2. System Requirements
    3. Software Features
    4. Usage Information
    5. Known Issues
    6. Change Log

1. Introduction

    DCG is a template based code generator engine. It consists in
    an engine and a template language syntax.

    It could be used in any plain text generation, for example, a DCG
    template can be an e-mail template, a SQL script template or even
    a C# source code template.

    It offers a variety of templating characteristics to ease template
    writting and provide a lot of rich templating features such as
    multi-output template, etc.

2. System Requirements

    * .NET Framework 2.0 or higher

3. Software Features

    * Single output
    * Multi output
    * Completely support C# as dynamic code
    * Completely integrated with .NET Framework
    * Dynamic source code generation
    * Dynamic assembly generation
    * Static and dynamic context concept support
    * Total control of output formatting guaranteed
    * Easy to use template debugging feature
    * Rich templating features

4. Usage Information

    Copy Dcg.dll and Dcg.xml to your own project and then just reference it.
    See examples and syntax document for quick learning.

5. Known Issues

    1. All header directives must appear prior to any body directive,
       otherwise rare exceptions may occur.

6. Change Log
    v3.3.100
        * Bug fixes.

    v3.3.90
        * A new parsing structure is employed to ensure future
          Dcg extensibility.
        * Support for .NET Framework 3.5 compiler added.
        * Private template sections functionality implemented.
        * AtTemplateProxy AppDomain base directory bug fix.
        * Possible output key escape problem fixed.
        * Other fixes.

    v3.2.65
        * Parameter parsing bug fix.
        * Assembly name change.

    v3.2.62
        * Bug fix.

    v3.2.61
        * Templates can have multiple runtime instances now. This means that
          global members will not be shared if you use a new instance.

    v3.1.55
        * Multi-Output block enhancement, now Multi-Output blocks can be
          nested.

    v3.1.35
        * Cross domain proxy added.
        * Resource cleaning has been put inside the assembly.

    v3.0.20 (RTW)
        * Enhancements.

    v3.0.18 (beta3)
        * Minor bug fixes.

    v3.0.9 (beta2)
        * Bug fixes.
        * CallTemplate function added.
        * Multi-line evaluation directive added.
        * Between directive added.

    v3.0.0 (beta1)
        * At template language
        * Completely new line based parsing engine

    v2.x.x

    v1.x.x

