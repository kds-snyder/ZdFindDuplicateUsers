using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ZdFindDuplicateUsers.ZdModels
{
    public class ZdUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }
        [JsonProperty("iana_time_zone")]
        public string IanaTimeZone { get; set; }
        [JsonProperty("phone")]
        public object Phone { get; set; }
        [JsonProperty("shared_phone_number")]
        public object SharedPhoneNumber { get; set; }
        [JsonProperty("photo")]
        public object Photo { get; set; }
        [JsonProperty("locale_id")]
        public int? LocaleId { get; set; }
        [JsonProperty("locale")]
        public string Locale { get; set; }
        [JsonProperty("organization_id")]
        public long? OrganizationId { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("verified")]
        public bool Verified { get; set; }
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
        [JsonProperty("alias")]
        public object Alias { get; set; }
        [JsonProperty("active")]
        public bool Active { get; set; }
        [JsonProperty("shared")]
        public bool Shared { get; set; }
        [JsonProperty("shared_agent")]
        public bool Shared_agent { get; set; }
        [JsonProperty("last_login_at")]
        public object LastLoginAt { get; set; }
        [JsonProperty("two_factor_auth_enabled")]
        public bool? TwoFactorAuthEnabled { get; set; }
        [JsonProperty("signature")]
        public object Signature { get; set; }
        [JsonProperty("details")]
        public string Details { get; set; }
        [JsonProperty("notes")]
        public object Notes { get; set; }
        [JsonProperty("role_type")]
        public object RoleType { get; set; }
        [JsonProperty("custom_role_id")]
        public object CustomRoleId { get; set; }
        [JsonProperty("moderator")]
        public bool? Moderator { get; set; }
        [JsonProperty("ticket_restriction")]
        public string TicketRestriction { get; set; }
        [JsonProperty("only_private_comments")]
        public bool OnlyPrivateComments { get; set; }
        [JsonProperty("restricted_agent")]
        public bool RestrictedAgent { get; set; }
        [JsonProperty("suspended")]
        public bool Suspended { get; set; }
        [JsonProperty("chat_only")]
        public bool ChatOnly { get; set; }
        [JsonProperty("default_group_id")]
        public object DefaultGroupId { get; set; }
        [JsonProperty("report_csv")]
        public bool ReportCsv { get; set; }
        [JsonProperty("user_fields")]
        public object UserFields { get; set; }
        [JsonProperty("result_type")]
        public string ResultType { get; set; }
    }
}
