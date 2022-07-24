using Octokit;
using System.Diagnostics;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private static readonly string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private static string user;
        private static string token;
        private static string nameMainRepo;
        private static GitHubClient client;

        public static void Main(string[] args)
        {
            user = "pvtdung0612";
            token = "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";
            nameMainRepo = "moonlight-embedded";
            try
            {
                client = InitClient(token).GetAwaiter().GetResult();

                if (client is null) return;

                GetRateLimit().GetAwaiter().GetResult();

                // Test().GetAwaiter().GetResult();
                Run(client, user, "BigExercise1_Dictionary").GetAwaiter().GetResult();

            } catch (Exception exp) {
                Console.WriteLine($"Error: {exp.Message}");
            }
        }

        /// <summary>
        /// InitClient without OAuthorization
        /// </summary>
        /// <returns></returns>
        public static async Task<GitHubClient> InitClient()
        {
            Console.WriteLine(appName + @" setting client to https://github.com/" + user + " ...");

            ProductHeaderValue productInfo = new ProductHeaderValue(appName);
            GitHubClient client = new GitHubClient(productInfo);

            return client;
        }

        /// <summary>
        /// InitClient with token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<GitHubClient> InitClient(string token)
        {
            Console.WriteLine(appName + @" setting client to https://github.com/" + user + " ...");

            ProductHeaderValue productInfo = new ProductHeaderValue(appName);
            GitHubClient client = new GitHubClient(productInfo)
            {
                Credentials = new Credentials(token)
            };

            return client;
        }

        /// <summary>
        /// RateLimit Access or Search to GitHubAPI
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> GetRateLimit()
        {
            Console.WriteLine("GetRateLimit()");
            if (client != null)
            {
                //// 01
                //// Prior to first API call, this will be null, because it only deals with the last call.
                //var apiInfo = client.GetLastApiInfo();

                //// If the ApiInfo isn't null, there will be a property called RateLimit
                //var rateLimit = apiInfo?.RateLimit;

                //var howManyRequestsCanIMakePerHour = rateLimit?.Limit;
                //var howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
                //var whenDoesTheLimitReset = rateLimit?.Reset; // UTC time

                //Console.WriteLine(client);
                //Console.WriteLine(rateLimit);
                //Console.WriteLine($"01 - Limit: {howManyRequestsCanIMakePerHour} Remaining: {howManyRequestsDoIHaveLeft} Reset: {whenDoesTheLimitReset}");

                // 02
                var miscellaneousRateLimit = await client.Miscellaneous.GetRateLimits();

                //  The "core" object provides your rate limit status except for the Search API.
                var coreRateLimit = miscellaneousRateLimit.Resources.Core;

                var howManyCoreRequestsCanIMakePerHour = coreRateLimit.Limit;
                var howManyCoreRequestsDoIHaveLeft = coreRateLimit.Remaining;
                var whenDoesTheCoreLimitReset = coreRateLimit.Reset; // UTC time

                Console.WriteLine($"coreRateLimit - Limit: {coreRateLimit.Limit} Remaining: {coreRateLimit.Remaining} Reset: {coreRateLimit.Reset}");

                // the "search" object provides your rate limit status for the Search API.
                var searchRateLimit = miscellaneousRateLimit.Resources.Search;

                var howManySearchRequestsCanIMakePerMinute = searchRateLimit.Limit;
                var howManySearchRequestsDoIHaveLeft = searchRateLimit.Remaining;
                var whenDoesTheSearchLimitReset = searchRateLimit.Reset; // UTC time

                Console.WriteLine($"searchRateLimit - Limit: {searchRateLimit.Limit} Remaining: {searchRateLimit.Remaining} Reset: {searchRateLimit.Reset}");
            }
            return true;
        }

        /// <summary>
        /// get current User which client access
        /// </summary>
        /// <returns></returns>
        public static async Task<User> GetUser()
        {
            Console.WriteLine("GetUser()");
            var userCurrent = await client.User.Current();

            return userCurrent;
        }

        /// <summary>
        /// get any User
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public static async Task<User> GetUser(string userLogin)
        {
            Console.WriteLine("GetUser()");

            var userChoosed = await client.User.Get(userLogin);

            return userChoosed;
        }

        /// <summary>
        /// get a Repository
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public static async Task<Repository> GetRepository(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetRepository()");

            Repository repo = await client.Repository.Get(user, repoName);

            return repo;
        }

        public static async Task<IReadOnlyList<Repository>> GetForks(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetForks()");

            IReadOnlyList<Repository> allForks = await client.Repository.Forks.GetAll(user, repoName);

            return allForks;
        }

        public static async Task<Repository> GetParentRepository(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetParentRepository()");

            Repository repo = await client.Repository.Get(user, repoName);

            return repo.Parent;
        }

        /// <summary>
        /// get all branchs of repo input
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyList<Branch>> GetBranchs(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetBranchs()");

            IReadOnlyList<Branch> branchs = await client.Repository.Branch.GetAll(user, repoName);

            return branchs;
        }

        /// <summary>
        /// get all commits of all branchs of repo input
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public static async Task<List<GitHubCommit>> GetRepositoryAllBranchsCommits(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetRepositoryAllBranchsCommits()");

            List<GitHubCommit> listCommits = new List<GitHubCommit>();
            IRepositoryCommitsClient _fixture = client.Repository.Commit;

            IReadOnlyList<Branch> branchs = await client.Repository.Branch.GetAll(user, repoName);
            foreach (var item in branchs)
            {
                //IReadOnlyList<GitHubCommit> listCommit = await _fixture.GetAll(user, repoName, new CommitRequest { Sha = item.Commit.Sha });
                listCommits.AddRange(await _fixture.GetAll(user, repoName, new CommitRequest { Sha = item.Commit.Sha }));
            }

            return listCommits;
        }

        /// <summary>
        /// get commits of default branchs of repo input
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public static async Task<List<GitHubCommit>> GetRepositoryDefaultBranchsCommits(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetRepositoryAllBranchsCommits()");

            IRepositoryCommitsClient _fixture = client.Repository.Commit;

            return (List<GitHubCommit>)await _fixture.GetAll(user, repoName);
        }

        /// <summary>
        /// get commits of a branch of repo input
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="repoName"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public static async Task<List<GitHubCommit>> GetRepositoryOneBranchsCommits(GitHubClient client, string user, string repoName, Branch branch)
        {
            Console.WriteLine("GetRepositoryAllBranchsCommits()");

            IRepositoryCommitsClient _fixture = client.Repository.Commit;

            return (List<GitHubCommit>)await _fixture.GetAll(user, repoName, new CommitRequest() { Sha = branch.Commit.Sha });
        }

        public static async Task<bool> Run(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("Run()");

            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();

            // get commits in repo input
            List<GitHubCommit> list1 = await GetRepositoryAllBranchsCommits(client, user, repoName);
            if (list1 == null) return false;
            Console.WriteLine("check1");

            // get commits in repo fork from repo input or fork from origin repo different repo input
            List<GitHubCommit> list2 = new List<GitHubCommit>();
            // get commits repo fork from origin repo
            Repository repoParent = (await client.Repository.Get(user, repoName)).Parent;
            if (repoParent is null) return false;
            foreach (var item in await client.Repository.Forks.GetAll(user, repoParent.Name))
            {
                if (item.Name != repoName)
                {
                    list2.AddRange(await GetRepositoryAllBranchsCommits(client, user, repoName));
                }
            }
            Console.WriteLine("check2");
            // get commits repo fork from repo input
            foreach (var item in await client.Repository.Forks.GetAll(user, repoName))
            {
                list2.AddRange(await GetRepositoryAllBranchsCommits(client, user, repoName));
            }
            Console.WriteLine("check3");

            List<GitHubCommit> listExcept = list1.Except(list2).ToList<GitHubCommit>();
            Console.WriteLine("check4");
            foreach (var item in listExcept)
            {
                if (item.Author != null && item.Commit != null && item.Sha != null)
                    Console.WriteLine($"Owner: {item.Author.Login} Message: {item.Commit.Message} Sha: {item.Sha}");
            }
            Console.WriteLine("check5");
            Console.WriteLine($"Count: {listExcept.Count}");

            watch.Stop();
            Console.WriteLine("Program time: " + watch.Elapsed);
            return true;
        }

        // testing ... get commits all branch
        public static async Task<bool> Test()
        {
            Console.WriteLine("Test()");

            Stopwatch sw =  Stopwatch.StartNew();
            List<string> strings1 = new List<string>()
            {
                "1", "2", "3", "4", "5", "6", 
            };
            List<string> strings2 = new List<string>()
            {
                "5", "6", "7", "8", "9", "10",
            };

            sw.Start();
            foreach (var item in strings1.Except(strings2))
            {
                Console.WriteLine(item);
            }
            sw.Stop();
            Console.WriteLine("Time: " + sw.Elapsed);
            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Code below are being developed, has been checked yet ///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public static async Task<bool> GetAllRepositoryPublic()
        {
            Console.WriteLine("GetAllRepositoryPublic");
            IReadOnlyList<Repository> listRepo = await client.Repository.GetAllPublic();
            Console.WriteLine("Count: " + listRepo.Count);
            foreach (var item in listRepo)
            {
                Console.WriteLine(item.Owner.Login +"/"+ item.FullName);
            }

            return true;
        }

        public static async void UseSearchCode()
        {
            SearchCodeResult result = await client.Search.SearchCode(
            new SearchCodeRequest("issue")
            {
                In = new CodeInQualifier[] { CodeInQualifier.Path },
                Language = Language.CSharp,
                Repos = new RepositoryCollection { "octokit/octokit.net" }
            });
            Console.WriteLine($"Search.SearchCode (Simple Search): TotalCode={result.TotalCount}");
            Console.WriteLine($"Search.SearchCode.ToList (Simple Search):");

            foreach (var item in result.Items.ToList())
            {
                Console.WriteLine(item.HtmlUrl);
            }
        }

        public static async void UseSearchUser()
        {
            Console.WriteLine("user");
            Console.WriteLine("stop");

            SearchRepositoryResult result_test01 = await client.Search.SearchRepo(
                new SearchRepositoriesRequest("octokit")
                {
                    In = new InQualifier[] { InQualifier.Name }
                });

            SearchRepositoryResult result_test02 = await client.Search.SearchRepo(
                new SearchRepositoriesRequest("octokit")
                {
                    In = new InQualifier[] { InQualifier.Name }
                });

            SearchUsersResult result = await client.Search.SearchUsers(
                new SearchUsersRequest("dung")
                {
                    AccountType = AccountSearchType.User
                });

            Console.WriteLine(result.Items.Count);

            foreach (var item in result.Items.ToList())
            {
                Console.WriteLine(1);
                Console.WriteLine(item.Login);
            }
        }
    }
}