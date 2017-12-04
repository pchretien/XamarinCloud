using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDrive.REST.Model
{
    [DataContract]
    public class GoogleFileList
    {
        [DataMember(Name = "kind", EmitDefaultValue = false)]
        public string Kind { get; set; }

        [DataMember(Name = "nextPageToken", EmitDefaultValue = false)]
        public string NextPageToken { get; set; }

        [DataMember(Name = "incompleteSearch", EmitDefaultValue = false)]
        public bool Message { get; set; }

        [DataMember(Name = "files", EmitDefaultValue = false)]
        public List<GoogleFile> Files { get; set; }
    }

    [DataContract]
    public class GoogleFile
    {
        [DataMember(Name = "kind", EmitDefaultValue = false)]
        public string Kind { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "mimeType", EmitDefaultValue = false)]
        public string MimeType { get; set; }
    }
}
