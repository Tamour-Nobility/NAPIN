using NPMSyncWorker.Enums;
using System;

namespace NPMSyncWorker.Entities
{
    internal class Patient
    {
        public long Patient_Account { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public DateTime Date_Of_Birth { get; set; }
        public eGender Gender { get; set; }
        public string Email_Address { get; set; }
    }
}
