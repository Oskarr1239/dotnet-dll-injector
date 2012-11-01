package com.github.sarxos.netinject;

public class Signature {

	private String namespace = null;
	private String clazz = null;
	private String method = null;

	public Signature(String namespace, String clazz, String method) {
		super();
		this.namespace = namespace;
		this.clazz = clazz;
		this.method = method;
	}

	@Override
	public String toString() {
		return new StringBuilder().append(namespace).append('.').append(clazz).append('.').append(method).toString();
	}
}
