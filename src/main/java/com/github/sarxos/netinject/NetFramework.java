package com.github.sarxos.netinject;

import java.io.File;
import java.io.FilenameFilter;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import com.github.sarxos.winreg.HKey;
import com.github.sarxos.winreg.RegistryException;
import com.github.sarxos.winreg.WindowsRegistry;


public class NetFramework {

	private static class NetDirectoryFilter implements FilenameFilter {

		public boolean accept(File directory, String fileName) {
			File file = new File(directory + "/" + fileName);
			return file.isDirectory() && file.getName().startsWith("v");
		}
	};

	private static final String NET_FRAMEWORK_KEY = "SOFTWARE\\Microsoft\\.NETFramework";
	public static final String INSTALL_ROOT = getFrameworksDirectory();

	private String version = null;
	private File directory = null;

	public NetFramework(String version) {
		this.version = version;
	}

	private static String getFrameworksDirectory() {
		try {
			return WindowsRegistry.getInstance().readString(HKey.HKLM, NET_FRAMEWORK_KEY, "InstallRoot");
		} catch (RegistryException e) {
			throw new RuntimeException(e);
		}
	}

	public static List<NetFramework> getFrameworks() {

		File directory = new File(NetFramework.INSTALL_ROOT);
		File[] directories = directory.listFiles(new NetDirectoryFilter());

		List<NetFramework> frameworks = new ArrayList<NetFramework>();
		for (File d : directories) {
			frameworks.add(new NetFramework(d.getName()));
		}

		return Collections.unmodifiableList(frameworks);
	}

	public String getVersion() {
		return version;
	}

	public String getSimpleVersion() {
		return getSimpleVersion(this.version);
	}

	public static String getSimpleVersion(String version) {
		return version.substring(1, 4);
	}

	public static NetFramework getFramework(String version) {
		if (version == null) {
			throw new IllegalArgumentException("Version cannot be null");
		}
		for (NetFramework nf : getFrameworks()) {
			if (version.startsWith("v")) {
				if (version.equals(nf.getVersion())) {
					return nf;
				}
			} else {
				if (version.equals(nf.getSimpleVersion())) {
					return nf;
				}
			}
		}
		return null;
	}

	public File getDirectory() {
		if (directory == null) {
			directory = new File(INSTALL_ROOT + "/" + getVersion());
		}
		return directory;
	}
}
