﻿using GorudoYami.Common.Modules;

namespace RaspberryPi.Common.Modules;

public interface IRaspberryPiModule : IModule {
	Task RunAsync();
}
