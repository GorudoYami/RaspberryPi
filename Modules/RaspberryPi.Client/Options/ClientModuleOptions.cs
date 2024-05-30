using RaspberryPi.Common.Options;
using System;
using System.ComponentModel.DataAnnotations;

namespace RaspberryPi.Client.Options {
	public class ClientModuleOptions : IModuleOptions {
		public bool Enabled { get; set; }
		public string ServerHost { get; set; }
		public int MainServerPort { get; set; }
		public int TimeoutSeconds { get; set; }
		public int VideoServerPort { get; set; }
	}
}
