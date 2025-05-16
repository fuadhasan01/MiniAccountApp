using System;
using System.Collections.Generic;

namespace MiniAccountApp.Models
{
    public class AccountTreeNode
    {
        public Guid AccountId { get; set; }             // Unique ID from DB
        public string DisplayCode { get; set; }         // Short display code for UI, e.g., "1", "2"
        public string AccountName { get; set; }         // e.g., "Assets"
        public string AccountCode { get; set; }         // Actual account code stored in DB (e.g., "1")
        public List<AccountTreeNode> Children { get; set; } = new List<AccountTreeNode>();  // Children nodes
    }
}
