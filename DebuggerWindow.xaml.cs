using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;
using DebuggingTool.Controls;
using Newtonsoft.Json;
using HunterPie.GUI;
using System.Diagnostics;
using HunterPie.Core.Definitions;
using Debugger = HunterPie.Logger.Debugger;

namespace DebuggingTool
{
    /// <summary>
    /// Interaction logic for DebuggerWindow.xaml
    /// </summary>
    public partial class DebuggerWindow : Window
    {
        struct MemoryManagement
        {
            public string[] PrivateMemory;
            public string[] PagedMemory;
            public int[] GarbageCollection;
        }
        struct sDebuggingInfo
        {
            public string[] ScanPlayer;
            public string[] ScanMonster;
            public string[] RenderTime;
            public MemoryManagement MemoryManagement;
        };

        public string DataText
        {
            get { return (string)GetValue(DataTextProperty); }
            set { SetValue(DataTextProperty, value); }
        }
        public static readonly DependencyProperty DataTextProperty =
            DependencyProperty.Register("DataText", typeof(string), typeof(DebuggerWindow));

        readonly Stopwatch playerBenchmark = Stopwatch.StartNew();
        readonly Stopwatch renderBenchmark = Stopwatch.StartNew();
        readonly Stopwatch[] monstersBenchmark = { Stopwatch.StartNew(), Stopwatch.StartNew(), Stopwatch.StartNew() };

        sDebuggingInfo debuggingInfo = new sDebuggingInfo
        {
            ScanPlayer = new string[] { "0ms" },
            ScanMonster = new string[] { "0ms", "0ms", "0ms" },
            RenderTime = new string[] { "0ms" },
            MemoryManagement = new MemoryManagement
            {
                PrivateMemory = new string[] { "0MB" },
                PagedMemory = new string[] { "0MB" },
                GarbageCollection = new int[] {0,0,0}
            }

        };

        Game game;

        TreeViewItem monsterItem;
        CustomItem playerItem;
        TreeViewItem partyItem;
        CustomItem hunterPieItem;

        public DebuggerWindow()
        {
            InitializeComponent();
        }

        private void SetupItems()
        {
            monsterItem = new TreeViewItem()
            {
                Header = "Monsters",
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.WhiteSmoke
            };
            playerItem = new CustomItem()
            {
                Header = "Player",
                Data = game.Player,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.WhiteSmoke
            };
            playerItem.Items.Add(new CustomItem { Header = "Inventory", Data = game.Player.Inventory, FontWeight = FontWeights.SemiBold, Foreground = Brushes.WhiteSmoke });
            playerItem.Items.Add(new CustomItem { Header = "Set Skills", Data = game.Player.Skills, FontWeight = FontWeights.SemiBold, Foreground = Brushes.WhiteSmoke });

            partyItem = new TreeViewItem()
            {
                Header = "Party",
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.WhiteSmoke
            };

            hunterPieItem = new CustomItem()
            {
                Header = "HunterPie",
                Data = debuggingInfo,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.WhiteSmoke
            };

            dataTreeView.Items.Add(monsterItem);
            dataTreeView.Items.Add(playerItem);
            dataTreeView.Items.Add(partyItem);
            dataTreeView.Items.Add(hunterPieItem);
        }

        private void OnRender(object sender, EventArgs e)
        {
            float elapsed = renderBenchmark.ElapsedTicks / ((float)TimeSpan.TicksPerMillisecond);
            debuggingInfo.RenderTime[0] = $"{elapsed}ms";
            
            debuggingInfo.MemoryManagement.PrivateMemory[0] = $"{(Process.GetCurrentProcess().PrivateMemorySize64 / 1e+6)} MB";
            debuggingInfo.MemoryManagement.PagedMemory[0] = $"{(Process.GetCurrentProcess().PagedMemorySize64 / 1e+6)} MB";
            debuggingInfo.MemoryManagement.GarbageCollection[0] = GC.CollectionCount(0);
            debuggingInfo.MemoryManagement.GarbageCollection[1] = GC.CollectionCount(1);
            debuggingInfo.MemoryManagement.GarbageCollection[2] = GC.CollectionCount(2);

            renderBenchmark.Restart();
            if (dataTreeView.SelectedItem is CustomItem)
            {
                CustomItem selected = dataTreeView.SelectedItem as CustomItem;
                
                if (selected.Data is Monster)
                {
                    Monster data = selected.Data as Monster;
                    DataText = JsonConvert.SerializeObject(new MonsterFilteredData(data), Formatting.Indented);
                } else if (selected.Data is Player)
                {
                    Player data = selected.Data as Player;
                    DataText = JsonConvert.SerializeObject(new PlayerFilteredData(data), Formatting.Indented);
                }
                else
                {
                    DataText = JsonConvert.SerializeObject(selected.Data, Formatting.Indented);
                }
                
            } else
            {
                DataText = "No data to be displayed.";
            }
        }

