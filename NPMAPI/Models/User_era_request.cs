//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NPMAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User_era_request
    {
        public long Id { get; set; }
        public long USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public System.DateTime ENTRY_DATE { get; set; }
        public Nullable<long> PracticeCode { get; set; }
        public string LogFile_Name { get; set; }
        public string LogFile_Path { get; set; }
        public int DOWNLOADED_FILE_COUNT { get; set; }
        public string STATUS { get; set; }
        public string FTP_EXCEPTION { get; set; }
        public Nullable<int> T_Duplicate_Count { get; set; }
        public Nullable<int> Failed_Count { get; set; }
        public Nullable<int> Processed_File_Count { get; set; }
    }
}
