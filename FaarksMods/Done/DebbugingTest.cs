using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This stuff makes your mod-dll live-editable while debugging it via you debug-launcher. Does not always work perfect, though.
[assembly: System.Diagnostics.Debuggable(System.Diagnostics.DebuggableAttribute.DebuggingModes.Default | System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations | System.Diagnostics.DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | System.Diagnostics.DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]

