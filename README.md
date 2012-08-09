dotnet-dll-injector
============

Tool for injecting managed .NET DLL libraries in native process (or not native with some limitations). 
Support both x86 and x64. Tested on v2.0.50727 and v4.0.30319 runtimes. For loading runtime was 
used interface marked as obsolete from 4.0 and later.

*This project is based on original [NDLLInjector](https://github.com/fday/NDllInjector) by 
[fday](https://github.com/fday).* The only difference is that the one you are currently looking on can
be build with Maven. It can be also run with only .NET framework 2.0 version installed.

You can download precompiled binaries [here](http://repo.sarxos.pl/maven2/com/github/sarxos/dotnet-dll-injector/0.1/dotnet-dll-injector-0.1-dist.zip). 

## Maven Build

In your ```settings.xml``` create profile with parameters required by _maven-dotnet-plugin_:

```xml
<profiles>
	<profile>
		<id>dotnet</id>
		<properties>
			<dotnet.2.0.sdk.directory>C:/WINNT/Microsoft.NET/Framework/v2.0.50727</dotnet.2.0.sdk.directory> 
		</properties>
	</profile>
</profiles>
```

Paths on your computer *can be different* - check them carefully!!!

Sonar _maven-dotnet-plugin_ is not able to take parameters for MSBuild, so you have to change all ```*.csproj```
files from the project top reflect correct path to .NET framework home directory. I've already created ticket 
for this problem in Sonar JIRA - [SONARPLUGINS-2133](http://jira.codehaus.org/browse/SONARPLUGINS-2133).

Find this line and align path:

```xml
<Import Project="C:/WINNT/Microsoft.NET/Framework/v2.0.50727\Microsoft.CSharp.targets" />
```

Download flat assembler ([flatassembler.net](http://flatassembler.net)), extract it wherever you want, and set
```FASM_HOME``` environment variable to point this location.

At the end, to build whole project, simply run this command:

```
mvn clean install -P dotnet
```

In the target directory you will find ```zip``` file containing all required binaries.


## Usage

```
Usage: injectdll [procname] [runtime] [dllpath] [class] [function]
  [procname] - process name
  [runtime]  - framework runtime version
  [dllpath]  - path to injectee DLL file
  [class]    - injectee class name togehter with namespace (e.g. Test.Program)
  [function] - injectee function to run (e.g. Main)
```

For example:

```
injectdll testprocess v2.0.50727 c:\somedir\my.dll Test.Program Main
```

For the above example you will have to have such code compiled to DLL:

```cs
namespace Test {
    class Program {
        public static int Main(string arg) {
            MessageBox.Show("Hello World from Injectee!");
            return 0;
        }
    }
}
```

It will be injected into process executed from ```testprocess.exe```.

Please note that injectee function signature must be:

```cs
public static int [function name](string arg)
```

Its also important to note that your DLL framework version should be compatible with the target process version. 
For example you cannot inject .NET 4.0 based DLL into 2.0 process.