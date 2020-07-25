using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicInfoCompletion.Data
{
    public static class Utils
    {
        public static void SetDefaultAndComputedValues(this EntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.Property(nameof(BaseModel.AddDate)).UseMySqlIdentityColumn();
            entityTypeBuilder.Property(nameof(BaseModel.LastModifyDate)).UseMySqlComputedColumn();
        }
    }
}
