using Newtonsoft.Json;

namespace ContosoBankBot_MSA2017.Models
{
    public class BankBranch
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "cityName")]
        public string CityName { get; set; }

        [JsonProperty(PropertyName = "branchAddress")]
        public string BranchAddress { get; set; }
    }
}