package com.github.sarxos.netinject;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.URI;
import java.net.URISyntaxException;
import java.security.CodeSource;
import java.security.ProtectionDomain;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipException;
import java.util.zip.ZipFile;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;


/**
 * Native process utilities.
 * 
 * @author Cenobite Limited
 */
public class InjectorUtils {

	private static final Logger LOG = LoggerFactory.getLogger(InjectorUtils.class);

	public static URI resourceToLocalURI(String resource, Class<?> ctx) throws ZipException, IOException, URISyntaxException {

		ProtectionDomain domain = ctx.getProtectionDomain();
		CodeSource source = domain.getCodeSource();
		URI where = source.getLocation().toURI();

		File location = new File(where);
		URI uri = null;

		if (location.isDirectory()) {
			uri = URI.create(where.toString() + resource);
		} else {
			ZipFile zip = new ZipFile(location);
			try {
				uri = extract(zip, resource);
			} finally {
				zip.close();
			}
		}

		return uri;
	}

	private static URI extract(ZipFile zip, String filen) throws IOException {

		File tmp = File.createTempFile(filen + ".", ".tmp");
		tmp.deleteOnExit();

		final ZipEntry entry = zip.getEntry(filen);
		if (entry == null) {
			throw new FileNotFoundException(String.format("Cannot find file %s in archive %s", filen, zip.getName()));
		}

		InputStream is = zip.getInputStream(entry);
		OutputStream os = new FileOutputStream(tmp);

		final byte[] buf = new byte[1024];
		int i = 0;
		try {
			while ((i = is.read(buf)) != -1) {
				os.write(buf, 0, i);
			}
		} finally {
			if (is != null) {
				try {
					is.close();
				} catch (IOException e) {
					LOG.error(e.getMessage(), e);
				}
				try {
					os.close();
				} catch (IOException e) {
					LOG.error(e.getMessage(), e);
				}
			}
		}

		return tmp.toURI();
	}

	public static final int getProcessID(String name) {
		int[] pids = getProcessIDs(name);
		return pids.length > 0 ? pids[0] : -1;
	}

	/**
	 * Return native process IDs. If there are more than one process, this
	 * method will return the first one.
	 * 
	 * @param name the process name to be found (e.g. winamp.exe)
	 * @return Given process ID
	 */
	public static final int[] getProcessIDs(String name) {

		if (name == null) {
			throw new IllegalArgumentException("Process name cannot be null!");
		}
		if (name.isEmpty()) {
			throw new IllegalArgumentException("Process name cannot be empty!");
		}

		Process process = null;
		try {
			process = Runtime.getRuntime().exec(new String[] { "tasklist", "/fo", "csv" });
			process.getOutputStream().close();
		} catch (IOException e) {
			throw new RuntimeException(e);
		}

		InputStream is = process.getInputStream();
		InputStreamReader isr = new InputStreamReader(is);
		BufferedReader br = new BufferedReader(isr);

		List<String> found = new ArrayList<String>();

		String line = null;
		try {

			// skip first two lines (blank line + headers)
			br.readLine();
			br.readLine();

			while ((line = br.readLine()) != null) {
				String[] parts = StringUtils.split(line, "\",\"", 3);
				if (name.equalsIgnoreCase(parts[0])) {
					found.add(parts[1]);
				}
			}

		} catch (IOException e) {
			throw new RuntimeException(e);
		} finally {
			try {
				br.close();
			} catch (IOException e) {
				LOG.error(e.getMessage(), e);
			}
		}

		int[] pids = new int[found.size()];
		for (int i = 0; i < found.size(); i++) {
			pids[i] = Integer.parseInt(found.get(i));
		}

		return pids;
	}
}
