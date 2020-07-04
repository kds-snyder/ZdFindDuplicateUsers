using System.Collections.Generic;
using Newtonsoft.Json;

namespace ZdFindDuplicateUsers.ZdModels
{
    public class ZdUsers
    {
        [JsonProperty("users")]
        public IEnumerable<ZdUser> Users { get; set; }
        [JsonProperty("next_page")]
        public string NextPage { get; set; }
        [JsonProperty("previous_page")]
        public string PreviousPage { get; set; }
        [JsonProperty("count")]
        public long Count { get; set; }
    }
}
