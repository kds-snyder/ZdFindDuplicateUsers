using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZdFindDuplicateUsers.ZdModels;

namespace ZdFindDuplicateUsers.HelperFunctions
{
    public static class ConsoleHelperFunctions
    {
        // Output duplicate users to console
        public static void OutputDuplicatedUsersToConsole(IEnumerable<ZdUser> zdUsers, IOrderedEnumerable<IGrouping<string, ZdUser>> duplicatedUsersGrouped)
        {
            if (duplicatedUsersGrouped.Any())
            {
                Console.WriteLine($"Total # user records: {zdUsers.Count()}, # duplicated users: {duplicatedUsersGrouped.Count()}");

                Console.WriteLine("");
                Console.WriteLine("User name\tEmail\tRole\tUpdated");
                bool firstLine;
                foreach (var userGroup in duplicatedUsersGrouped)
                {
                    firstLine = true;
                    foreach (var user in userGroup)
                    {
                        if (firstLine)
                        {
                            Console.WriteLine($"{user.Name}\t{user.Email}\t{user.Role}\t{user.UpdatedAt}");
                            firstLine = false;
                        }
                        else
                        {
                            Console.WriteLine($"\t{user.Email}\t{user.Role}\t{user.UpdatedAt}");
                        }

                    }
                }
            }
        }

        // Redirect console to text file
        public static void RedirectConsoleToTextFile(string outputTextFile)
        {
            var filestream = new FileStream(outputTextFile, FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);
        }
    }
}
