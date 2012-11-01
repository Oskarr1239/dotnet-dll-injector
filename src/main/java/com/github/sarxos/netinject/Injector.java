package com.github.sarxos.netinject;

import java.io.BufferedReader;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.ZipException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;


public class Injector {

	private static final String X64_BOOTSTRAP_BIN = "bootstrap64.bin";
	private static final String X32_BOOTSTRAP_BIN = "bootstrap32.bin";
	private static final String INJECTDLL_EXE = "injectdll.exe";
	private static final String X86_RUNNER_EXE = "x86runner.exe";
	private static final String TESTINJECTEE_20_EXE = "test20.dll";

	private static final Logger LOG = LoggerFactory.getLogger(Injector.class);

	private static Injector instance = new Injector();

	private File x64bootstrapBin = null;
	private File x32bootstrapBin = null;
	private File injectdllExe = null;
	private File x86runnerExe = null;
	private File testinjecteeDll = null;

	private boolean initialized = false;

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

	private void initialize() {

		if (initialized) {
			return;
		}

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
		if (testinjecteeDll == null) {
			testinjecteeDll = getFile(TESTINJECTEE_20_EXE);
		}

		initialized = true;
	}

	public static Injector getInstance() {
		return instance;
	}

	public boolean test(int pid) {

		initialize();

		return inject(pid, testinjecteeDll, new Signature("Test", "Program", "Main"));
	}

	public boolean inject(int pid, File dll, Signature signature) {

		initialize();

		List<String> command = new ArrayList<String>();
		command.add(injectdllExe.getAbsolutePath());
		command.add(String.format("--proc-id=%s", pid));
		command.add(String.format("--dll-path=%s", dll.getAbsolutePath()));
		command.add(String.format("--signature=%s", signature));
		command.add(String.format("--x86-runner-path=%s", x86runnerExe.getAbsolutePath()));
		command.add(String.format("--x64-bootstrap-path=%s", x64bootstrapBin.getAbsolutePath()));
		command.add(String.format("--x32-bootstrap-path=%s", x32bootstrapBin.getAbsolutePath()));

		if (LOG.isDebugEnabled()) {
			command.add("--verbose");
		}

		Process process = null;
		try {
			process = Runtime.getRuntime().exec(command.toArray(new String[command.size()]));
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
				if (!line.trim().isEmpty()) {
					LOG.info(String.format("NATIVE: %s", line));
				}
			}
		} catch (IOException e) {
			throw new InjectorRuntimeException(e);
		}

		return true;
	}
}
