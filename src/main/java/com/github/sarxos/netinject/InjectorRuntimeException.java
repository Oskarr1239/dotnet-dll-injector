package com.github.sarxos.netinject;

public class InjectorRuntimeException extends RuntimeException {

	private static final long serialVersionUID = 6302362155135944497L;

	public InjectorRuntimeException() {
		super();
	}

	public InjectorRuntimeException(String message, Throwable cause) {
		super(message, cause);
	}

	public InjectorRuntimeException(String message) {
		super(message);
	}

	public InjectorRuntimeException(Throwable cause) {
		super(cause);
	}

}
