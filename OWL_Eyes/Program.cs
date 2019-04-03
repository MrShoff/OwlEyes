using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OverwatchLeagueApiResponseTypes;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Drawing;
using MySql.Data.MySqlClient;
using System.ServiceProcess;
using System.IO;

namespace OWL_Eyes
{
    class Program
    {
        // ---- FOR TESTING ----
        static DateTime SIMULATED_TIME = new DateTime(2018, 6, 16, 15, 30, 0);
        static Boolean _useSimulatedTime = false;
        static Boolean _debuggingOn = false;
        // ---- FOR TESTING ----

        // ---- SET TRUE FOR PUBLISH ----
        static Boolean _autoRunEnabled = true;
        // ---- SET TRUE FOR PUBLISH ----

        static TimeSpan MATCH_LENGTH = new TimeSpan(2, 15, 0);
        static DateTime programStartTime = (_useSimulatedTime ? SIMULATED_TIME : DateTime.Now);
        static BackgroundWorker autoRunWorker = new BackgroundWorker();
        static Boolean _currentlyWatching { get; set; }
        static OwlSchedule _currentSchedule { get; set; }

        #region Nested classes to support running as service
        public const string ServiceName = "OWL Eyes";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }
        #endregion

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start(args);
            }
        }

        public static void Start(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            if (_debuggingOn)
                File.AppendAllText(@"G:\Development\Visual Studio 2017\OWL_Eyes\OWL_Eyes\bin\Service\DebugLog.txt", String.Format("{0} started{1}", DateTime.Now, Environment.NewLine));
            autoRunWorker.DoWork += AutoRunWorker_DoWork;                 
            
            try
            {
                _autoRunEnabled = true;
                UserTextFeedback.ConsoleOut("Auto run enabled.");
                autoRunWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                UserTextFeedback.ConsoleOut($"There was an exception: {ex.ToString()}");
                SendExceptionToMySql(ex);
            }

            if (!Environment.UserInteractive) // if service
                return;            

            string help = "Commands (type number or word):\n" +
                "1) help - Display this menu.\n" +
                "2) get - Retrieve the next game data.\n" +
                "3) watch - Open the OWL Twitch channel in browser.\n" +
                "4) auto on - Automatically watch games.\n" +
                "5) auto off - Turn off auto-watch watch games.\n" +
                "6) stats - Get stats for current session.\n" +
                "0) quit - Exit program.";
            UserTextFeedback.ConsoleOut(help, false);
            string userResponse = Console.ReadLine();
            
            while (userResponse != "quit")
            {
                switch (userResponse)
                {
                    case "1":
                    case "help":
                        UserTextFeedback.ConsoleOut(help, false);
                        break;
                    case "2":
                    case "get":
                        try
                        {
                            SetCurrentSchedule().Wait();
                            DisplayGametimes();
                        }
                        catch (Exception ex)
                        {
                            UserTextFeedback.ConsoleOut($"There was an exception: {ex.ToString()}");
                            SendExceptionToMySql(ex);
                        }
                        break;
                    case "3":
                    case "watch":
                        try
                        {
                            OpenTwitchOwl();
                        }
                        catch (Exception ex)
                        {
                            UserTextFeedback.ConsoleOut($"There was an exception: {ex.ToString()}");
                            SendExceptionToMySql(ex);
                        }
                        break;
                    case "4":
                    case "auto on":
                        try
                        {
                            if (!_autoRunEnabled)
                            {
                                _autoRunEnabled = true;
                                UserTextFeedback.ConsoleOut("Auto run enabled.");
                                autoRunWorker.RunWorkerAsync();
                            }
                            else
                            {
                                UserTextFeedback.ConsoleOut("Auto run was already enabled.");
                            }
                        }
                        catch (Exception ex)
                        {
                            UserTextFeedback.ConsoleOut($"There was an exception: {ex.ToString()}");
                            SendExceptionToMySql(ex);
                        }
                        break;
                    case "5":
                    case "auto off":
                        _autoRunEnabled = false;
                        UserTextFeedback.ConsoleOut("Auto run disabled.");
                        break;
                    case "6":
                    case "stats":
                        string[] stats = GetSessionStats();
                        foreach (string statLine in stats)
                        {
                            UserTextFeedback.ConsoleOut(statLine);
                        }
                        break;
                    case "runtest":
                        CloseTwitchOwl();
                        break;
                    case "0":
                    case "quit":
                        return;
                    default:
                        UserTextFeedback.ConsoleOut(help, false);
                        break;
                }
                userResponse = Console.ReadLine();
            }
        }

        public static void Stop()
        {
            if (_debuggingOn)
                File.AppendAllText(@"DebugLog.txt", String.Format("{0} stopped{1}", DateTime.Now, Environment.NewLine));
        }

        private static async void AutoRunWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_currentSchedule == null)
            {
                await SetCurrentSchedule();
            }

            TimeSpan sleeptime = new TimeSpan(0, 0, 30);

            while (_autoRunEnabled)
            {
                DateTime currentTime = (_useSimulatedTime ? SIMULATED_TIME : DateTime.Now);

                Match nextGame = GetNextGame(_currentSchedule);
                Match mostRecentGame = GetMostRecentGame(_currentSchedule);
                DateTime nextGameTime = nextGame.StartDate.LocalDateTime;
                DateTime mrgGametime = mostRecentGame.StartDate.LocalDateTime;

                TimeSpan timeUntilNextMatch = nextGameTime - currentTime;
                TimeSpan timeSinceLastMatchStarted = currentTime - mrgGametime;
                _currentlyWatching = isUserWatching();
                if (!_currentlyWatching)
                    sleeptime = CalculateSleepTime(timeUntilNextMatch, mrgGametime, currentTime);
                else
                    sleeptime = new TimeSpan(0, 2, 0);

                List <string> userfeedback = new List<string>();
                if (_useSimulatedTime)
                    userfeedback.Add($"Current time: {currentTime}");
                string closingLine = "";

                
                if (_currentlyWatching)
                {
                    userfeedback.Add($"Most recent match started {(timeSinceLastMatchStarted).Hours} hours {(timeSinceLastMatchStarted).Minutes} minutes ago.");
                    if (nextGameTime > mrgGametime.Add(MATCH_LENGTH))
                    {
                        if ((currentTime - mrgGametime.Add(MATCH_LENGTH)).TotalMinutes > 0)
                        {
                            closingLine = "Closing Twitch now";
                        }
                        else
                        {
                            closingLine = $"Closing Twitch in {Math.Floor((mrgGametime.Add(MATCH_LENGTH) - currentTime).TotalMinutes)} minutes.";
                        }
                    }
                }
                else
                {
                    if (mrgGametime <= currentTime && mrgGametime.Add(MATCH_LENGTH) > currentTime)
                    {
                        OpenTwitchOwl();
                    }
                }
                if (mrgGametime.Add(MATCH_LENGTH) < currentTime && _currentlyWatching)
                {
                    CloseTwitchOwl();
                }

                // display user feedback
                //if (Environment.UserInteractive)
                    //Console.Clear();
                _currentlyWatching = isUserWatching();
                userfeedback.Add($"Currently watching: {(_currentlyWatching ? "YES" : "no")} ");
                userfeedback.Add($"Next match starting at {nextGameTime}");
                userfeedback.Add($"Application sleeping for next {sleeptime.Hours} hours {sleeptime.Minutes} minutes {sleeptime.Seconds} seconds.");
                if (closingLine != "")
                {
                    userfeedback.Add(closingLine);
                }
                foreach (string line in userfeedback)
                {
                    UserTextFeedback.ConsoleOut(line);
                }                

                if (_debuggingOn)
                {
                    userfeedback.Insert(0, ($"programStartTime: {programStartTime}"));
                    LogToMySqlDebug(userfeedback);
                    string userfeedbackJson = Newtonsoft.Json.JsonConvert.SerializeObject(userfeedback);
                    File.AppendAllText(@"G:\Development\Visual Studio 2017\OWL_Eyes\OWL_Eyes\bin\Service\DebugLog.txt", String.Format("{0} " + userfeedbackJson + "{1}", DateTime.Now, Environment.NewLine));
                }

                Thread.Sleep(sleeptime);
                SIMULATED_TIME = SIMULATED_TIME.AddSeconds(600);                
            }
        }

        private async static void LogToMySqlDebug(List<string> userfeedback)
        {
            string query = "INSERT INTO owleyes_debug (timestamp, value) VALUES (@timestamp, @value); ";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            MySqlParameter parTimestamp = new MySqlParameter("@timestamp", MySqlDbType.DateTime);
            MySqlParameter parValue = new MySqlParameter("@value", MySqlDbType.Text);
            parTimestamp.Value = DateTime.Now;
            parValue.Value = Newtonsoft.Json.JsonConvert.SerializeObject(userfeedback);
            parameters.Add(parTimestamp);
            parameters.Add(parValue);
            try
            {
                MySqlInteractions sql = new MySqlInteractions();
                await sql.Insert(query, parameters.ToArray());
            }
            catch (Exception ex)
            {
                UserTextFeedback.ConsoleOut($"There was an exception during MySQL logging: {ex.ToString()}");
                SendExceptionToMySql(ex);
            }
        }

        private static TimeSpan CalculateSleepTime(TimeSpan timeUntilNextMatch, DateTime mrgGametime, DateTime currentTime)
        {
            if (timeUntilNextMatch > new TimeSpan(0,5,0) && currentTime > mrgGametime.Add(MATCH_LENGTH))
            {
                return timeUntilNextMatch.Add(new TimeSpan(0, -5, 0));
                // sleep until 5 minutes before the next match
            }
            else
            {
                return new TimeSpan(0, 0, 30);
                // only sleep for 30 seconds at a time during gametime
            }
        }

        private static bool isUserWatching()
        {
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            if (procsChrome.Length <= 0)
            {
                return false;
            }
            else
            {
                foreach (Process proc in procsChrome)
                {
                    try
                    {
                        // the chrome process must have a window 
                        if (proc.MainWindowHandle == IntPtr.Zero)
                        {
                            continue;
                        }
                        // to find the tabs we first need to locate something reliable - the 'New Tab' button 
                        AutomationElement root = AutomationElement.FromHandle(proc.MainWindowHandle);
                        Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, "New Tab");
                        AutomationElement elmNewTab = root.FindFirst(TreeScope.Descendants, condNewTab);
                        // get the tabstrip by getting the parent of the 'new tab' button 
                        TreeWalker treewalker = TreeWalker.ControlViewWalker;
                        AutomationElement elmTabStrip = treewalker.GetParent(elmNewTab);
                        // loop through all the tabs and get the names which is the page title 
                        Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
                        foreach (AutomationElement tabitem in elmTabStrip.FindAll(TreeScope.Children, condTabItem))
                        {
                            Condition condEnabled = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                            AutomationElementCollection elmTabChildren = tabitem.FindAll(TreeScope.Children, condEnabled);
                            if (tabitem.Current.Name.Contains("OverwatchLeague - Twitch"))
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {                        
                        //UserTextFeedback.ConsoleOut($"There was an error during isUserWatching(): {ex.ToString()}");
                        SendExceptionToMySql(ex);
                    }
                }
            }
            return false;
        }

        private static void SendExceptionToMySql(Exception ex)
        {
            try
            {
                string exceptionJson = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
                File.AppendAllText(@"G:\Development\Visual Studio 2017\OWL_Eyes\OWL_Eyes\bin\Service\DebugLog.txt", String.Format("{0} " + exceptionJson + "{1}", DateTime.Now, Environment.NewLine));
                //string query = "INSERT INTO owleyes_debug (";
            }
            catch { //UserTextFeedback.ConsoleOut($"There was an error during SendExceptionToMySql(): {ex.ToString()}"); 
            }
        }

        private static void CloseTwitchOwl()
        {
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            if (procsChrome.Length <= 0)
            {
                Console.WriteLine("Chrome is not running");
            }
            else
            {
                foreach (Process proc in procsChrome)
                {
                    // the chrome process must have a window 
                    if (proc.MainWindowHandle == IntPtr.Zero)
                    {
                        continue;
                    }
                    // to find the tabs we first need to locate something reliable - the 'New Tab' button 
                    AutomationElement root = AutomationElement.FromHandle(proc.MainWindowHandle);
                    Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, "New Tab");
                    AutomationElement elmNewTab = root.FindFirst(TreeScope.Descendants, condNewTab);
                    // get the tabstrip by getting the parent of the 'new tab' button 
                    TreeWalker treewalker = TreeWalker.ControlViewWalker;
                    AutomationElement elmTabStrip = treewalker.GetParent(elmNewTab);
                    // loop through all the tabs and get the names which is the page title 
                    Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
                    foreach (AutomationElement tabitem in elmTabStrip.FindAll(TreeScope.Children, condTabItem))
                    {
                        Condition condEnabled = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                        AutomationElementCollection elmTabChildren = tabitem.FindAll(TreeScope.Children, condEnabled);
                        if (tabitem.Current.Name.Contains("OverwatchLeague - Twitch"))
                        {
                            Point curCursorPosition = System.Windows.Forms.Cursor.Position;

                            System.Windows.Forms.Cursor.Position = new Point((int)elmTabChildren[2].Current.BoundingRectangle.X + 12, (int)elmTabChildren[2].Current.BoundingRectangle.Y + 15);                            
                            WindowsInput.InputSimulator inputSim = new WindowsInput.InputSimulator();
                            inputSim.Mouse.LeftButtonClick();

                            System.Windows.Forms.Cursor.Position = curCursorPosition;
                        }
                    }
                }
            }
        }        

        private static string[] GetSessionStats()
        {
            string runtime = Math.Floor(((_useSimulatedTime ? SIMULATED_TIME : DateTime.Now) -  programStartTime).TotalMinutes) + " minutes";
            string matchesWatched = "not implemented yet";
            string[] stats = {
                $"Runtime: {runtime}",
                $"Matches watched: {matchesWatched}",
                $"Match length currently set at: {MATCH_LENGTH.TotalMinutes} minutes",
                $"Auto-run: {(_autoRunEnabled ? "enabled" : "disabled")}",
            };
            return stats;
        }

        private static void OpenTwitchOwl()
        {
            Process.Start("http://twitch.tv/OverwatchLeague");
        }

        private async static Task SetCurrentSchedule()
        {
            _currentSchedule = await GetOwlSchedule();
        }

        private static void DisplayGametimes()
        {
            Match nextGame = GetNextGame(_currentSchedule);
            string team1 = nextGame.Competitors[0].Name;
            string team2 = nextGame.Competitors[1].Name;
            DateTime gametime = nextGame.StartDate.LocalDateTime;
            UserTextFeedback.ConsoleOut($"The next game is going to be {team1} vs {team2} at {gametime.ToString()}");

            Match mostRecentGame = GetMostRecentGame(_currentSchedule);
            string mrgTeam1 = mostRecentGame.Competitors[0].Name;
            string mrgTeam2 = mostRecentGame.Competitors[1].Name;
            DateTime mrgGametime = mostRecentGame.StartDate.LocalDateTime;
            UserTextFeedback.ConsoleOut($"The most recent game was {mrgTeam1} vs {mrgTeam2} at {mrgGametime.ToString()}");
        }

        private async static Task<OwlSchedule> GetOwlSchedule()
        {
            OwlApi api = new OwlApi();
            HttpResponseMessage response = await api.GetOwlSchedule();
            string responseString = await response.Content.ReadAsStringAsync();
            OwlSchedule schedule = OwlSchedule.FromJson(responseString);
            return schedule;
        }

        private static Match GetNextGame(OwlSchedule schedule)
        {
            Match nextMatch = new Match();
            DateTime nextGame = new DateTime(3000, 1, 1);
            foreach(Stage stage in schedule.Data.Stages)
            {
                foreach(Match game in stage.Matches)
                {
                    DateTime gametime = game.StartDate.LocalDateTime;
                    if (gametime > (_useSimulatedTime ? SIMULATED_TIME : DateTime.Now) && gametime < nextGame)
                    {
                        nextGame = gametime;
                        nextMatch = game;
                    }
                }
            }
            return nextMatch;
        }

        private static Match GetMostRecentGame(OwlSchedule schedule)
        {
            Match mostRecentGame = new Match();
            DateTime mostRecentGameTime = new DateTime(2000, 1, 1);
            foreach (Stage stage in schedule.Data.Stages)
            {
                foreach (Match game in stage.Matches)
                {
                    DateTime gametime = game.StartDate.LocalDateTime;
                    if (gametime <= (_useSimulatedTime ? SIMULATED_TIME : DateTime.Now) && gametime > mostRecentGameTime)
                    {
                        mostRecentGameTime = gametime;
                        mostRecentGame = game;
                    }
                }
            }
            return mostRecentGame;
        }
    }
}
