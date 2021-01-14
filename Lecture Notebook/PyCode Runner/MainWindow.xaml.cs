using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Python.Runtime;

namespace PyCode_Runner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PyCodeManager CodeManager { get; set; }
        Code CurrentCode { get; set; }

        readonly FileSystemWatcher codefileWatcher = new FileSystemWatcher("PyCode");

        System.Windows.Threading.Dispatcher dispatcher;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5),
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            codefileWatcher.Created += CodefileWatcher_Created;
            codefileWatcher.Deleted += CodefileWatcher_Deleted;
            codefileWatcher.Renamed += CodefileWatcher_Renamed;

            codefileWatcher.Filter = "*.py";

            codefileWatcher.EnableRaisingEvents = true;

            dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        private void CodefileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            CodeManager.Delete(new FileInfo(e.OldFullPath));
            CodeManager.Create(new FileInfo(e.FullPath));

            dispatcher.Invoke(() =>
            {
                pyCodes.Items.Remove(e.OldName);
                pyCodes.Items.Add(e.Name);
            });
        }

        private void CodefileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            CodeManager.Delete(new FileInfo(e.FullPath));

            dispatcher.Invoke(() =>
            {
                pyCodes.Items.Remove(e.Name);
            });
        }

        private void CodefileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            CodeManager.Create(e.FullPath);

            dispatcher.Invoke(() =>
            {
                pyCodes.Items.Add(e.Name);
            });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CodeManager.Update();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var PYHOME = Environment.ExpandEnvironmentVariables(@"C:\Program Files\Python38");

            AddEnvPath(
                PYHOME, 
                System.IO.Path.Combine(PYHOME, @"Library\bin"),
                System.IO.Path.Combine(PYHOME, @"DLLs")
            );

            PythonEngine.PythonHome = PYHOME;

            PythonEngine.PythonPath = string.Join(System.IO.Path.PathSeparator,
            new[]
            {
                PythonEngine.PythonPath,
                System.IO.Path.Combine(PYHOME, @"Lib\site-packages"),
                System.IO.Path.Combine(PYHOME, @"Lib"),
            });

            PythonEngine.Initialize();
            IntPtr ts = PythonEngine.BeginAllowThreads();

            CodeManager = new PyCodeManager();
            CodeManager.Load("PyCode");

            var iter = CodeManager.CodeNames().GetEnumerator();
            while (iter.MoveNext())
            {
                pyCodes.Items.Add(iter.Current);
            }

            pyCodes.SelectionChanged += ChangeCode;
            if (pyCodes.Items.Count > 0)
            {
                pyCodes.SelectedIndex = 0;
            }
        }

        private void ChangeCode(object sender, SelectionChangedEventArgs e)
        {
            CurrentCode = CodeManager.GetCode((string)pyCodes.SelectedValue ?? "");
            codeBox.Text = CurrentCode.code;
        }

        private void OnEditCode(object sender, TextChangedEventArgs e)
        {
            CurrentCode.code = codeBox.Text;
            CurrentCode.isModfied = true;
        }

        static void AddEnvPath(params string[] paths)
        {
            // PC에 설정되어 있는 환경 변수를 가져온다.
            var envPaths = Environment.GetEnvironmentVariable("PATH").Split(System.IO.Path.PathSeparator).ToList();
            // 중복 환경 변수가 없으면 list에 넣는다.
            envPaths.InsertRange(0, paths.Where(x => x.Length > 0 && !envPaths.Contains(x)).ToArray());
            // 환경 변수를 다시 설정한다.
            Environment.SetEnvironmentVariable("PATH", string.Join(System.IO.Path.PathSeparator.ToString(), envPaths), EnvironmentVariableTarget.Process);
        }

        private void RunCode(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(CurrentCode.fi.Name);

                StdoutPrinter printer = new StdoutPrinter();

                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    sys.stdout = printer;
                    sys.stderr = printer;

                    PythonEngine.RunSimpleString(CurrentCode?.code ?? "");

                    outputBox.Text = printer.Output;
                }
            }
            catch (Exception ex)
            {
                outputBox.Text = ex.Message;
            }
        }

        public class StdoutPrinter
        {
            readonly StringBuilder buffer = new StringBuilder();

            public string Output { get { return buffer.ToString(); } }

            public void write(string str)
            {
                buffer.Append(str);
            }

            public void writelines(string[] str)
            {
                foreach (var line in str)
                {
                    buffer.AppendLine(line);
                }
            }

            public void flush() { }

            public void close() { }
        }
    }
}
