using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDrive.REST.Model
{
    [DataContract]
    public class BoxFileList
    {
        [DataMember(Name = "total_count", EmitDefaultValue = false)]
        public int TotalCound { get; set; }

        [DataMember(Name = "offset", EmitDefaultValue = false)]
        public int Offset { get; set; }

        [DataMember(Name = "limit", EmitDefaultValue = false)]
        public int Limit { get; set; }

        [DataMember(Name = "entries", EmitDefaultValue = false)]
        public List<BoxFile> Entries { get; set; }
    }

    [DataContract]
    public class BoxFile
    {
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "mimeType", EmitDefaultValue = false)]
        public string MimeType { get; set; }
    }
}
