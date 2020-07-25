using System;
using System.ComponentModel.DataAnnotations;

namespace MusicInfoCompletion.Data
{
    public class BaseModel
    {
        [Key]
        public virtual Guid Pk { get; set; }

        public string AddedBy { get; set; }

        public DateTime AddDate { get; set; }

        public string LastModifyBy { get; set; }

        public DateTime LastModifyDate { get; set; }
    }
}
