using Newtonsoft.Json;

namespace ContosoBankBot_MSA2017.Models
{
    public class Account
    {
        [JsonProperty(PropertyName = "fullName")]
        public string FullName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public double Balance { get; set; }
    }
}