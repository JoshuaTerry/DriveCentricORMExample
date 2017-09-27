﻿using DriveCentric.Shared.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveCentric.Shared.Models.Security
{
    [Table("UserClaims")]
    public class UserClaim : IdentityUserClaim<Guid>, IEntity 
    {
        [Key] 
        public new Guid Id { get; set; } 
        [MaxLength(64)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [NotMapped]
        public string DisplayName { get; set; }
        [MaxLength(64)]
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        

    }
}
