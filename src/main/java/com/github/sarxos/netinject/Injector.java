package com.github.sarxos.netinject;

import java.io.BufferedReader;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.URISyntaxException;
import java.util.zip.ZipException;


public class Injector {

	private static Injector instance = new Injector();

	private static final String X64_BOOTSTRAP_BIN = "bootstrap64.bin";
	private static final String X32_BOOTSTRAP_BIN = "bootstrap32.bin";
	private static final String INJECTDLL_EXE = "injectdll.exe";
	private static final String X86_RUNNER_EXE = "x86runner.exe";
	private static final String TESTINJECTEE_EXE = "TestInjectee.dll";

	private File x64bootstrapBin = null;
	private File x32bootstrapBin = null;
	private File injectdllExe = null;
	private File x86runnerExe = null;
	private File testinjecteeDll = null;

	private File getFile(String name) {
		try {
			return new File(InjectorUtils.resourceToLocalURI(name, Injector.class));
		} catch (ZipException e) {
			throw new InjectorRuntimeException(e);
		} catch (IOException e) {
			throw new InjectorRuntimeException(e);
		} catch (URISyntaxException e) {
			throw new InjectorRuntimeException(e);
		}
	}

	public static Injector getInstance() {
		return instance;
	}

	public boolean test(int pid) {
		if (testinjecteeDll == null) {
			testinjecteeDll = getFile(TESTINJECTEE_EXE);
		}
		return inject(pid, testinjecteeDll, "Test.Program.Main");
	}

	public boolean inject(int pid, File dll, String signature) {

		if (injectdllExe == null) {
			injectdllExe = getFile(INJECTDLL_EXE);
		}
		if (x32bootstrapBin == null) {
			x32bootstrapBin = getFile(X32_BOOTSTRAP_BIN);
		}
		if (x64bootstrapBin == null) {
			x64bootstrapBin = getFile(X64_BOOTSTRAP_BIN);
		}
		if (x86runnerExe == null) {
			x86runnerExe = getFile(X86_RUNNER_EXE);
		}

		// @formatter:off
		String[] command = new String[] {
			injectdllExe.getAbsolutePath(),
			"--proc-id=" + pid,
			"--dll-path=" + dll.getAbsolutePath(),
			"--signature=" + signature,
			"--x86-runner-path=" + x86runnerExe.getAbsolutePath(),
			"--x64-bootstrap-path=" + x64bootstrapBin.getAbsolutePath(),
			"--x32-bootstrap-path=" + x32bootstrapBin.getAbsolutePath(),
			"--verbose"
		};
		// @formatter:on

		Process process = null;
		try {
			process = Runtime.getRuntime().exec(command);
			process.getOutputStream().close();
		} catch (IOException e) {
			throw new InjectorRuntimeException(e);
		}

		InputStream is = process.getInputStream();
		InputStreamReader isr = new InputStreamReader(is);
		BufferedReader br = new BufferedReader(isr);

		String line = null;
		try {
			while ((line = br.readLine()) != null) {
				System.out.println(line);
			}
		} catch (IOException e) {
			throw new InjectorRuntimeException(e);
		}

		return true;
	}
}
