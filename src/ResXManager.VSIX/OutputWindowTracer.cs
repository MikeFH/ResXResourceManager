﻿namespace ResXManager.VSIX
{
    using System;

    using JetBrains.Annotations;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    using ResXManager.Infrastructure;

    public class OutputWindowTracer : ITracer
    {
        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private static Guid _outputPaneGuid = new Guid("{C49C2D45-A34D-4255-9382-40CE2BDAD575}");

        public OutputWindowTracer([NotNull]IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void LogMessageToOutputWindow(string? value)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (!(_serviceProvider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow))
                return;

            var errorCode = outputWindow.GetPane(ref _outputPaneGuid, out var pane);

            if (ErrorHandler.Failed(errorCode) || pane == null)
            {
                outputWindow.CreatePane(ref _outputPaneGuid, Resources.ToolWindowTitle, Convert.ToInt32(true), Convert.ToInt32(false));
                outputWindow.GetPane(ref _outputPaneGuid, out pane);
            }

            pane?.OutputString(value);
        }

#pragma warning disable VSTHRD010 // Accessing ... should only be done on the main thread.

        public void TraceError(string value)
        {
            WriteLine(string.Concat(Resources.Error, @" ", value));
        }

        public void TraceWarning(string value)
        {
            WriteLine(string.Concat(Resources.Warning, @" ", value));
        }

        public async void WriteLine(string value)
        {
            if (!Microsoft.VisualStudio.Shell.ThreadHelper.CheckAccess())
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            LogMessageToOutputWindow(value + Environment.NewLine);
        }

#pragma warning restore VSTHRD010 // Accessing ... should only be done on the main thread.
    }
}
