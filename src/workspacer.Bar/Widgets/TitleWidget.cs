using System;
using System.Drawing;
using System.Linq;

namespace workspacer.Bar.Widgets
{
	public class TitleWidget : BarWidgetBase
    {
        private int _maxLength = 54;

        public Color MonitorHasFocusColor { get; set; } = Color.Yellow;
        public Func<IWindow, string> TitleCreator { get; set; }

        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = value > 0 ? value : 1;
        }

        public override IBarWidgetPart[] GetParts()
        {
            var window = GetWindow();
            var isFocusedMonitor = Context.MonitorContainer.FocusedMonitor == Context.Monitor;
            var multipleMonitors = Context.MonitorContainer.NumMonitors > 1;
            var color = isFocusedMonitor && multipleMonitors ? MonitorHasFocusColor : null;

            if (!(window is null))
            {
                if (TitleCreator is null)
                {
                    TitleCreator = (w) =>
                    {
                        var pn = w.ProcessName.Trim();
                        var wt = w.Title.Trim();
                        var wts = wt.Substring(0, Math.Min(wt.Length, MaxLength));
                        var incPn = !(wts.IndexOf(pn, StringComparison.InvariantCultureIgnoreCase) >= 0);
                        var tt =
                            $"{(incPn ? pn.Substring(0, Math.Min(pn.Length, MaxLength / 4)) + " - " : string.Empty)}{wts}";
                        return tt.Substring(0, Math.Min(tt.Length, MaxLength));

                    };
                }
                //var procName = window.ProcessName.Trim().Substring(0, Math.Min(window.ProcessName.Trim().Length, (int)Math.Floor(MaxLength * 0.25)));
                //var winTitle = window.Title.Trim().Substring(0, Math.Min(window.Title.Trim().Length, (int)Math.Floor(MaxLength * 0.75)));
                //var includeProcName = !(winTitle.IndexOf(procName, StringComparison.InvariantCultureIgnoreCase) >= 0);
                //var procNameAppended = includeProcName ? $"{procName} - " : string.Empty;

                //var titleText = $"{procNameAppended}{winTitle}";
                return Parts(Part(TitleCreator(window), color));
            }
            else
            {
                return Parts(Part("No Managed Windows", color));
            }
        }

        public override void Initialize()
        {
            Context.Workspaces.WindowAdded += RefreshAddRemove;
            Context.Workspaces.WindowRemoved += RefreshAddRemove;
            Context.Workspaces.WindowUpdated += RefreshUpdated;
            Context.Workspaces.FocusedMonitorUpdated += RefreshFocusedMonitor;
        }

        private IWindow GetWindow()
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            return currentWorkspace.FocusedWindow ??
                   currentWorkspace.LastFocusedWindow ??
                   currentWorkspace.Windows.FirstOrDefault(w => w?.CanLayout ?? false);
        }

        private void RefreshAddRemove(IWindow window, IWorkspace workspace)
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            if (workspace == currentWorkspace)
            {
                Context.MarkDirty();
            }
        }

        private void RefreshUpdated(IWindow window, IWorkspace workspace)
        {
            var currentWorkspace = Context.WorkspaceContainer.GetWorkspaceForMonitor(Context.Monitor);
            if (workspace == currentWorkspace && window == GetWindow())
            {
                Context.MarkDirty();
            }
        }

        private void RefreshFocusedMonitor()
        {
            Context.MarkDirty();
        }
    }
}
