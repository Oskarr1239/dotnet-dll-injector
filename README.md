dotnet-dll-injector
============

Tool for injecting managed .NET DLL libraries into native process (or not native with some limitations). 
Support both x86 and x64. Tested on v2.0.50727 and v4.0.30319 runtimes. For loading runtime was 
used interface marked as obsolete from 4.0 and later.

**This project is based on original [NDLLInjector](https://github.com/fday/NDllInjector) by [fday](https://github.com/fday).** 

## How To Use

You can either use dependency manager like Maven, Grendle or Ivy or download precompiled JARs and 
include them in your project's classpath.

The JAR is being releases as OSGi bundle and therefore it can be used in such OSGi
frameworks like [Equinox](http://www.eclipse.org/equinox/), 
[Apache Felix](http://felix.apache.org/site/index.html) or 
[FUSE ESB](http://fusesource.com/products/enterprise-servicemix/). 

### Maven Users

Maven dependency to be added into the POM:

```xml
<dependency>
	<groupId>com.github.sarxos</groupId>
	<artifactId>dotnet-dll-injector</artifactId>
	<version>0.2.1</version>
</dependency>
```

### Non-Maven Users

If you are not using Maven (nor any other dependency manager), then you have to download precompiled
binaries available [here](http://repo.sarxos.pl/maven2/com/github/sarxos/dotnet-dll-injector/0.1/dotnet-dll-injector-0.1-dist.zip) 
along with all required dependencies (~1MB zip file).

### Code Sample

Use this code to inject DLL into any process (this is **Java** code, not C#):

```java
public class Main {

	public static void main(String[] args) {

		String procName = "someprocess.exe";     // process name
		File dll = new File("path/to/some.dll"); // injectee DLL path
		
		// signature of method to be run after DLL is injected
		Signature signature = new Signature("TestNamespace", "Program", "Main"); 

		int pid = InjectorUtils.getProcessID(procName);     // get process ID
		Injector.getInstance().inject(pid, dll, signature); // inject DLL into process
	}
}
```

The DLL file which you want to inject has to define method with the following signature:

```cs
public static int MethodNameHere(string arg) {
	// code
}
```

For example (this is **C#** code, not Java):

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

Follow the next points to understand how to configure build if your environment has not been configured.


### Configure .NET

Sonar _maven-dotnet-plugin_ is not able to take parameters for MSBuild, so you have to change all ```*.csproj```
files from the project top reflect correct path to .NET framework home directory. I've already created ticket 
for this problem in Sonar JIRA - [SONARPLUGINS-2133](http://jira.codehaus.org/browse/SONARPLUGINS-2133). 
Because of that you have to edit each ```*.csproj``` file and align this path to point existing file:

```xml
<Import Project="C:\WINNT\Microsoft.NET\Framework\v2.0.50727\Microsoft.CSharp.targets" />
```