        internal void SetGameContext(Game ctx)
        {
            game = ctx;
            SetupItems();
            HookEvents();
        }

        private void HookEvents()
        {
            foreach (Monster m in game.Monsters)
            {
                m.OnMonsterSpawn += OnMonsterSpawn;
                m.OnMonsterAilmentsCreate += OnAilmentsCreate;
                m.OnMonsterDespawn += OnMonsterDespawn;
                m.OnMonsterScanFinished += OnMonsterScanFinished;
            }
            game.Player.OnPlayerScanFinished += OnPlayerScanFinished;
        }

        internal void UnhookEvents()
        {
            foreach (Monster m in game.Monsters)
            {
                m.OnMonsterSpawn -= OnMonsterSpawn;
                m.OnMonsterAilmentsCreate -= OnAilmentsCreate;
                m.OnMonsterDespawn -= OnMonsterDespawn;
                m.OnMonsterScanFinished -= OnMonsterScanFinished;
            }
            game.Player.OnPlayerScanFinished -= OnPlayerScanFinished;
        }

        private void OnPlayerScanFinished(object source, EventArgs args)
        {
            float elapsed = playerBenchmark.ElapsedTicks / ((float)TimeSpan.TicksPerMillisecond) - UserSettings.PlayerConfig.Overlay.GameScanDelay;
            debuggingInfo.ScanPlayer[0] = $"{elapsed:0.00000}ms";
            playerBenchmark.Restart();

        }

        private void OnMonsterScanFinished(object source, EventArgs args)
        {
            Monster m = (Monster)source;

            float elapsed = monstersBenchmark[m.MonsterNumber - 1].ElapsedTicks / ((float)TimeSpan.TicksPerMillisecond) - UserSettings.PlayerConfig.Overlay.GameScanDelay;
            debuggingInfo.ScanMonster[m.MonsterNumber - 1] = $"{elapsed:0.00000}ms";
            monstersBenchmark[m.MonsterNumber - 1].Restart();
        }

        private void OnAilmentsCreate(object source, EventArgs args)
        {
            Monster m = (Monster)source;

            Dispatch(() =>
            {
                CustomItem parentItem = monsterItem.Items.Cast<CustomItem>()
                .Where(e => ((Monster)e.Data).MonsterNumber == m.MonsterNumber).FirstOrDefault();

                if (parentItem is null)
                {
                    return;
                }

                TreeViewItem ailmParent = parentItem.Items.Cast<TreeViewItem>().Where(p => p.Header.ToString() == "Ailments").First();

                foreach (Ailment ailment in m.Ailments)
                {
                    CustomItem item = new CustomItem
                    {
                        Header = $"{ailment.Id} [{ailment.Name}]",
                        Data = ailment.cMonsterAilment,
                        FontWeight = FontWeights.Normal
                    };
                    parentItem.Items.Add(item);
                }
            });
            
        }

        private void OnMonsterSpawn(object source, MonsterSpawnEventArgs args)
        {
            Monster m = (Monster)source;

            Dispatch(() =>
            {
                CustomItem parentItem = new CustomItem
                {
                    Header = $"{m.MonsterNumber} [{m.Name}]",
                    Data = m,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.WhiteSmoke
                };

                TreeViewItem partParent = new TreeViewItem
                {
                    Header = "Parts",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.WhiteSmoke
                };

                TreeViewItem ailmParent = new TreeViewItem
                {
                    Header = "Ailments",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.WhiteSmoke
                };

                foreach (Part part in m.Parts)
                {
                    CustomItem partItem = new CustomItem
                    {
                        Header = part.Name,
                        Data = part.cMonsterPartData,
                        FontWeight = FontWeights.Normal,
                        Foreground = Brushes.WhiteSmoke
                    };
                    partParent.Items.Add(partItem);
                }

                parentItem.Items.Add(partParent);
                parentItem.Items.Add(ailmParent);

                if (m.Ailments.Count > 0)
                {
                    foreach (Ailment ailment in m.Ailments)
                    {
                        CustomItem ailmItem = new CustomItem
                        {
                            Header = ailment.Name,
                            Data = ailment.cMonsterAilment,
                            FontWeight = FontWeights.Normal,
                            Foreground = Brushes.WhiteSmoke
                        };
                        ailmParent.Items.Add(ailmItem);
                    }
                }

                monsterItem.Items.Add(parentItem);
            });
        }

        private void OnMonsterDespawn(object source, EventArgs args)
        {
            Monster m = (Monster)source;

            Dispatch(() =>
            {
                CustomItem parentItem = monsterItem.Items.Cast<CustomItem>()
                .Where(e => ((Monster)e.Data).MonsterNumber == m.MonsterNumber).FirstOrDefault();
                
                parentItem.Items.Clear();
                monsterItem.Items.Remove(parentItem);
            });
            
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            CompositionTarget.Rendering += OnRender;
        }

        private void Dispatch(Action f) =>
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, f);

        private void OnForceGCClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
