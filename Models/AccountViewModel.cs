using System.Collections.Generic;

namespace IncognitoReads.Models
{
    public class AccountViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string PrimaryEmail { get; set; } = string.Empty;
        public List<string> EmailAddresses { get; set; }  = new List<string>();
        public List<ConnectedAccount> ConnectedAccounts { get; set; } = new List<ConnectedAccount>();
    }

    public class ConnectedAccount
    {
        public string Provider { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool RequiresAction { get; set; }
    }
}
