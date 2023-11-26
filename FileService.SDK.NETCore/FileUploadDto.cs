using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.SDK.NETCore
{
    public class FileUploadDto
    {
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
    }

}
