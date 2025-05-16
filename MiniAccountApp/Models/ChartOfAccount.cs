using System;
using System.ComponentModel.DataAnnotations;

namespace MiniAccountApp.Models
{
    public class ChartOfAccount
    {
        public Guid AccountId { get; set; }

        public string? AccountCode { get; set; }
        
        [Required(ErrorMessage = "Account Name is required")]
        [MaxLength(150)]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Account Type is required")]
        [MaxLength(50)]
        public string AccountType { get; set; }

        [MaxLength(50)]
        public string? AccountCategory { get; set; } 

        public Guid? ParentAccountId { get; set; }

        [Required]
        public int AccountLevel { get; set; } = 1;

        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50)]
        public string IsActive { get; set; }

        [MaxLength(20)]
        public string? AccountNature { get; set; }
        
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }       

        public DateTime CreatedDate { get; set; }    

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }



        // These properties are used for view purposes only
        public List<ChartOfAccount> Children { get; set; } = new();
        public string? ParentAccountName { get; set; }

    }
}
