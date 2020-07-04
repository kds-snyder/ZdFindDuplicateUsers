using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZdHelpDeskSync.Models.TicketFields;

namespace ZdHelpDeskSync.ZdFunctions
{
    public static class TicketFields
    {
        // Create ticket field
        public static ZdTicketFieldCreate CreateTicketField(string baseUrl, string apiCredentials, ZdTicketFieldCreate ticketField)
        {
            ZdTicketFieldCreate createdTicketField = null;
            var jsonBody = JsonConvert.SerializeObject(ticketField);
            IRestResponse response;

            response = RestHelperFunctions.SendRestRequest(baseUrl, apiCredentials, "api/v2/ticket_fields.json", System.Net.HttpStatusCode.Created, "creating ticket field", jsonBody, Method.POST);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                createdTicketField = JsonConvert.DeserializeObject<ZdTicketFieldCreate>(response.Content);
            }

            return createdTicketField;
        }

        // Create ZdTicketFieldCreate object from source ticket field 
        public static ZdTicketFieldCreate CreateTicketFieldObj (ZdTicketField sourceTicketField)
        {
            var targetTicketFieldObj = new ZdTicketField
            {
                Type = sourceTicketField.Type,
                Title = sourceTicketField.Title,
                RawTitle = sourceTicketField.RawTitle,
                Description = sourceTicketField.Description,
                RawDescription = sourceTicketField.RawDescription,
                Active = sourceTicketField.Active,
                TicketFieldRequired = sourceTicketField.TicketFieldRequired,
                CollapsedForAgents = sourceTicketField.CollapsedForAgents,
                RegexpForValidation = sourceTicketField.RegexpForValidation,
                TitleInPortal = sourceTicketField.TitleInPortal,
                RawTitleInPortal = sourceTicketField.RawTitleInPortal,
                VisibleInPortal = sourceTicketField.VisibleInPortal,
                EditableInPortal = sourceTicketField.EditableInPortal,
                RequiredInPortal = sourceTicketField.RequiredInPortal,
                Tag = sourceTicketField.Tag,
                Removable = sourceTicketField.Removable,
                AgentDescription = sourceTicketField.AgentDescription               
            };

            if (!(sourceTicketField.CustomFieldOptions is null))
            {
                targetTicketFieldObj.CustomFieldOptions = sourceTicketField.CustomFieldOptions.Select(x => new CustomFieldOption()
                {
                    Name = x.Name,
                    Value = x.Value
                }).ToList();
            }

            return new ZdTicketFieldCreate
            {
                TicketField = targetTicketFieldObj
            };
        }

        // Get ticket fields
        public static ZdTicketFieldsGet GetTicketFields(string baseUrl, string apiCredentials)
        {
            var response = RestHelperFunctions.SendRestRequest(baseUrl, apiCredentials, "api/v2/ticket_fields.json", System.Net.HttpStatusCode.OK, "getting ticket fields");
            ZdTicketFieldsGet result = null;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                result = JsonConvert.DeserializeObject<ZdTicketFieldsGet>(response.Content);
            }

