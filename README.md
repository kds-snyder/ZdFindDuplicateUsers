# ZdFindDuplicateUsers

This program uses the Zendesk User API to read in all users, finds users with more than record with same user name,
and outputs an Excel file with a list of the duplicate users.

Basic authentication with an email address and API token is used as explained in https://developer.zendesk.com/rest_api/docs/support/introduction#api-token.

The Microsoft Open XML SDK is used to create the Excel file, as explained in:
https://docs.microsoft.com/en-us/office/open-xml/how-to-create-a-spreadsheet-document-by-providing-a-file-name

When you use the program, you will be prompted to input the following:
1. Support API base (e.g. https://xyz.zendesk.com)
2. Email address of a Zendesk user who is an admin or agent
3. API token for Zendesk
4. Output Excel file name
5. Output Excel file sheet name

To run the program, open it in Visual Studio and then enter CTRL + F5. The program will open a console window and
start prompting for input. It will output the result page number to show the progress, and then will output statistics
when it completes.