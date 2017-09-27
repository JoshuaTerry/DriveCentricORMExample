using DriveCentric.Shared.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace DriveCentric.Data.Conventions
{
    public class StringLengthRequiredConvention : Convention
    {
        public StringLengthRequiredConvention()
        {
            // Enforce MaxLength or Text attributes on string properties.
            Properties<string>()
                .Where(p => p.DeclaringType.Namespace.StartsWith("DriveCentric")
                                &&
                            !p.GetCustomAttributes(false).OfType<MaxLengthAttribute>().Any()
                                &&
                            !p.GetCustomAttributes(false).OfType<TextAttribute>().Any())
                .Configure(p => { throw new InvalidOperationException($"String max length is required: {p.ClrPropertyInfo.DeclaringType.Name}.{p.ClrPropertyInfo.Name}"); });

            // Text attribute => nvarchar(max)
            Properties<string>()
                .Where(p => p.GetCustomAttributes(false).OfType<TextAttribute>().Any()).Configure(p => p.IsMaxLength());
        }
    }
}
