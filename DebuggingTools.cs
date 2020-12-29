using HunterPie.Core;
using System;
using HunterPie.Plugins;
using System.Windows;
using HunterPie.GUI;

namespace DebuggingTool
{
    public class DebuggingTools : IPlugin
    {
        public string Name { get; set; } = "DebuggingTools";
        public string Description { get; set; } = "HunterPie tools for debugging data";
        public Game Context { get; set; }

        DebuggerWindow window;

        public void Initialize(Game context)
        {
            Context = context;
            HookEvents();
        }

        public void Unload()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UnhookEvents();
                window.Close();
                window = null;
            }));
        }

        private void HookEvents()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (window is null)
                {
                    window = new DebuggerWindow();
                    window.SetGameContext(Context);
                    window.Show();
                }
            }));

        }

        private void UnhookEvents()
        {
            window.UnhookEvents();
        }
    }
}
