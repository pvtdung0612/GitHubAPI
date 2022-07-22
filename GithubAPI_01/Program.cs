using System;
using Octokit;

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
                client = InitClient().GetAwaiter().GetResult();

                if (client is null) return;
                Test().GetAwaiter().GetResult();
                //Run(client, user, nameMainRepo).GetAwaiter().GetResult();

            } catch (Exception exp) {
                Console.WriteLine($"Error: {exp.Message}");
            }
        }

        public static async Task<GitHubClient> InitClient()
        {
            Console.WriteLine(appName + @" setting client to https://github.com/" + user + " ...");

            ProductHeaderValue productInfo = new ProductHeaderValue(appName);
            GitHubClient client = new GitHubClient(productInfo)
            {
                Credentials = new Credentials("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")
            };

            return client;
        }

        public static async Task<bool> GetRateLimit()
        {
            Console.WriteLine("GetRateLimit()");
            if (client != null)
            {
                // 01
                // Prior to first API call, this will be null, because it only deals with the last call.
                var apiInfo = client.GetLastApiInfo();

                // If the ApiInfo isn't null, there will be a property called RateLimit
                var rateLimit = apiInfo?.RateLimit;

                var howManyRequestsCanIMakePerHour = rateLimit?.Limit;
                var howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
                var whenDoesTheLimitReset = rateLimit?.Reset; // UTC time

                Console.WriteLine(client);
                Console.WriteLine(rateLimit);
                Console.WriteLine($"01 - Limit: {howManyRequestsCanIMakePerHour} Remaining: {howManyRequestsDoIHaveLeft} Reset: {whenDoesTheLimitReset}");

                // 02
                var miscellaneousRateLimit = await client.Miscellaneous.GetRateLimits();

                //  The "core" object provides your rate limit status except for the Search API.
                var coreRateLimit = miscellaneousRateLimit.Resources.Core;

                var howManyCoreRequestsCanIMakePerHour = coreRateLimit.Limit;
                var howManyCoreRequestsDoIHaveLeft = coreRateLimit.Remaining;
                var whenDoesTheCoreLimitReset = coreRateLimit.Reset; // UTC time

                // the "search" object provides your rate limit status for the Search API.
                var searchRateLimit = miscellaneousRateLimit.Resources.Search;

                var howManySearchRequestsCanIMakePerMinute = searchRateLimit.Limit;
                var howManySearchRequestsDoIHaveLeft = searchRateLimit.Remaining;
                var whenDoesTheSearchLimitReset = searchRateLimit.Reset; // UTC time

                Console.WriteLine(client);
                Console.WriteLine(searchRateLimit);
                Console.WriteLine($"02 - Limit: {searchRateLimit.Limit} Remaining: {searchRateLimit.Remaining} Reset: {searchRateLimit.Reset}");

            }
            return true;
        }

        public static async Task<User> GetUser()
        {
            Console.WriteLine("GetUser()");
            // GetUserCurrent
            var userCurrent = await client.User.Current();
            Console.WriteLine(userCurrent.Url);

            return userCurrent;
        }

        public static async Task<User> GetUser(string userLogin)
        {
            Console.WriteLine("GetUser()");
            // GetUserChoosed
            var userChoosed = await client.User.Get(userLogin);
            //Console.WriteLine("{0} has {1} public repositories - go check out their profile at {2}",
            //    userChoosed.Login,
            //    userChoosed.PublicRepos,
            //    userChoosed.Url);

            return userChoosed;
        }

        public static async Task<IReadOnlyList<GitHubCommit>> GetCommitsList(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("GetCommitsList()");

            IRepositoryCommitsClient _fixture = client.Repository.Commit;
            IReadOnlyList<GitHubCommit> list = await _fixture.GetAll(user, repoName);

            return list;
        }

        public static async Task<bool> Run(GitHubClient client, string user, string repoName)
        {
            Console.WriteLine("Run()");

            // Test GetParent
            //Repository repo = await client.Repository.Get(user, repoName);
            //Repository repoParent = repo.Parent;
            //Console.WriteLine($"repoOwner: {repo.Owner.Login} repoName: {repo.Name}");
            //Console.WriteLine($"repoParentOwner: {repoParent.Owner.Login} repoParentName: {repoParent.Name}");

            // Start. 
            Repository repo = await client.Repository.Get(user, repoName);
            var list_repo = GetCommitsList(client, user, repoName);

            // RepoParent and RepoFork from RepoParent
            Repository repoParent = repo.Parent;

            return true;
        }

        public static async Task<bool> Test()
        {
            Console.WriteLine("Test()");

            IRepositoryCommitsClient _fixture = client.Repository.Commit;
            var list = await _fixture.GetAll(user, "Test");
            Console.WriteLine($"ListCount: {list.Count}");
            foreach (var item in list)
            {
                Console.WriteLine($"ItemUrl: {item.Commit.Url} ItemMessage: {item.Commit.Message}");
            }
            return true;
        }

        public static async Task<bool> GetFork()
        {
            IReadOnlyList<Repository> allForks = await client.Repository.Forks.GetAll("pvtdung0612", "Game_Tetris");
            Console.WriteLine(allForks.Count);
            foreach (var item in allForks)
            {
                Console.WriteLine(item.FullName);
            }

            return true;
        }

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