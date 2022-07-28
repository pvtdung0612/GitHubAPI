using MahApps.Metro.Controls;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private string user;
        private string token;
        private GitHubClient client;
        private int countNevigatorPageStore;
        private string storeScreenWillVisible;
        private string storeScreenWillCollapsed;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            countNevigatorPageStore = 0; // equivalent homeScreen and no have page in viewSceen
            storeScreenWillVisible = "loginScreen";// [0] will Collapsed __ [1] will Visible
            storeScreenWillCollapsed = "loginScreen";
            Panel.SetZIndex(loginSceen, 2);

            ProductHeaderValue productInfo = new ProductHeaderValue(appName);
            client = new GitHubClient(productInfo)
            {
                Credentials = new Credentials("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")
            };
            this.ResizeMode = ResizeMode.CanResize;
        }

        private void hS_cbxOptionAuthenticated_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (hs_panelInputToken != null)
            {
                if (comboBox.SelectedItem != null && ((ComboBoxItem)comboBox.SelectedItem).Content.ToString() == "OAuth token authentication")
                    hs_panelInputToken.Visibility = Visibility.Visible;
                else hs_panelInputToken.Visibility = Visibility.Collapsed;
            }
        }

        public GitHubClient Connect()
        {
            ProductHeaderValue productInfo = new ProductHeaderValue(appName);
            GitHubClient client = new GitHubClient(productInfo);
            return client;
        }

        public GitHubClient Connect(string token)
        {
            ProductHeaderValue productInfo = new ProductHeaderValue(appName);
            GitHubClient client = new GitHubClient(productInfo)
            {
                Credentials = new Credentials(token)
            };
            return client;
        }

        private async void Btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mode = ((ComboBoxItem) hS_cbxOptionAuthenticated.SelectedItem).Content.ToString();
                user = hs_pi_textBoxUser.Text.Trim();
                if (!string.IsNullOrEmpty(user))
                {
                    if (mode == "Unauthenticated")
                    {
                        client = Connect();
                    }
                    else
                    {
                        token = hs_pi_textBoxToken.Text.Trim();
                        if (!String.IsNullOrEmpty(token)) client = Connect(token);
                    }
                }
                if (client == null) return;

                InitContainScreen();

            } catch (Exception ex) {
                MessageBox.Show("Can't connect");
                return;
            }
        }

        public void InitContainScreen()
        {
            this.ResizeMode = ResizeMode.CanResize;
            this.Width = 800;
            this.Height = 600;

            Panel.SetZIndex(loginSceen, -1);

            RefreshScreen("loginSceen", "containScreen");
            RefreshRateLimit();
        }

        public void RefreshScreen(string gridCollapsedName, string gridVisibleName)
        {
            Grid gridCollapsed = null;
            Grid gridVisible = null;
            switch (gridCollapsedName)
            {
                case "loginScreen":
                    gridCollapsed = (Grid)this.FindName("loginScreen");
                    break;
                case "containScreen":
                    gridCollapsed = (Grid)this.FindName("containScreen");
                    break;
                case "vs_content":
                    gridCollapsed = (Grid)this.FindName("vs_content");
                    break;
                case "viewScreen":
                    gridCollapsed = (Grid)this.FindName("viewScreen");
                    break;
                case "vs_c_commandLine":
                    gridCollapsed = (Grid)this.FindName("vs_c_commandLine");
                    break;
                case "vs_c_repository":
                    gridCollapsed = (Grid)this.FindName("vs_c_repository");
                    break;
                case "vs_c_cmd_checkCommitScreen":
                    gridCollapsed = (Grid)this.FindName("vs_c_cmd_checkCommitScreen");
                    break;
                default:
                    gridCollapsed = null;
                    break;
            }
            switch (gridVisibleName)
            {
                case "loginScreen":
                    gridVisible = (Grid)this.FindName("loginScreen");
                    break;
                case "containScreen":
                    gridVisible = (Grid)this.FindName("containScreen");
                    break;
                case "vs_content":
                    gridVisible = (Grid)this.FindName("vs_content");
                    break;
                case "viewScreen":
                    gridVisible = (Grid)this.FindName("viewScreen");
                    break;
                case "vs_c_commandLine":
                    gridVisible = (Grid)this.FindName("vs_c_commandLine");
                    break;
                case "vs_c_repository":
                    gridVisible = (Grid)this.FindName("vs_c_repository");
                    break;
                case "vs_c_cmd_checkCommitScreen":
                    gridVisible = (Grid)this.FindName("vs_c_cmd_checkCommitScreen");
                    break;
                default:
                    gridVisible = null;
                    break;
            }
            if (gridCollapsed != null) Panel.SetZIndex(gridCollapsed, -1);
            if (gridVisible != null) Panel.SetZIndex(gridVisible, 1);
        }

        public void RefreshRateLimit()
        {
            RateLimit rateLimit = GetRateLimit();
            vs_rl_tblLimit.Text = rateLimit.Limit.ToString();
            vs_rl_tblRemaining.Text = rateLimit.Remaining.ToString();
            vs_rl_tblReset.Text = rateLimit.Reset.ToString();
        }

        private void cbxCommandsLine_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (storeScreenWillVisible != "vs_c_commandLine")
            {
                storeScreenWillCollapsed = storeScreenWillVisible;
                RefreshScreen(storeScreenWillCollapsed, "vs_c_commandLine");
            }
            storeScreenWillVisible = "vs_c_commandLine";

            cmd_cc_tbxRepoInput.Text = null;
            cmd_cc_lsvShowCommit.ItemsSource = null;
        }

        private void vs_c_cbxCommandLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex == 0)
            {
                cmd_cc_tbxRepoInput.Text = null;
                vs_c_cmd_checkCommitScreen.Visibility = Visibility.Visible;
            }
            else
            {
                cmd_cc_tbxRepoInput.Text = null;
                vs_c_cmd_checkCommitScreen.Visibility = Visibility.Collapsed;
            }
        }

        private void cmd_cc_btnCheck_Click(object sender, RoutedEventArgs e)
        {
            string repoName = cmd_cc_tbxRepoInput.Text.Trim();
            ObservableCollection<string> listCommitsResult = CMD_GetCheckedDifferentCommitsBetweenForkInputAndForkRelate(client, user, repoName);
            //ObservableCollection<int> listCommitsResult = new ObservableCollection<int>()
            //{
            //    1,2,3,4,5,6,7,7,8,9,0,4,2,2,4,5,6,34,3,3,5,76,7,3,2,2,5,4
            //};
            if (listCommitsResult != null) cmd_cc_lsvShowCommit.ItemsSource = listCommitsResult;
            else cmd_cc_lsvShowCommit.Items.Add(new ListBoxItem() { Content = "null" });

            RefreshRateLimit();
        }

        private void trwRepository_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (storeScreenWillVisible != "vs_c_repository")
            {
                storeScreenWillCollapsed = storeScreenWillVisible;
                RefreshScreen(storeScreenWillCollapsed, "vs_c_repository");
            }
            RefreshRateLimit();
            storeScreenWillVisible = "vs_c_repository";

            vs_c_r_lbxAllRepository.ItemsSource = null;
            vs_c_r_lbxAllRepository.Visibility = Visibility.Visible;
            vs_c_r_lbxAllRepository.ItemsSource = client.Repository.GetAllForCurrent().GetAwaiter().GetResult().ToList();

        }

        private void trvBack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (countNevigatorPageStore == 1 || countNevigatorPageStore == 0)
            //{
            //    countNevigatorPageStore = 0;
            //    this.Width = 400;
            //    this.Height = 500;
            //    this.ResizeMode = ResizeMode.NoResize;
            //    Panel.SetZIndex(loginSceen, 100);
            //}
            RefreshScreen("containScreen", "loginScreen");
        }

        //////////////////// BackEnd  ////////////////////////////////////////////

        public RateLimit GetRateLimit()
        {
            if (client != null)
            {
                var miscellaneousRateLimit = client.Miscellaneous.GetRateLimits().GetAwaiter().GetResult();

                //  The "core" object provides your rate limit status except for the Search API.
                var coreRateLimit = miscellaneousRateLimit.Resources.Core;
                return coreRateLimit;
            }
            return null;
        }

        public ObservableCollection<string> CMD_GetCheckedDifferentCommitsBetweenForkInputAndForkRelate(GitHubClient client, string user, string repoName)
        {
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();

            // get commits in repo input
            List<GitHubCommit> list1 = GetRepositoryAllBranchsCommits(client, user, repoName);
            if (list1 == null) return null;

            // get commits in repo fork from repo input or fork from origin repo different repo input
            List<GitHubCommit> list2 = new List<GitHubCommit>();
            // get commits repo fork from origin repo
            Repository repoParent = (client.Repository.Get(user, repoName)).GetAwaiter().GetResult().Parent;
            if (repoParent is null) return null;
            foreach (var item in client.Repository.Forks.GetAll(user, repoParent.Name).GetAwaiter().GetResult())
            {
                if (item.Name != repoName)
                {
                    list2.AddRange(GetRepositoryAllBranchsCommits(client, user, repoName));
                }
            }
            // get commits repo fork from repo input
            foreach (var item in client.Repository.Forks.GetAll(user, repoName).GetAwaiter().GetResult())
            {
                list2.AddRange(GetRepositoryAllBranchsCommits(client, user, repoName));
            }

            List<GitHubCommit> listExcept = list1.Except(list2).ToList<GitHubCommit>();
            if (listExcept == null) return null;
            ObservableCollection<string> listCommitsResult = new ObservableCollection<string>();
            foreach (var item in listExcept)
            {
                if (item.Commit != null)
                    listCommitsResult.Add(item.Commit.Message);
            }

            return listCommitsResult;
        }

        /// <summary>
        /// get all commits of all branchs of repo input
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public List<GitHubCommit> GetRepositoryAllBranchsCommits(GitHubClient client, string user, string repoName)
        {
            try
            {
                List<GitHubCommit> listCommits = new List<GitHubCommit>();
                IRepositoryCommitsClient _fixture = client.Repository.Commit;

                IReadOnlyList<Branch> branchs = client.Repository.Branch.GetAll(user, repoName).GetAwaiter().GetResult();
                foreach (var item in branchs)
                {
                    //IReadOnlyList<GitHubCommit> listCommit = await _fixture.GetAll(user, repoName, new CommitRequest { Sha = item.Commit.Sha });
                    listCommits.AddRange(_fixture.GetAll(user, repoName, new CommitRequest { Sha = item.Commit.Sha }).GetAwaiter().GetResult());
                }

                return listCommits;
            }
            catch (Exception exp)
            {
                return null;
            }
        }
    }
}
