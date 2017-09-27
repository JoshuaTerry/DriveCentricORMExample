using DriveCentric.Shared.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveCentric.Shared.Models
{
    [Table("Customers")]
    public class Customer : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        [MaxLength(256)]
        public string FirstName { get; set; }
        [MaxLength(256)]
        public string LastName { get; set; }
        [MaxLength(512)]
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }         
    }
}
