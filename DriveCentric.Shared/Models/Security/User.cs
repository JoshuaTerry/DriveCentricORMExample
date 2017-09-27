using DriveCentric.Shared.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;


namespace DriveCentric.Shared.Models.Security
{
    [Table("Users")]
    public class User : IdentityUser<Guid, UserLogin, UserRole, UserClaim>, IEntity 
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, Guid> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here 
            return userIdentity;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UserId")]
        public override Guid Id { get; set; }
        [MaxLength(256)]
        [Index("IX_UserName", IsUnique = true)]
        public override string UserName
        {
            get { return base.UserName; }
            set { base.UserName = value; }
        }
        [MaxLength(256)]
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        [MaxLength(64)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(64)]
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }

        
    }
}