            return result;
        }

        // Copy ticket fields from source to target
        public static void CopyTicketFields(string baseUrlSource, string apiCredentialsSource, string baseUrlTarget, string apiCredentialsTarget, ZdTicketFieldsGet ticketFieldsSource, ZdTicketFieldsGet ticketFieldsTarget)
        {
            long numDuplicateTicketFieldNames = 0;
            long ticketFieldCounter = 0;
            var ticketFieldNames = new List<string>();
            long numTicketFieldsCopied = 0;
            long numTicketFieldsNotCopiedInactive = 0;
            long numTicketFieldsNotCopiedExistInTarget = 0;
            long numTicketFieldsNotCopiedSystem = 0;
            var syncTimer = Stopwatch.StartNew();

            foreach (ZdTicketField ticketFieldToCopy in ticketFieldsSource.TicketFields)
            {
                ++ticketFieldCounter;
                Console.Write($"{ticketFieldCounter}. {ticketFieldToCopy.Title}: ");

                // Output warning if ticket field name is duplicated
                if (ticketFieldNames.Contains(ticketFieldToCopy.Title.Trim())) {
                    ++numDuplicateTicketFieldNames;
                    Console.Write("Ticket field name duplicated in source, may cause sync problems. ");
                }
                else
                {
                    ticketFieldNames.Add(ticketFieldToCopy.Title.Trim());
                }
               
                // Do not copy system fields (not removable or system field options exist)
                if (!ticketFieldToCopy.Removable || !(ticketFieldToCopy.SystemFieldOptions is null)) {
                    Console.WriteLine($"Not copied because it is system field");
                    ++numTicketFieldsNotCopiedSystem;
                    continue;
                }

                // Do not copy inactive fields
                if (!ticketFieldToCopy.Active)
                {
                    Console.WriteLine($"Not copied because it is not active (source ticket field ID: {ticketFieldToCopy.Id}, type: {ticketFieldToCopy.Type})");
                    ++numTicketFieldsNotCopiedInactive;
                    continue;
                }

                // Do not copy source ticket field if target ticket field exists with same name
                var foundTargetTicketField = ticketFieldsTarget.TicketFields.FirstOrDefault(x => x.Active && (x.Title == ticketFieldToCopy.Title));
                if (!(foundTargetTicketField is null))
                {
                    Console.WriteLine($"Not copied because already exists in target with ticket field ID: {foundTargetTicketField.Id}, (source ticket field ID: {ticketFieldToCopy.Id}, type: {ticketFieldToCopy.Type})");
                    ++numTicketFieldsNotCopiedExistInTarget;
                    continue;
                }

                var ticketFieldToCreate = CreateTicketFieldObj(ticketFieldToCopy);

                var createdTicketField = CreateTicketField(baseUrlTarget, apiCredentialsTarget, ticketFieldToCreate);
                if (!(createdTicketField is null))
                {
                    ++numTicketFieldsCopied;
                    Console.WriteLine($"Copied to target with ticket field ID: {createdTicketField.TicketField.Id} (source ticket field ID: {ticketFieldToCopy.Id}, type: {ticketFieldToCopy.Type})");
                }
            }
            syncTimer.Stop();
            TimeSpan ts = syncTimer.Elapsed;

            // Display metrics
            Console.WriteLine("");
            Console.WriteLine("Completed ticket field sync");
            Console.WriteLine($"Total # ticket fields copied to target: {numTicketFieldsCopied}");
            Console.WriteLine($"Total # ticket fields not copied to target because they are inactive: {numTicketFieldsNotCopiedInactive}");
            Console.WriteLine($"Total # ticket fields not copied to target because they are system fields: {numTicketFieldsNotCopiedSystem}");
            Console.WriteLine($"Total # ticket fields not copied to target because ticket field with same name already exists in target: {numTicketFieldsNotCopiedExistInTarget}");
            Console.WriteLine($"Total # duplicate ticket field names in source: {numDuplicateTicketFieldNames}");
            Console.WriteLine($"Ticket field sync duration time: {ts.Hours} hours, {ts.Minutes} minutes, {ts.Seconds} seconds, {ts.Milliseconds} milliseconds");
        }

        // Sync ticket fields from source to target
        public static void SyncTicketFields(string baseUrlSource, string apiCredentialsSource, string baseUrlTarget, string apiCredentialsTarget)
        {
            Console.WriteLine("Starting ticket field sync");

            // Read source ticket fields
            Console.Write("Getting source ticket fields: ");
            ZdTicketFieldsGet ticketFieldsSource = GetTicketFields(baseUrlSource, apiCredentialsSource);
            Console.WriteLine($"Read {ticketFieldsSource.TicketFields.Count} ticket fields");

            // Read target ticket fields
            Console.Write("Getting target ticket fields: ");
            ZdTicketFieldsGet ticketFieldsTarget = GetTicketFields(baseUrlTarget, apiCredentialsTarget);
            Console.WriteLine($"Read {ticketFieldsTarget.TicketFields.Count} ticket fields");

            // Copy ticket fields from source to target
            CopyTicketFields(baseUrlSource, apiCredentialsSource, baseUrlTarget, apiCredentialsTarget, ticketFieldsSource, ticketFieldsTarget);
        }

    }
}
