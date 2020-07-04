using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ZdFindDuplicateUsers.HelperFunctions;
using ZdFindDuplicateUsers.ZdModels;

namespace ZdFindDuplicateUsers.ZdFunctions
{
    public static class Users
    {

        // Get ticket fields
        public static ZdUsers GetUsers(string baseUrl, string apiCredentials, string appendResource = "")
        {
            var response = RestHelperFunctions.SendRestRequest(baseUrl, apiCredentials, "api/v2/users.json" + appendResource, System.Net.HttpStatusCode.OK, "getting users");
            ZdUsers result = null;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                result = JsonConvert.DeserializeObject<ZdUsers>(response.Content);
            }
            else
            {
                Console.WriteLine($"Error getting users: {response.StatusCode}");
            }

            return result;
        }

        public static IEnumerable<ZdUser> GetAllUsers(string baseUrl, string apiCredentials)
        {
            IEnumerable<ZdUser> allUsers = Enumerable.Empty<ZdUser>();

            var url = baseUrl;
            bool done = false;
            string appendResource = "";

            while (!done)
            {
                var userBatch = GetUsers(url, apiCredentials, appendResource);
                if (!(userBatch is null))
                {
                    allUsers = allUsers.Concat(userBatch.Users);
                    if (userBatch.NextPage is null)
                    {
                        done = true;
                    }
                    else
                    {
                        appendResource = userBatch.NextPage.Substring(userBatch.NextPage.IndexOf("?page"));
                    }
                }
                else
                {
                    done = true;
                }

            }
            return allUsers;

        }

    }
}
