using System;

namespace GorudoYami.Common.Modules {
	public class InitializeModuleException : Exception {
		public InitializeModuleException(string message) : base(message) { }
		public InitializeModuleException(string message, Exception innerException) : base(message, innerException) { }
		public InitializeModuleException() : base() { }
	}
}
