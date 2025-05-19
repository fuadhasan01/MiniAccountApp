using System;
using System.Collections.Generic;

namespace MiniAccountApp.Models
{
    public class AccountTreeNode
    {
        public Guid AccountId { get; set; }             
        public string DisplayCode { get; set; }         
        public string AccountName { get; set; }        
        public string AccountCode { get; set; }         
        public List<AccountTreeNode> Children { get; set; } = new List<AccountTreeNode>();  
    }
}
