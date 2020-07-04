using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ZdFindDuplicateUsers.ZdModels;

namespace ZdFindDuplicateUsers
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get input variables
            Console.Write("Enter Zendesk Support API base (e.g. https://xyz.zendesk.com): ");
            string userApiBase = Console.ReadLine();
            Console.Write("Enter email address of a Zendesk admin or agent: ");
            string emailAddress = Console.ReadLine();
            Console.Write("Enter Zendesk API token: ");
            string apiToken = Console.ReadLine();
            Console.Write("Enter output Excel file name: ");
            string excelFileName = Console.ReadLine();
            Console.Write("Enter output Excel file sheet name: ");
            string sheetName = Console.ReadLine();

            var apiCredentials = Convert.ToBase64String(Encoding.Default.GetBytes($"{emailAddress}/token:{apiToken}"));

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Get all users
                var zdUsers = ZdFunctions.Users.GetAllUsers(userApiBase, apiCredentials);

                // Get duplicate users, grouped by name
                var duplicatedUsersGrouped = zdUsers.GroupBy(x => x.Name)
                                            .Where(g => g.Count() > 1)
                                            .OrderBy(g => g.Key);
                // Create output Excel file
                ExcelHelperFunctions.CreateExcelFile(excelFileName, sheetName);

                // Output duplicate users to Excel file
                ExcelHelperFunctions.OutputDuplicatedUsersToExcel(excelFileName, sheetName, duplicatedUsersGrouped);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred, message: {ex.Message}, stackTrace: {ex.StackTrace}");
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine("");
            Console.WriteLine($"Total # user records: {zdUsers.Count()}, # duplicated users: {duplicatedUsersGrouped.Count()}");
            Console.WriteLine($"Program duration time: {ts.Hours} hours, {ts.Minutes} minutes, {ts.Seconds} seconds, {ts.Milliseconds} milliseconds");
            Console.ReadLine();
        }

        
    }
}
