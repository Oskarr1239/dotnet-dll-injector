dotnet-dll-injector
============

Tool for injecting managed .NET DLL libraries into native process (or not native with some limitations). 
Support both x86 and x64. Tested on v2.0.50727 and v4.0.30319 runtimes. For loading runtime was 
used interface marked as obsolete from 4.0 and later.

**This project is based on original [NDLLInjector](https://github.com/fday/NDllInjector) by [fday](https://github.com/fday).** 

This project gives you Java JAR files which are wrappers for Fday's .NET files. Therefore you can use is
to execute Java code which will inject specific DLL into other managed process.

You can download precompiled binaries [here](http://repo.sarxos.pl/maven2/com/github/sarxos/dotnet-dll-injector/0.1/dotnet-dll-injector-0.1-dist.zip). 

## How To Use

Use this code to inject DLL into any process:

```java
// grab process ID
int pid = InjectorUtils.getProcessID("some-managed-process.exe");

// specify which DLL file should be injected (can be relative or absolute path) 
File dll = new File("path/to/file.dll");

// specify signature of method to be run
String signature = "TestNamespace.Program.Main"; 

// inject!
Injector.getInstance().inject(pid, file, signature);
```

The DLL file which you want to inject has to define method with the following signature:

```cs
public static int MethodNameHere(string arg) {
	// code
}
```

For example:

```cs
namespace TestNamespace {
    class Program {
        public static int Main(string arg) {
            MessageBox.Show("Hello World from Injectee!");
            return 0;
        }
    }
}
```

It's important to note that if you want to read / write to the process memory classes / objects, 
the DLL file should be build with the same framework version as the process into which you want
to inject it.

If you do not want to mess with process runtime, then you can use any framework you need.


## How To Build

If everything is configured, then it's enough to run:

```
mvn clean install
```

Follow the next points to learn how to configure build if your environment has not been configured.


### Configure .NET

Sonar _maven-dotnet-plugin_ is not able to take parameters for MSBuild, so you have to change all ```*.csproj```
files from the project top reflect correct path to .NET framework home directory. I've already created ticket 
for this problem in Sonar JIRA - [SONARPLUGINS-2133](http://jira.codehaus.org/browse/SONARPLUGINS-2133). 
Because of that you have to edit each ```*.csproj``` file and align this path to point existing file:

```xml
<Import Project="C:\WINNT\Microsoft.NET\Framework\v2.0.50727\Microsoft.CSharp.targets" />
```

### Configure FASM

Download flat assembler ([flatassembler.net](http://flatassembler.net)), extract it wherever you want, and set
```FASM_HOME``` environment variable to point this location.

This step will become obsolete with upcoming fasm-compiler-plugin 0.2 release, but for now FASM_HOME has to be set. 
